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

public enum StageState
{
    StageInit = 0,
    PlayerActive = 1,
    PlayerWinCheck = 2,
    EnemyActive = 3,
    PlayerLoseCheck = 4,
    EndRound = 5,
    StageComplete = 6,
    StageFailed = 7
}

public class StageManager : MonoBehaviour
{
    public static StageManager instance;
    public StageState state { get; private set; } = StageState.StageInit;
    private List<EnemyPawn> enemyPawns; // enemy pawns are created during StageInit and stored in this list
    [field: SerializeField]public PlayerPawn playerPawn  { get; private set; } // player pawn is placed in the scene in the editor and stored here
    [SerializeField]private List<TilemapSegment> tilemapPrefabs; // these are used to generate the play area
    [SerializeField]private TilemapSegment tilemapPrefabEnd; // an endcap that can fit anywhere
    [SerializeField]private int tilemapSizeMin = 10; // once this many tilemaps are placed avoid tilemaps with lots of branches (2-3 exits)
    [SerializeField]private int tilemapSizeGood = 15; // start reducing the number of branches once this many tilemaps are placed (1-3 exits)
    [SerializeField]private int tilemapSizeMax = 20; // always use closed segments when this many tilemaps are placed (1 exit)
    List<TilemapSegment> tilemapActive = new List<TilemapSegment>(); // a list of all the actual tilemaps in the level

    // Awake
    // set up singleton
    void Awake()
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

        // TEMP
        state = StageState.StageInit;
    }

    // removes any tilemaps in the stage
    void ClearTilemap()
    {
        for (int i = tilemapActive.Count - 1; i >= 0; i--)
        {
            GameObject destroyThis = tilemapActive[i].gameObject;
            // TODO destroy all occupants of the tilemap too!!!

            tilemapActive.Remove(tilemapActive[i]);
            Destroy(destroyThis);
        }
        tilemapActive = new List<TilemapSegment>();
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
        List<TilemapSegment> tilemapOptions = SelectTilemapsByExits(tilemapPrefabs, 3, 4);

        if (tilemapActive.Count > 0)
        {
            ClearTilemap();
        }

        // place the first tilemap segment at the origin
        TilemapSegment tilemapHub = Instantiate(tilemapOptions[Random.Range(0, tilemapOptions.Count)], Vector3.zero, Quaternion.identity);
        tilemapActive.Add(tilemapHub);

        while (complete == false && whileDump < 100)
        {
            List<TilemapSegment> tilemapIncomplete = new List<TilemapSegment>();
            TilemapSegment tilemapBase; // the current tilemap segment which will be used to connect the next segment to
            Vector3 tilePlaceOffset = new Vector3(); // the offset from the current tilemap segment to place the new tilemap segment on

            // make a list of all the tilemap sections that are not complete (not all exits have been closed)
            for (int i = 0; i < tilemapActive.Count; i++)
            {
                if (!tilemapActive[i].exitsDone)
                    tilemapIncomplete.Add(tilemapActive[i]);
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
                    tilemapConnected.Add(tilemapPrefabEnd); // for some reason (bug?) there are no acceptable connection tiles, use the generic end cap
                }

                if (Mathf.Approximately(tilePlaceOffset.magnitude, 0))
                {
                    Debug.Log("<color=orange>WARNING</color> tilePlace offset is zero vector!");
                }
                else
                {
                    Vector3 tilePlacePos = tilemapBase.transform.position + tilePlaceOffset * Global.TILEMAPDIMS;
                    TilemapSegment tilemapAdded = Instantiate(tilemapConnected[Random.Range(0, tilemapConnected.Count)], tilePlacePos, Quaternion.identity);

                    tilemapActive.Add(tilemapAdded);
                    tilemapAdded.BuildNavNodes();
                    if (tilemapAdded.ExitCount() <= 1) tilemapAdded.ExitsDone(); // will always be done when placed

                    if (tilemapActive.Count == tilemapSizeMin)
                    {
                        // once at least tilemapSizeMin are placed, reduce the amount of branching
                        tilemapOptions = SelectTilemapsByExits(tilemapPrefabs, 2, 4);
                    }
                    if (tilemapActive.Count == tilemapSizeGood)
                    {
                        // once at least tilemapSizeGood are placed, reduce the possible number of branches and start ending branches
                        tilemapOptions = SelectTilemapsByExits(tilemapPrefabs, 1, 3);
                    }
                    if (tilemapActive.Count == tilemapSizeMax)
                    {
                        // once at least tilemapSizeMax are placed, start closing off
                        tilemapOptions = SelectTilemapsByExits(tilemapPrefabs, 0, 1);
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

    void Update()
    {
        switch (state)
        {
            default:
            case StageState.StageInit:
            {
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
                state = StageState.PlayerActive;
                break;
            }
            case StageState.PlayerActive:
            {
                // call the player pawn and allow it to act
                // when player pawn returns true, transition to PlayerWinCheck
                playerPawn.PawnUpdate();
                break;
            }
            case StageState.PlayerWinCheck:
            {
                // check if the player has completed the stage completion criteria
                // transition to StageComplete if os
                // else transition to EnemyActive
                break;
            }
            case StageState.EnemyActive:
            {
                // build enemy list (take all enemies, make list of active enemies, sort by proximity to player so nearest go first)
                // loop through all active enemies completing their actions one by one
                // when an enemy pawn returns true, iterate to the next enemy in the active enemy list
                // if an enemy pawn returns true and it's the end of the list, transition to PlayerLoseCheck
                break;
            }
            case StageState.PlayerLoseCheck:
            {
                // check if player is dead (or some other end game conditions)
                // if defeated transition to StageFailed
                // else transition to EndRound
                break;
            }
            case StageState.EndRound:
            {
                // do any end round cleanup/checks
                // transition to PlayerActive
                break;
            }
            case StageState.StageComplete:
            {
                // show stage victorious UI
                // apply XP
                // when player acknowledges completion:
                // check for game completion, give player option for eternal play
                // if not complete or eternal play, proceed to next stage and advance difficulty
                // if complete and not eternal play, proceed to victory/credits/score scene
                break;
            }
            case StageState.StageFailed:
            {
                // show stage defeat UI
                // when player acknowledges defeat:
                // player selects either return to main menu or start over from the beginning
                break;
            }
        }
    }
}
