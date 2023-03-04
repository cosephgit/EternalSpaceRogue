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
    [SerializeField]public int tilemapSizeMin = 10; // once this many tilemaps are placed avoid tilemaps with lots of branches (2-3 exits)
    [SerializeField]public int tilemapSizeGood = 15; // start reducing the number of branches once this many tilemaps are placed (1-3 exits)
    [SerializeField]public int tilemapSizeMax = 20; // always use closed segments when this many tilemaps are placed (1 exit)
    // generated stage details
    [HideInInspector]public List<TilemapSegment> tilemapActive = new List<TilemapSegment>(); // a list of all the actual tilemaps in the level
    [HideInInspector]public List<NavNode> spawnPoints = new List<NavNode>();
    [HideInInspector]public NavNode[] navNodeMap;
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

    #if UNITY_EDITOR
    void OnGUI()
    {
        GUILayout.Label($"<color='black'><size=40>State: {currentState.name}</size></color>");
    }
    #endif
}
