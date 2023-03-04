using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this FSM state is entered after the player finishes their go and victory has been checked
// it cycles through all enemies in the level, makes them active if inactive and they can see the player, and creates a list of all active enemies
// the active enemy list is sorted in order of distance closest to furthest from player
// it then activates them one by one and tells them to complete their actions
// it automatically passes to the playerLoseCheck when complete

public class StageEnemyActive : BaseState
{
    protected StageManager _sm;

    public StageEnemyActive(StageManager stateMachine) : base("StageEnemyActive", stateMachine) {
      _sm = stateMachine;
    }

    public override void Enter()
    {

    }
    public override void UpdateLogic()
    {
        base.UpdateLogic();
        _sm.ChangeState(_sm.playerLoseCheckStage);
    }
    public override void Exit() { }
}
