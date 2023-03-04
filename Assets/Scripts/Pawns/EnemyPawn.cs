using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPawn : PawnControllerBase
{
    int enemyXP = 1; // XP award for defeating this enemy
    bool alert;

    // this is called after an enemy is instantiated
    // it attempts to calibrate the enemy strength (damage, health, speed, etc) to the provided targetStrength
    // it returns the ACTUAL strength of the enemy (resulting from e.g. rounding) for the StageManager to adjust the total stage balancing
    public float SetStrength(float targetStrength)
    {
        float actualStrength = targetStrength;

        // TODO actual strength calculation and balancing

        return actualStrength;
    }

    protected override void Death()
    {
        base.Death();
        StageManager.instance.EnemyDead(this, enemyXP);
        Destroy(gameObject);
    }

    // TODO
    // use action points to move to player
    // once action points are spent, switch to attack mode
    // if player is in position to attack, attack them
    public override bool PawnUpdate()
    {
        if (moving) return false;

        if (movePoints == 0 && moveActionDone) return true;

        Vector3 targetOffset = StageManager.instance.playerPawn.transform.position - transform.position;

        if (movePoints > 0)
        {
            // TODO pathfind to target
            movePoints--;
        }
        else
        {
            // check for attack
            if (targetOffset.magnitude == 1)
            {
                attackFacing = targetOffset;
                attackRange = 1;
                StartCoroutine(Attack());
            }
            moveActionDone = true; // target is out of reach, end go
        }

        return false;
    }

    bool CanSeePlayer(Vector3 playerPos)
    {
        Vector3 offset = playerPos - transform.position;

        if (Mathf.Abs(offset.x) < GameManager.instance.screenCellWidth
            && Mathf.Abs(offset.y) < GameManager.instance.screenCellHeight)
            return true;

        return false;
    }

    // this checks if this enemy is already active OR if not, if they can see the player pos (passed in for convenience) and so can become active
    public bool IsAlert(Vector3 playerPos)
    {
        if (!IsAlive()) return false;
        if (alert) return true;

        if (!alert)
        {
            if (CanSeePlayer(playerPos))
            {
                // TODO play some sort of alert exclamation mark event?
                alert = true;
            }
        }

        return alert;
    }
}
