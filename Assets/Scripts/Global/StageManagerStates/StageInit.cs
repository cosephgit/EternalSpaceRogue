using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this FSM state is entered at the beginning of a stage
// it clears any existing tilemap and then generates a new tilemap
// enemies, loot and objectives are then added
// pathfinding data is built

public class StageInit : BaseState
{
    protected StageManager _sm;
    // these values are set up at the start of stage creation and are only used during init
    // they're copied into the stagemanager when they are set
    float initPower;
    float initEnemyTotal;
    float initEnemyIndividual;

    public StageInit(StageManager stateMachine) : base("StageInit", stateMachine) {
      _sm = stateMachine;
    }

    // calculate the power up and enemy strenghts for this stage
    void SetDifficulty()
    {
        initPower = _sm.powerStrengthBaseTotal * Mathf.Pow(_sm.stageCurrent + 1, Global.BONUSLOOTEXPONENT);
        if (_sm.playerPawn.upgradeSupply > 0)
        {
            initPower *= 1f + ((float)_sm.playerPawn.upgradeSupply * 0.2f);
        }
        initEnemyTotal = _sm.enemyStrengthBaseTotal * Mathf.Pow(_sm.stageCurrent + 1, Global.BONUSENEMIESEXPONENT);
        initEnemyIndividual = _sm.enemyStrengthBaseIndividual * Mathf.Pow(_sm.stageCurrent + 1, Global.BONUSENEMYEXPONENT);
        if (_sm.playerPawn.upgradeTerror > 0)
        {
            // face less enemy strength overall BUT the basic group strength will be bigger
            // the 20% in the UI is just an approximation
            float factor = 1f + ((float)_sm.playerPawn.upgradeTerror * 0.1f);
            initEnemyTotal /= factor;
            initEnemyIndividual *= factor;
        }

        _sm.powerPoints = initPower;
        _sm.enemyStrengthTotal = initEnemyTotal;
        _sm.enemyStrengthIndividual = initEnemyIndividual;
    }

    // removes any tilemaps in the stage
    void ClearTilemap()
    {
        for (int i = _sm.tilemapActive.Count - 1; i >= 0; i--)
        {
            GameObject destroyThis = _sm.tilemapActive[i].gameObject;
            // TODO destroy all occupants of the tilemap too!!!

            _sm.tilemapActive.Remove(_sm.tilemapActive[i]);
            GameObject.Destroy(destroyThis);
        }
        _sm.tilemapActive.Clear();
    }

    List<TilemapSegment> SelectTilemapsByExits(List<TilemapSegment> segmentList, int exitMin, int exitMax)
    {
        List<TilemapSegment> segmentListCleared = new List<TilemapSegment>();

        for (int i = 0; i < segmentList.Count; i++)
        {
            if (segmentList[i].ExitCount() <= exitMax && segmentList[i].ExitCount() >= exitMin)
            {
                segmentListCleared.Add(segmentList[i]);
            }
        }

        return segmentListCleared;
    }

    // called at the start of a stage to populate it with tilemap segments
    void BuildTilemap()
    {
        bool incomplete = true;
        bool objectiveNeeded = true;

        int whileDump = 0; // paranoid infinite loop protection
        List<TilemapSegment> tilemapOptions = SelectTilemapsByExits(_sm.tilemapPrefabs, 3, 4);

        if (_sm.tilemapActive.Count > 0)
        {
            ClearTilemap();
        }

        // place the first tilemap segment at the origin
        TilemapSegment tilemapHub = GameObject.Instantiate(_sm.tilemapEntrance, Vector3.zero, Quaternion.identity);
        _sm.tilemapActive.Add(tilemapHub);

        while (incomplete && whileDump < 100)
        {
            List<TilemapSegment> tilemapIncomplete = new List<TilemapSegment>();
            TilemapSegment tilemapBase = _sm.tilemapActive[0]; // the current tilemap segment which will be used to connect the next segment to
            Vector3 tilePlaceOffset = new Vector3(); // the offset from the current tilemap segment to place the new tilemap segment on
            bool startNeeded = true;
            int tileCheck = 0;

            while (startNeeded && tileCheck < _sm.tilemapActive.Count)
            {
                if (!_sm.tilemapActive[tileCheck].exitsDone)
                {
                    // take the first active tile in the list that has an exit unplaced
                    tilemapBase = _sm.tilemapActive[tileCheck];
                    startNeeded = false;
                }
                tileCheck++;
            }

            if (startNeeded)
            {
                // all exits are closed
                incomplete = false;
            }
            else
            {
                // a list of all tilemap segments that are acceptable and can connect to the current tilemapBase
                List<TilemapSegment> tilemapConnected = new List<TilemapSegment>();

                tilePlaceOffset = tilemapBase.PickAdjacentDirection();

                for (int i = 0; i < tilemapOptions.Count; i++)
                {
                    if (tilemapOptions[i].CanConnect(tilePlaceOffset))
                    {
                        tilemapConnected.Add(tilemapOptions[i]);
                    }
                }

                if (tilemapConnected.Count == 0)
                {
                    tilemapConnected.Add(_sm.tilemapPrefabEnd); // for some reason (bug?) there are no acceptable connection tiles, use the generic end cap
                }

                if (Mathf.Approximately(tilePlaceOffset.magnitude, 0))
                {
                    Debug.Log("<color=orange>WARNING</color> tilePlace offset is zero vector!");
                }
                else
                {
                    Vector3 tilePlacePos = tilemapBase.transform.position + tilePlaceOffset * Global.TILEMAPDIMS;
                    TilemapSegment tilemapAdded = GameObject.Instantiate(tilemapConnected[Random.Range(0, tilemapConnected.Count)], tilePlacePos, Quaternion.identity);

                    _sm.tilemapActive.Add(tilemapAdded);

                    if (objectiveNeeded && _sm.tilemapActive.Count >= _sm.tilemapSizeMax && (Global.OrthogonalDist(tilemapAdded.transform.position, Vector3.zero) >= _sm.tilemapExitDistMin))
                    {
                        // place the objective
                        objectiveNeeded = false;
                        ObjectiveZone objective = GameObject.Instantiate(_sm.objectiveZone, tilemapAdded.transform.position, Quaternion.identity, tilemapAdded.transform);
                    }

                    if (tilemapAdded.ExitCount() <= 1) tilemapAdded.ExitsDone(); // will always be done when placed

                    if (_sm.tilemapActive.Count == _sm.tilemapSizeMin)
                    {
                        // once at least tilemapSizeMin are placed, reduce the amount of branching
                        tilemapOptions = SelectTilemapsByExits(_sm.tilemapPrefabs, 2, 4);
                    }
                    if (_sm.tilemapActive.Count == _sm.tilemapSizeGood)
                    {
                        // once at least tilemapSizeGood are placed, reduce the possible number of branches and start ending branches
                        tilemapOptions = SelectTilemapsByExits(_sm.tilemapPrefabs, 1, 3);
                    }
                    if (_sm.tilemapActive.Count == _sm.tilemapSizeMax)
                    {
                        // once at least tilemapSizeMax are placed, start closing off
                        tilemapOptions = SelectTilemapsByExits(_sm.tilemapPrefabs, 0, 1);
                    }
                }
            }

            whileDump++;
        }

        if (objectiveNeeded)
        {
            if (_sm.tilemapActive.Count > 1)
            {
                // couldn't place it earlier, so just put it on the last placed tile
                objectiveNeeded = false;
                ObjectiveZone objective = GameObject.Instantiate(_sm.objectiveZone, _sm.tilemapActive[_sm.tilemapActive.Count - 1].transform.position, Quaternion.identity, _sm.tilemapActive[_sm.tilemapActive.Count - 1].transform);
            }
            else
            {
                Debug.Log("<color=red>ERROR</color> failed to place objective in BuildTilemap()");
            }
        }

        if (incomplete) Debug.Log("<color=red>ERROR</color> Dumped out of tilemap generation in BuildTilemap()");
    }

    // this collects all instantiated navnodes and stores them in an array for pathfinding
    // it also copies this list into the spawnpoint list (used for placing enemies, loot, etc)
    void BuildNavmap()
    {
        List<NavNode> navNodeCollector = new List<NavNode>();

        // place the navigation nodes
        for (int i = 0; i < _sm.tilemapActive.Count; i++)
        {
            navNodeCollector.AddRange(_sm.tilemapActive[i].BuildNavNodes());
        }

        _sm.navNodeMap = navNodeCollector.ToArray();

        #if UNITY_EDITOR
        Debug.Log("BuildNavmap found " + _sm.navNodeMap.Length + " NavNode objects");
        #endif
        // NOTE: this produces some 4000+ NavNodes in a typical stage build
        // so building full pathfinding data on each stage build is probably NOT viable
        // but pathfinding should never be needed between nodes more than (say) 20 nodes apart, so the pathfinding cost should be fairly low and will only be called every few seconds

        for (int i = 0; i < _sm.navNodeMap.Length; i++)
        {
            // build connectivity between all nodes
            _sm.navNodeMap[i].CheckConnection(i);
        }

        // set up the spawn points
        _sm.spawnPoints.Clear();
        _sm.spawnPoints.AddRange(_sm.navNodeMap);
    }

    // this randomises through the list of spawn points
    // spawn points are checked for rejection criteria (e.g. too close to the player or too close to another spawn) and spawn points are removed from the list piecemeal
    // like this rather than doing it all at once up front
    Vector3 SelectSpawnPoint()
    {
        NavNode spawnPoint;
        Vector3 result = Vector3.zero;

        if (_sm.spawnPoints.Count > 0)
        {
            bool needPoint = true;

            while (needPoint)
            {
                if (_sm.spawnPoints.Count > 0)
                {
                    spawnPoint = _sm.spawnPoints[Random.Range(0, _sm.spawnPoints.Count)];

                    _sm.spawnPoints.Remove(spawnPoint); // remove the entry from spawn points so nothing else is spawned there

                    // make sure it's not on screen
                    if (!_sm.PointOnScreen(spawnPoint.transform.position))
                    {
                        result = spawnPoint.transform.position;
                        needPoint  = false;
                    }
                    // else reject and try again (it's already been removed from the list so wont get picked again)
                }
                else
                {
                    Debug.Log("<color=orange>WARNING</color> trying to SelectSpawnPoint with no spawn points");
                    needPoint = false;
                }
            }
        }
        else
        {
            Debug.Log("<color=orange>WARNING</color> trying to SelectSpawnPoint with no spawn points");
        }

        return result;
    }

    void PopulateEnemies()
    {
        EnemyPawn enemySpawned;
        List<EnemyPawn> enemiesValid = _sm.EnemiesValid(); // note this also GENERATES the valid enemies list for the stage difficulty

        if (_sm.enemySpawns.Count > 0)
        {
            for (int i = _sm.enemySpawns.Count - 1; i >= 0; i--)
            {
                // if this has happened (which it shouldn't) we MIGHT have destroyed terrain tiles and taken the enemies with them already, so make sure they're valid first
                if (_sm.enemySpawns[i])
                {
                    GameObject.Destroy(_sm.enemySpawns[i].gameObject);
                }
            }
            _sm.enemySpawns.Clear();
        }

        while (initEnemyTotal > 0)
        {
            Vector3 pos = SelectSpawnPoint();

            if (pos.magnitude > 0)
            {
                // place the enemy
                enemySpawned = GameObject.Instantiate(enemiesValid[Random.Range(0, enemiesValid.Count)], pos, Quaternion.identity);
                _sm.enemySpawns.Add(enemySpawned);
                // set the enemy strength based on stage difficulty, and adjust the remaining stage enemy strength total by the result
                // TODO spawn "pods" of enemies if they're individually very weak on a later stage
                initEnemyTotal -= enemySpawned.SetStrength(Mathf.Min(initEnemyIndividual, initEnemyTotal));
            }
            else
            {
                // else there are no valid spawn points so stop trying to spawn (this SHOULD NOT HAPPEN!)
                initEnemyTotal = 0;
            }
        }
    }

    void PopulateLoot()
    {
        PowerUpBase powerupSpawned;

        while (initPower > 0)
        {
            Vector3 pos = SelectSpawnPoint();

            if (pos.magnitude > 0)
            {
                powerupSpawned = GameObject.Instantiate(_sm.powerupPrefabs[Random.Range(0, _sm.powerupPrefabs.Length)], pos, Quaternion.identity);
                initPower -= _sm.powerStrengthBaseIndividual;
            }
            else
            {
                // else there are no valid spawn points so stop trying to spawn (this SHOULD NOT HAPPEN!)
                initPower = 0;
            }
        }
    }

    public override void Enter()
    {
        // set up the stage
        // procedurally place:
        // level structure
        // enemies
        // loot boxes
        // level exit
        // traps and puzzle elements
        // when complete, transition to PlayerActive
    }
    public override void UpdateLogic()
    { 
        base.UpdateLogic();

        SetDifficulty();
        BuildTilemap();
        BuildNavmap();
        PopulateLoot();
        PopulateEnemies();

        if (_sm.levelUpPending > 0)
            _sm.ChangeState(_sm.roundEndStage);
        else
            _sm.ChangeState(_sm.playerActiveStage);
    }
    public override void Exit()
    {
    }
}
