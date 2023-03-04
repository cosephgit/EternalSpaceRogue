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
    int enemyCurrent = 0;
    List<EnemyPawn> enemyActive = new List<EnemyPawn>();

    public StageEnemyActive(StageManager stateMachine) : base("StageEnemyActive", stateMachine) {
      _sm = stateMachine;
    }

    public override void Enter()
    {
        enemyCurrent = 0;

        enemyActive.Clear();

        for (int i = 0; i < _sm.enemySpawns.Count; i++)
        {
            // check if each enemy is or should be alert and, if so, add them to the active list
            if (_sm.enemySpawns[i].IsAlert(_sm.playerPawn.transform.position))
                enemyActive.Add(_sm.enemySpawns[i]);
        }

        Debug.Log("Alert enemies " + enemyActive.Count);
    }
    public override void UpdateLogic()
    {
        base.UpdateLogic();

        // create a list of all enemies
        // if they're not active, check if they can see the player and if so make them active
        // add all active or newly active enemies to a list
        // sort the list based on their proximity to the player
        // then iterate through the last activating enemies one at a time
        // TODO add idle enemy wandering around behaviour

        Debug.Log("Checking enemy " + enemyCurrent);

        if (enemyCurrent >= enemyActive.Count)
        {
            // out of enemies, end the phase
            _sm.ChangeState(_sm.playerLoseCheckStage);
        }
        else if (!enemyActive[enemyCurrent] || !enemyActive[enemyCurrent].IsAlive())
        {
            // the enemy has been deleted or died (possibly self-destructed), iterate
            enemyCurrent++;
        }
        else if (enemyActive[enemyCurrent].PawnUpdate())
        {
            // if returned true, the enemy has finished their go, iterate to the next one
            enemyCurrent++;
        }
    }
    public override void Exit() { }
}
