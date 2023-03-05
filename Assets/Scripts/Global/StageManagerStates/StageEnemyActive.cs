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
        Vector3 playerPos = _sm.playerPawn.transform.position; // going to use this a lot here

        enemyCurrent = 0;

        enemyActive.Clear();

        for (int i = 0; i < _sm.enemySpawns.Count; i++)
        {
            // check if each enemy is or should be alert and, if so, add them to the active list
            if (_sm.enemySpawns[i].CheckAlert(playerPos))
            {
                enemyActive.Add(_sm.enemySpawns[i]);
                _sm.enemySpawns[i].RoundPrep();
            }
        }
        for (int i = 0; i < enemyActive.Count; i++)
        {
            Collider2D[] enemyAdjacent = Physics2D.OverlapCircleAll(enemyActive[i].transform.position, 2.5f, Global.LayerPawn());

            for (int j = 0; j < enemyAdjacent.Length; j++)
            {
                EnemyPawn enemyPawn = enemyAdjacent[j].GetComponent<EnemyPawn>();
                if (enemyPawn)
                {
                    if (enemyPawn.MakeAlert()) // this returns true if the pawn wasn't alert, but is now made alert
                    {
                        enemyActive.Add(enemyPawn);
                    }
                }
            }
        }
        // TODO update the music volume with the number and strength of active enemies
        //enemyActive.Sort((p1,p2)=>p1.score.CompareTo(p2.score));
        enemyActive.Sort((p1,p2)=>p1.DistanceTo(playerPos).CompareTo(p2.DistanceTo(playerPos)));
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
