using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this FSM state should be entered when the current stage is complete
// a UI is shown indicating stage results
// it may show game victory if objectives are met
// the player may then continue playing or return to menu

public class StageComplete : BaseState
{
    protected StageManager _sm;

    public StageComplete(StageManager stateMachine) : base("StageComplete", stateMachine) {
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
