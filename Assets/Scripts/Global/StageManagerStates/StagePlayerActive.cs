using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this FSM state is entered at the start of a game round
// it checks with the player pawn each frame, which can then take in player inputs
// it transitions to the playerWinCheck when the player finishes their go

public class StagePlayerActive : BaseState
{
    protected StageManager _sm;

    public StagePlayerActive(StageManager stateMachine) : base("StagePlayerActive", stateMachine) {
      _sm = stateMachine;
    }

    public override void Enter()
    {
        _sm.playerPawn.RoundPrep();
    }
    public override void UpdateLogic()
    {
        base.UpdateLogic();
        if (_sm.playerPawn.PawnUpdate())
        {
            _sm.ChangeState(_sm.playerWinCheckStage);
        }
    }
    public override void Exit() { }
}
