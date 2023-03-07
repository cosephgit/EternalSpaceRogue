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
    bool levelUpOpen;

    public StageEndRound(StageManager stateMachine) : base("StageEndRound", stateMachine) {
      _sm = stateMachine;
    }

    public override void Enter()
    {
        levelUpOpen = false;
    }
    public override void UpdateLogic()
    {
        if (_sm.levelUpPending > 0)
        {
            if (!levelUpOpen)
            {
                // player has pending level ups, open the menu so they can choose these before the next round
                UIManager.instance.levelUpMenu.OpenLevelUps(_sm.levelUpPending);
                levelUpOpen = true;
            }
        }
        else
        {
            levelUpOpen = false;
            base.UpdateLogic();
            _sm.ChangeState(_sm.playerActiveStage);
        }
    }
    public override void Exit() { }
}
