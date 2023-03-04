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

public class StageManager : StateMachine
{
    public static StageManager instance;
    public StageState state { get; private set; } = StageState.StageInit;
    private List<EnemyPawn> enemyPawns; // enemy pawns are created during StageInit and stored in this list
    [field: SerializeField]public PlayerPawn playerPawn  { get; private set; } // player pawn is placed in the scene in the editor and stored here
    [SerializeField]public List<TilemapSegment> tilemapPrefabs; // these are used to generate the play area
    [SerializeField]public TilemapSegment tilemapPrefabEnd; // an endcap that can fit anywhere
    [SerializeField]public EnemyPawn[] enemyPrefabs; // enemies that may be spawned in a level
    [SerializeField]public PowerUpBase[] powerupPrefabs; // powerups that may be spawned in a level
    [SerializeField]public int tilemapSizeMin = 10; // once this many tilemaps are placed avoid tilemaps with lots of branches (2-3 exits)
    [SerializeField]public int tilemapSizeGood = 15; // start reducing the number of branches once this many tilemaps are placed (1-3 exits)
    [SerializeField]public int tilemapSizeMax = 20; // always use closed segments when this many tilemaps are placed (1 exit)
    public List<TilemapSegment> tilemapActive = new List<TilemapSegment>(); // a list of all the actual tilemaps in the level
    public List<NavNode> spawnPoints = new List<NavNode>();
    public NavNode[] navNodeMap;
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


    void WooblyWoo()
    {
        switch (state)
        {
            default:
            {
                break;
            }
            case StageState.PlayerActive:
            {
                // call the player pawn and allow it to act
                // when player pawn returns true, transition to PlayerWinCheck
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


    #if UNITY_EDITOR
    void OnGUI()
    {
        GUILayout.Label($"<color='black'><size=40>State: {currentState.name}</size></color>");
    }
    #endif
}
