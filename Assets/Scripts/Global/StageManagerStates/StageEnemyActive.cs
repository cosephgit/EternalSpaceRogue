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

        // check the list of all enemies
        // if they're not active, check if they can see the player and if so make them active
        // add all active or newly active enemies to a list
        // sort the list based on their proximity to the player

        enemyActive.Clear();

        for (int i = 0; i < _sm.enemySpawns.Count; i++)
        {
            // check if each enemy is or should be alert and, if so, add them to the active list
            if (_sm.enemySpawns[i].CheckAlert(playerPos))
            {
                enemyActive.Add(_sm.enemySpawns[i]);
            }
        }
        for (int i = 0; i < enemyActive.Count; i++)
        {
            Collider2D[] enemyAdjacent = Physics2D.OverlapCircleAll(enemyActive[i].transform.position, _sm.enemyShoutDistance, Global.LayerPawn());

            for (int j = 0; j < enemyAdjacent.Length; j++)
            {
                EnemyPawn enemyPawn = enemyAdjacent[j].GetComponent<EnemyPawn>();
                if (enemyPawn)
                {
                    if (enemyPawn.MakeAlert()) // this returns true if the pawn wasn't alert, but is now made alert
                    {
                        enemyActive.Add(enemyPawn); // so it can be added to the active list
                    }
                }
            }
        }
        // TODO update the music volume with the number and strength of active enemies
        enemyCurrent = 0;
        if (enemyActive.Count > 0)
        {
            enemyActive.Sort((p1,p2)=>p1.DistanceTo(playerPos).CompareTo(p2.DistanceTo(playerPos)));

            // get the first enemy in the que ready for the round
            enemyActive[enemyCurrent].RoundPrep();
        }
    }
    public override void UpdateLogic()
    {
        bool enemyNext = false;
        base.UpdateLogic();

        // iterate through active enemies and move/attack each of them in turn
        // TODO add idle enemy wandering around behaviour

        if (enemyCurrent >= enemyActive.Count)
        {
            // out of enemies, end the phase
            _sm.ChangeState(_sm.playerLoseCheckStage);
        }
        else if (!enemyActive[enemyCurrent] || !enemyActive[enemyCurrent].IsAlive())
        {
            // the enemy has been deleted or died (possibly self-destructed), iterate
            enemyNext = true;
        }
        else if (enemyActive[enemyCurrent].PawnUpdate())
        {
            // if returned true, the enemy has finished their go, iterate to the next one
            enemyNext = true;
        }
        if (enemyNext)
        {
            enemyCurrent++;
            if (enemyCurrent < enemyActive.Count && enemyActive[enemyCurrent])
                enemyActive[enemyCurrent].RoundPrep(); // prep the next enemy for their round, if they exist
        }
    }
    public override void Exit() { }
}
