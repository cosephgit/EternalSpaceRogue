using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPawn : PawnControllerBase
{
    int enemyXP = 1; // XP award for defeating this enemy
    bool alert;
    List<Vector3> route = new List<Vector3>();

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

    public override void RoundPrep()
    {
        route.Clear();

        // pathfind to target
        route = StageManager.instance.Pathfind(transform.position, StageManager.instance.playerPawn.transform.position);

        //Debug.Log(Global.VectorListToString(route));

        if (route.Count > Global.PATHFINDMAX)
        {
            // target is too far away, give up
            alert = false;
        }
        else
        {
            base.RoundPrep();
        }
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
            if (route.Count > 0)
            {
                if (CanMove(route[route.Count - 1]))
                {
                    StartCoroutine(MovePosition(route[route.Count - 1]));
                    route.RemoveAt(route.Count-1);
                }
                movePoints--;
            }
            else
            {
                movePoints = 0;
            }
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

    protected override float MoveSpeedScalar()
    {
        if (CanSeePlayer(StageManager.instance.playerPawn.transform.position, 1))
            return moveSpeed * Global.scalePawnSpeed;
        else
            return 1000; // if out of the player's view, move super fast
    }

    // returns true if the enemy is visible on the player's screen
    // like a certain beast of Traal, if you can't see it, it can't see you
    // buffer is used to make sure enemies behave this way when they might be edging into view
    bool CanSeePlayer(Vector3 playerPos, int buffer = 0)
    {
        Vector3 offset = playerPos - transform.position;

        if (Mathf.Abs(offset.x) < GameManager.instance.screenCellWidth + buffer
            && Mathf.Abs(offset.y) < GameManager.instance.screenCellHeight + buffer)
            return true;

        return false;
    }

    // this checks if this enemy is already active OR if not, if they can see the player pos (passed in for convenience) and so can become active
    public bool CheckAlert(Vector3 playerPos)
    {
        if (!IsAlive()) return false;
        if (alert) return true;

        if (CanSeePlayer(playerPos))
        {
            // TODO play some sort of alert exclamation mark event?
            alert = true;
        }

        return alert;
    }
    // checks if an enemy is already active, returns false if dead or already alert, returns true if it wasn't alert (and it now is)
    public bool MakeAlert()
    {
        if (!IsAlive()) return false;
        if (alert) return false;
        alert = true;
        return true;
    }
}
