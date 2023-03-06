using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (Input.GetButtonDown("Fire1"))
        {
            // restart on Z
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else if (Input.GetButtonDown("Fire3"))
        {
            // revive player on C
            _sm.playerPawn.Heal(1000);
            _sm.ChangeState(_sm.playerActiveStage);
        }
    }
    public override void Exit() { }
}
