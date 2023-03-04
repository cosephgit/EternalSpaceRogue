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

    public StageInit(StageManager stateMachine) : base("StageInit", stateMachine) {
      _sm = stateMachine;
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
        _sm.tilemapActive = new List<TilemapSegment>();
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
        bool complete = false;
        int whileDump = 0; // paranoid infinite loop protection
        List<TilemapSegment> tilemapOptions = SelectTilemapsByExits(_sm.tilemapPrefabs, 3, 4);

        if (_sm.tilemapActive.Count > 0)
        {
            ClearTilemap();
        }

        // place the first tilemap segment at the origin
        TilemapSegment tilemapHub = GameObject.Instantiate(tilemapOptions[Random.Range(0, tilemapOptions.Count)], Vector3.zero, Quaternion.identity);
        _sm.tilemapActive.Add(tilemapHub);
        tilemapHub.BuildNavNodes();

        while (complete == false && whileDump < 100)
        {
            List<TilemapSegment> tilemapIncomplete = new List<TilemapSegment>();
            TilemapSegment tilemapBase; // the current tilemap segment which will be used to connect the next segment to
            Vector3 tilePlaceOffset = new Vector3(); // the offset from the current tilemap segment to place the new tilemap segment on

            // make a list of all the tilemap sections that are not complete (not all exits have been closed)
            for (int i = 0; i < _sm.tilemapActive.Count; i++)
            {
                if (!_sm.tilemapActive[i].exitsDone)
                    tilemapIncomplete.Add(_sm.tilemapActive[i]);
            }

            if (tilemapIncomplete.Count > 0)
            {
                // a list of all tilemap segments that are acceptable and can connect to the current tilemapBase
                List<TilemapSegment> tilemapConnected = new List<TilemapSegment>();

                tilemapBase = tilemapIncomplete[Random.Range(0, tilemapIncomplete.Count)]; // this is the base tile which the next tilemap is being placed from
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
                    tilemapAdded.BuildNavNodes();
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
            else
            {
                // all exits are closed
                complete = true;
            }

            whileDump++;
        }

        if (!complete) Debug.Log("<color=red>ERROR</color> Dumped out of tilemap generation in BuildTilemap()");
    }

    // this collects all instantiated navnodes and stores them in an array for pathfinding
    // it also copies this list into the spawnpoint list (used for placing enemies, loot, etc)
    void BuildNavmap()
    {
        _sm.navNodeMap = (NavNode[])Object.FindObjectsOfType(typeof(NavNode));
        // TODO build pathfindingdata? is this needed?
        // I think it DOES need connection data, without that pathfinding will take longer and this data will never change so is ok
        // but the pathing data may be redundant (as enemies will block movement)

        #if UNITY_EDITOR
        Debug.Log("BuildNavmap found " + _sm.navNodeMap.Length + " NavNode objects");
        #endif
        // NOTE: this produces some 4000+ NavNodes in a typical stage build
        // so building full pathfinding data on each stage build is probably NOT viable
        // but pathfinding should never be needed between nodes more than (say) 20 nodes apart, so the pathfinding cost should be fairly low and will only be called every few seconds

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
            int whileDump = 0;

            while (needPoint && whileDump < 100)
            {
                if (_sm.spawnPoints.Count > 0)
                {
                    Vector3 offset;

                    spawnPoint = _sm.spawnPoints[Random.Range(0, _sm.spawnPoints.Count)];

                    offset = _sm.playerPawn.transform.position - spawnPoint.transform.position;

                    _sm.spawnPoints.Remove(spawnPoint); // remove this entry from spawn points so nothing else is spawned there

                    if (Mathf.Abs(offset.x) > GameManager.instance.screenCellWidth + 2
                        || Mathf.Abs(offset.y) > GameManager.instance.screenCellHeight + 2)
                    {
                        result = spawnPoint.transform.position;
                        needPoint  = false;
                    }
                    else
                    {
                        // reject and try again
                        whileDump++;
                    }
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
        float stageStrength = 100; // the number of strength points of enemies to spawn in the stage
        float stageStrengthIndividual = 1; // the target strength of each individual enemy

        while (stageStrength > 0)
        {
            Vector3 pos = SelectSpawnPoint();

            if (pos.magnitude > 0)
            {
                enemySpawned = GameObject.Instantiate(_sm.enemyPrefabs[Random.Range(0, _sm.enemyPrefabs.Length)], pos, Quaternion.identity);
                stageStrength -= enemySpawned.SetStrength(Mathf.Min(stageStrengthIndividual, stageStrength));
            }
            else
            {
                // else there are no valid spawn points so stop trying to spawn (this SHOULD NOT HAPPEN!)
                stageStrength = 0;
            }
        }
    }

    void PopulateLoot()
    {
        PowerUpBase powerupSpawned;
        float stagePower = 20; // the number of points of powerups to spawn in the stage
        float stagePowerIndividual = 1; // the target power of each powerup

        while (stagePower > 0)
        {
            Vector3 pos = SelectSpawnPoint();

            if (pos.magnitude > 0)
            {
                powerupSpawned = GameObject.Instantiate(_sm.powerupPrefabs[Random.Range(0, _sm.powerupPrefabs.Length)], pos, Quaternion.identity);
                stagePower -= stagePowerIndividual;
            }
            else
            {
                // else there are no valid spawn points so stop trying to spawn (this SHOULD NOT HAPPEN!)
                stagePower = 0;
            }
        }
    }

    void PlaceObjective()
    {
        Debug.Log("<color=red>TODO</color> place objective");
    }

    public override void Enter()
    {

    }
    public override void UpdateLogic()
    { 
        base.UpdateLogic();
        // TODO
        // set up the stage
        // procedurally place:
        // level structure
        // enemies
        // loot boxes
        // level exit
        // traps and puzzle elements
        // when complete, transition to PlayerActive
        BuildTilemap();
        BuildNavmap();
        PopulateEnemies();
        PopulateLoot();
        PlaceObjective();

        _sm.ChangeState(_sm.playerActiveStage);
    }
    public override void Exit() { }
}
