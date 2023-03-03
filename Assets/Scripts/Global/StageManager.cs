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
        state = StageState.PlayerActive;
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
