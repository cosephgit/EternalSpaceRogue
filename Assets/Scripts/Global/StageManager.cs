using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// the stage manager controls the core gameplay loop
// it tracks the current game state and triggers pawns to be active
// game states:
// initialising stage
// MAIN LOOP
// player pawn active
// check for stage completion
// enemy pawns active (loop through all active enemies)
// check for player defeat
// end of round checks
// REPEAT LOOP
// stage completed (start next stage)
// player defeated (start over)

// SINGLETON STRUCTURE
// FINITE STATE MACHINE

public class StageManager : StateMachine
{
    public static StageManager instance;
    [field: SerializeField]public PlayerPawn playerPawn { get; private set; } // player pawn is placed in the scene in the editor and stored here
    [field: SerializeField]public List<TilemapSegment> tilemapPrefabs { get; private set; } // these are used to generate the play area
    [field: SerializeField]public TilemapSegment tilemapPrefabEnd { get; private set; } // an endcap that can fit anywhere
    [field: SerializeField]public EnemyPawn[] enemyPrefabs { get; private set; } // enemies that may be spawned in a level
    [field: SerializeField]public PowerUpBase[] powerupPrefabs { get; private set; } // powerups that may be spawned in a level
    [field: SerializeField]public ObjectiveZone objectiveZone { get; private set; } // zone markers for objectives
    [field: SerializeField]public int tilemapSizeMin { get; private set; } = 10; // once this many tilemaps are placed avoid tilemaps with lots of branches (2-3 exits)
    [field: SerializeField]public int tilemapSizeGood { get; private set; } = 15; // start reducing the number of branches once this many tilemaps are placed (1-3 exits)
    [field: SerializeField]public int tilemapSizeMax { get; private set; } = 20; // always use closed segments when this many tilemaps are placed (1 exit)
    [field: SerializeField]public int tilemapExitDistMin { get; private set; } = 140; // if a tile is at least this far from the start it's eligible for early goal placement
    [field: Header("Enemy parameters")]
    [field: SerializeField]public float enemyStrengthBaseTotal { get; private set; } = 50f; // the basic amount of enemy strength in a level
    [field: SerializeField]public float enemyStrengthBaseIndividual { get; private set; } = 1f; // the basic amount of strength per enemy in a level
    [field: SerializeField]public float enemyStrengthAboveAverage { get; private set; } = 3f; // the maximum amount of strength about the individual value that an enemy may be at most
    [field: SerializeField]public float enemyShoutDistance { get; private set; } = 3.5f; // distance enemies shout if they see the player
    [field: Header("Powerup parameters")]
    [field: SerializeField]public float powerStrengthBaseTotal { get; private set; } = 20f; // the basic amount of powerup strength in a level
    [field: SerializeField]public float powerStrengthBaseIndividual { get; private set; } = 1f; // the basic amount of strength per powerup in a level
    // generated stage details
    [HideInInspector]public List<TilemapSegment> tilemapActive = new List<TilemapSegment>(); // a list of all the actual tilemaps in the level
    [HideInInspector]public List<NavNode> spawnPoints = new List<NavNode>();
    [HideInInspector]public NavNode[] navNodeMap;
    [HideInInspector]public List<NavNode> navNodeDirty = new List<NavNode>();
    [HideInInspector]public List<EnemyPawn> enemiesValid = new List<EnemyPawn>(); // list if valid spawn types in this level
    [HideInInspector]public List<EnemyPawn> enemySpawns = new List<EnemyPawn>(); // actual list of spawned enemies in the stage
    // FSM states
    [HideInInspector]public StageInit initStage { get; private set; } // initial state, sets up the stage
    [HideInInspector]public StagePlayerActive playerActiveStage { get; private set; }
    [HideInInspector]public StagePlayerWinCheck playerWinCheckStage { get; private set; }
    [HideInInspector]public StageEnemyActive enemyActiveStage { get; private set; }
    [HideInInspector]public StagePlayerLoseCheck playerLoseCheckStage { get; private set; }
    [HideInInspector]public StageEndRound roundEndStage { get; private set; }
    [HideInInspector]public StageComplete stageCompleteStage { get; private set; }
    [HideInInspector]public StageFailed stageFailedStage { get; private set; }
    // persistent stage state factors
    [HideInInspector]public float enemyStrengthTotal; // unspent power points in the stage, updated with each power up
    [HideInInspector]public float EnemyStrengthIndividual; // unspent power points in the stage, updated with each power up
    [HideInInspector]public float powerPoints; // unspent power points in the stage, updated with each power up
    [HideInInspector]public int gameState { get; private set; } = 0; // 0: normal, 1: victory, 2: defeat
    [HideInInspector]public int levelUpPending { get; private set; } = 0;
    [HideInInspector]public int stageCurrent { get; private set; } = 0;
    bool menuOpen = false;
    Vector3 playerPosInitial;

    // Awake
    // set up singleton
    protected override void Awake()
    {
        if (instance)
        {
            if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        else instance = this;

        // create FSM states
        initStage = new StageInit(this); // initial state, sets up the stage
        playerActiveStage = new StagePlayerActive(this);
        playerWinCheckStage = new StagePlayerWinCheck(this);
        enemyActiveStage = new StageEnemyActive(this);
        playerLoseCheckStage = new StagePlayerLoseCheck(this);
        roundEndStage = new StageEndRound(this);
        stageCompleteStage = new StageComplete(this);
        stageFailedStage = new StageFailed(this);
        playerPosInitial = playerPawn.transform.position;
        menuOpen = false;
        base.Awake();
    }


    // clean up the stage and put the player back at the start point
    public void NewStage()
    {
        for (int i = 0; i < enemySpawns.Count; i++)
        {
            if (enemySpawns[i])
                enemySpawns[i].SilentRemove();
        }
        enemySpawns.Clear();
        for (int i = 0; i < navNodeMap.Length; i++)
        {
            if (navNodeMap[i])
                Destroy(navNodeMap[i].gameObject);
        }
        spawnPoints.Clear();
        navNodeDirty.Clear();

        //tilemapActive
        for (int i = 0; i < tilemapActive.Count; i++)
        {
            if (tilemapActive[i])
                Destroy(tilemapActive[i].gameObject);
        }
        tilemapActive.Clear();

        // need to get ALL POWERUPS TOO!!!
        PowerUpBase[] powerupsAll = (PowerUpBase[])GameObject.FindObjectsOfType(typeof(PowerUpBase));
        for (int i = 0; i < powerupsAll.Length; i++)
        {
            Destroy(powerupsAll[i].gameObject);
        }

        playerPawn.transform.position = playerPosInitial;

        // TODO increment difficulty
        stageCurrent++;
        Debug.Log("Stage cleared - new stage difficulty " + stageCurrent);
        gameState = 0;
        menuOpen = false;

        ChangeState(GetInitialState());
    }

    protected override BaseState GetInitialState()
    {
        return initStage;
    }

    // this is called by an enemy when it is spawned to have it de-listed and give the player xp
    // the enemy destroys itself after calling this
    public void EnemyDead(EnemyPawn enemy, float strength)
    {
        enemySpawns.Remove(enemy);
        playerPawn.AddXP(Mathf.CeilToInt(strength * Global.XPPERSTRENGTH));
    }


    // returns a list of all enemies that are valid for the current difficulty
    public List<EnemyPawn> EnemiesValid()
    {
        EnemyPawn enemyWeakest = enemyPrefabs[0];
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            if (enemyPrefabs[i].enemyStrength < enemyWeakest.enemyStrength) enemyWeakest = enemyPrefabs[i];
            if (enemyPrefabs[i].enemyStrength <= enemyStrengthBaseIndividual + enemyStrengthAboveAverage)
                enemiesValid.Add(enemyPrefabs[i]);
        }
        if (!enemiesValid.Contains(enemyWeakest)) enemiesValid.Add(enemyWeakest); // ensure that at least the weakest enemy is added

        return enemiesValid;
    }

    // returns true if there are still half of the power points left in the level
    public bool PowerUpPlenty()
    {
        return (powerPoints > (powerStrengthBaseTotal * 0.5f));
    }
    // update the remaining power points in the level
    public void PowerUpSpawned(float quality)
    {
        powerPoints -= quality;
    }

    // called when a trap triggers another enemy to spawn
    public void EnemySpawn(Vector3 pos)
    {
        if (enemiesValid.Count > 0)
        {
            EnemyPawn spawn = Instantiate(enemiesValid[Random.Range(0, enemiesValid.Count)], pos, Quaternion.identity);
            spawn.SetStrength(enemyStrengthBaseIndividual);
            powerPoints += spawn.enemyStrength; // when a trap happens, the remaining powerups on the level get a little boost
            enemySpawns.Add(spawn); // add it to the active enemies
        }
    }

    void CleanNavNodes()
    {
        for (int i = 0; i < navNodeDirty.Count; i++)
        {
            navNodeDirty[i].PathClear();
        }
        navNodeDirty.Clear();
    }

    // pathfinds from origin to target
    // range is the total travel distance to the target
    // returns the result as a Vector3 list of moves to reach the target IN REVERSE ORDER
    // an empty list means no path could be found OR the path is too long
    public List<Vector3> Pathfind(Vector3 origin, Vector3 target, bool acceptBlocked = true, int distMax = Global.PATHFINDMAX)
    {
        Collider2D originNodeCollider = Physics2D.OverlapPoint(origin, Global.LayerNav());
        Collider2D targetNodeCollider = Physics2D.OverlapPoint(target, Global.LayerNav());
        NavNode originNode = null;
        NavNode targetNode = null;
        int fullHDist = Global.OrthogonalDist(target, origin);
        List<Vector3> result = new List<Vector3>();
        List<Vector3> testList = new List<Vector3>();

        if (originNodeCollider) originNode = originNodeCollider.GetComponent<NavNode>();
        if (targetNodeCollider) targetNode = targetNodeCollider.GetComponent<NavNode>();

        if (fullHDist > distMax) // the shortest possible path is too long
        {
            return result;
        }
        else if (originNode && targetNode)
        {
            if (originNode == targetNode) return result;

            List<Vector3> resultDirect = new List<Vector3>(); // this is used to make a direct path, ignoring pawn obstacles, so enemies stack up if they can't find an open route or if the open route is too long

            CleanNavNodes();
            // build the pathfinding data to the target
            originNode.PathFind(targetNode, null, true, distMax);
            testList = BuildPath(targetNode);
            if (testList.Count > 0) result.AddRange(testList);

            if (acceptBlocked)
            {
                CleanNavNodes();
                originNode.PathFind(targetNode, null, false, distMax);
                testList = BuildPath(targetNode);
                if (testList.Count > 0) resultDirect.AddRange(testList);
            }

            // so theoretically we have a route to the target created now
            // it is in reverse order from LAST move to FIRST move

            if (result.Count > distMax || result.Count == 0 || (acceptBlocked && resultDirect.Count * 4 < result.Count))
            {
                // EITHER: the open route is too long, there is no open route, or the open route is over 4x the length of the direct route
                // this will allow for enemies that try to wrap around the player in melee, but wont go on a massive diversion to find an open route

                result.Clear();

                if (acceptBlocked)
                {
                    // then try to accept a path that is not clear
                    if (resultDirect.Count > 0 && resultDirect.Count <= distMax)
                    {
                        result = resultDirect;
                    }
                }
            }

            return result;
        }
        else
        {
            // something has gone wrong! a pawn is in a position that lacks a navnode
            Debug.LogError("<color=red>ERROR</color> NavNode not present at pawn or target pawn position");
        }

        return result;
    }

    // the START of the returned list will be the LAST move required to reach the target
    // each entry in the list is an integer indicating a direction required for that move
    List<Vector3> BuildPath(NavNode target)
    {
        List<Vector3> path = new List<Vector3>();
        List<Vector3> pathSegment = new List<Vector3>();
        Vector3 testPath;
        // don't need to add the target square to the list - we just want to get adjacent! 
        // TODO later on we WILL want to get into the target for objectives/switches/etc but not needed right now
        if (target.pathPrev)
        {
            testPath = target.PathDirFrom(target.pathPrev);
            if (testPath == Vector3.zero)
            {
                Debug.Log("BuildPath failed with invalid testpath");
                return null;
            }
            path.Add(testPath);
            pathSegment = BuildPath(target.pathPrev);
            if (pathSegment == null)
            {
            }
            else
            {
                path.AddRange(pathSegment);
            }
            return path;
        }
        else
        {
            return path;
        }
    }

    protected override void Update()
    {
        if (menuOpen)
        {
            if (Input.GetButtonDown("Cancel"))
            {
                // close menu
                UIManager.instance.pauseMenu.PressReturn();
            }
            return;
        }
        else if (Input.GetButtonDown("Cancel"))
        {
            // open menu
            OpenMenu();
            return;
        }
        base.Update();
    }

    void OpenMenu()
    {
        menuOpen = true;
        UIManager.instance.pauseMenu.MenuOpen(gameState);
    }

    // called by the menu when the continue button is pressed and the menu closed
    public void MenuClosed()
    {
        menuOpen = false;
    }

    // the player has reached the objective zone! end the stage
    public void ObjectiveReached(int xp)
    {
        playerPawn.AddXP(xp);
        ChangeState(stageCompleteStage);
        gameState = 1;
        OpenMenu();
    }

    // do level up stuff - open the selection menu
    // need to avoid interrupting the round order for this - make sure it is only triggered at the end of a round?
    public void LevelGain()
    {
        levelUpPending++; // just in case the player gets multiple level ups before they get to the menu
    }

    // user by the level up menu to notify the stage manager that all level ups are completed and the menu has been closed, gameplay can continue
    public void LevelUpsDone()
    {
        levelUpPending = 0;
    }

    // the player has upgraded their terror skill - increase the remaining supply
    // this should ONLY EVER GET CALLED during StageEndRound()
    public void UpgradeSupply(float increment)
    {
        if (increment > 1)
        {
            // increase the supply remaining by the increment, the player will get a little luckier for the rest of the level
            powerPoints += (powerStrengthBaseTotal * (increment - 1));
        }
    }

    // the player has upgraded their terror skill - remove some (invisible) enemies
    // this should ONLY EVER GET CALLED during StageEndRound()
    public void UpgradeTerror(float increment)
    {
        if (increment > 1)
        {
            float disappearChance = 1 / Mathf.Pow(increment, 2);
            // remove any weaker spawns by the increment
            for (int i = enemySpawns.Count - 1; i >= 0; i--)
            {
                // check all spawns in the level
                // if they're not in view, and they're weak, remove them from the level (and all lists)
                if (enemySpawns[i].actualStrength < enemyStrengthBaseIndividual)
                {
                    if (!PointOnScreen(enemySpawns[i].transform.position))
                    {
                        if (Random.Range(0, 1) < disappearChance)
                        {
                            enemySpawns[i].SilentRemove();
                            enemySpawns.Remove(enemySpawns[i]);
                        }
                    }
                }
            }
        }
    }


    // returns true if the indicated point is visible on the screen (with a bit of buffer to allow for sprite size)
    public bool PointOnScreen(Vector3 point)
    {
        Vector3 offset = playerPawn.transform.position - point;

        if (Mathf.Abs(offset.x) < GameManager.instance.screenCellWidth + 2
            && Mathf.Abs(offset.y) < GameManager.instance.screenCellHeight + 2)
        {
            return true;
        }
        return false;
    }

    public void PlayerDefeated()
    {
        ChangeState(stageFailedStage);
        gameState = 2;
        OpenMenu();
    }

    #if UNITY_EDITOR
    void OnGUI()
    {
        GUILayout.Label($"<color='yellow'><size=40>State: {currentState.name}</size></color>");
    }
    #endif
}
