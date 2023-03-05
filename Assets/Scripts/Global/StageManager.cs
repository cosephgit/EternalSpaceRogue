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
    private List<EnemyPawn> enemyPawns; // enemy pawns are created during StageInit and stored in this list
    [field: SerializeField]public PlayerPawn playerPawn  { get; private set; } // player pawn is placed in the scene in the editor and stored here
    [SerializeField]public List<TilemapSegment> tilemapPrefabs; // these are used to generate the play area
    [SerializeField]public TilemapSegment tilemapPrefabEnd; // an endcap that can fit anywhere
    [SerializeField]public EnemyPawn[] enemyPrefabs; // enemies that may be spawned in a level
    [SerializeField]public PowerUpBase[] powerupPrefabs; // powerups that may be spawned in a level
    [SerializeField]public ObjectiveZone objectiveZone; // zone markers for objectives
    [SerializeField]public int tilemapSizeMin = 10; // once this many tilemaps are placed avoid tilemaps with lots of branches (2-3 exits)
    [SerializeField]public int tilemapSizeGood = 15; // start reducing the number of branches once this many tilemaps are placed (1-3 exits)
    [SerializeField]public int tilemapSizeMax = 20; // always use closed segments when this many tilemaps are placed (1 exit)
    // generated stage details
    [HideInInspector]public List<TilemapSegment> tilemapActive = new List<TilemapSegment>(); // a list of all the actual tilemaps in the level
    [HideInInspector]public List<NavNode> spawnPoints = new List<NavNode>();
    [HideInInspector]public NavNode[] navNodeMap;
    [HideInInspector]public List<EnemyPawn> enemySpawns = new List<EnemyPawn>(); // actual list of spawned enemies in the stage
    // FSM states
    [HideInInspector]public StageInit initStage; // initial state, sets up the stage
    [HideInInspector]public StagePlayerActive playerActiveStage;
    [HideInInspector]public StagePlayerWinCheck playerWinCheckStage;
    [HideInInspector]public StageEnemyActive enemyActiveStage;
    [HideInInspector]public StagePlayerLoseCheck playerLoseCheckStage; 
    [HideInInspector]public StageEndRound roundEndStage;
    [HideInInspector]public StageComplete stageCompleteStage;
    [HideInInspector]public StageFailed stageFailedStage;


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

        base.Awake();
    }

    protected override BaseState GetInitialState()
    {
        return initStage;
    }

    // this is called by an enemy when it is spawned to have it de-listed and give the player xp
    // the enemy destroys itself after calling this
    public void EnemyDead(EnemyPawn enemy, int xpGain)
    {
        enemySpawns.Remove(enemy);
        playerPawn.AddXP(xpGain);
    }

    // pathfinds from origin to target
    // range is the total travel distance to the target
    // returns the result as a Vector3 list of moves to reach the target IN REVERSE ORDER
    // an empty list means no path could be found OR the path is too long
    public List<Vector3> Pathfind(Vector3 origin, Vector3 target)
    {
        Collider2D originNodeCollider = Physics2D.OverlapPoint(origin, Global.LayerNav());
        NavNode originNode = originNodeCollider.GetComponent<NavNode>();
        Collider2D targetNodeCollider = Physics2D.OverlapPoint(target, Global.LayerNav());
        NavNode targetNode = targetNodeCollider.GetComponent<NavNode>();
        Vector3 offset = target - origin;
        float fullHDist = offset.magnitude;
        List<Vector3> result = new List<Vector3>();

        if (fullHDist > Global.PATHFINDMAX)
        {
            return result;
        }
        else if (originNode && targetNode)
        {
            for (int i = 0; i < navNodeMap.Length; i++)
            {
                navNodeMap[i].PathClear();
            }
            // build the pathfinding data to the target
            originNode.PathFind(targetNode);
            result.AddRange(BuildPath(targetNode));

            // so theoretically we have a route to the target created now
            // it is in reverse order from LAST move to FIRST move

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
            return null;
        }
    }

    #if UNITY_EDITOR
    void OnGUI()
    {
        GUILayout.Label($"<color='yellow'><size=40>State: {currentState.name}</size></color>");
    }
    #endif
}
