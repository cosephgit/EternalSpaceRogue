using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this FSM state is entered at the end of a game round after all actions are completed
// it should do any cleaning up that is required
// no specific purpose expected, but will probably be needed
// it automatically switches to the player active state when complete

public class StageEndRound : BaseState
{
    protected StageManager _sm;

    public StageEndRound(StageManager stateMachine) : base("StageEndRound", stateMachine) {
      _sm = stateMachine;
    }

    public override void Enter()
    {

    }
    public override void UpdateLogic()
    {
        base.UpdateLogic();
        _sm.ChangeState(_sm.playerActiveStage);
    }
    public override void Exit() { }
}
