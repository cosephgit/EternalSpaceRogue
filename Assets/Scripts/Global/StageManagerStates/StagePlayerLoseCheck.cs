using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this FSM state is entered after enemies are active
// it checks for defeat conditions and transitions to stageFailed if so
// else it transitions to the roundEndStage

public class StagePlayerLoseCheck : BaseState
{
    protected StageManager _sm;

    public StagePlayerLoseCheck(StageManager stateMachine) : base("StagePlayerLoseCheck", stateMachine) {
      _sm = stateMachine;
    }

    public override void Enter()
    {

    }
    public override void UpdateLogic()
    {
        base.UpdateLogic();
        _sm.ChangeState(_sm.roundEndStage);
    }
    public override void Exit() { }
}
