using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this FSM state is entered when the player is defeated
// it displays a UI showing the results of the stage
// the player may then start again or return to the main menu

public class StageFailed : BaseState
{
    protected StageManager _sm;

    public StageFailed(StageManager stateMachine) : base("StageFailed", stateMachine) {
      _sm = stateMachine;
    }

    public override void Enter()
    {

    }
    public override void UpdateLogic()
    {
        base.UpdateLogic();
    }
    public override void Exit() { }
}
