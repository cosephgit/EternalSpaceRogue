using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this FSM state is entered after the player finishes their go
// it checks if the player has met victory conditions and transitions to stageComplete if so
// otherwise it transitions to the enemyActiveStage

public class StagePlayerWinCheck : BaseState
{
    protected StageManager _sm;

    public StagePlayerWinCheck(StageManager stateMachine) : base("StagePlayerWinCheck", stateMachine) {
      _sm = stateMachine;
    }

    public override void Enter()
    {

    }
    public override void UpdateLogic()
    {
        base.UpdateLogic();
        _sm.ChangeState(_sm.enemyActiveStage);
    }
    public override void Exit() { }
}
