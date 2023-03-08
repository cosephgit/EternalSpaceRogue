using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPawn : PawnControllerBase
{
    [field: Header("Enemy specific values")]
    [field: SerializeField]public float enemyStrength { get; private set; } = 1; // the approximate MINIMUM strength of this enemy, used in populating a stage
    bool alert;
    public float actualStrength { get; private set; } = 1f; // the ACTUAL strength of this enemy, after accounting for buffs/weapons
    List<Vector3> route = new List<Vector3>();

    // this is called after an enemy is instantiated
    // it attempts to calibrate the enemy strength (damage, health, speed, etc) to the provided targetStrength
    // it returns the ACTUAL strength of the enemy (resulting from e.g. rounding) for the StageManager to adjust the total stage balancing
    public float SetStrength(float targetStrength)
    {
        float extraStrength = Mathf.Max(targetStrength - enemyStrength, 0); // any extra stage strength is used to set the maximum weapon power
        actualStrength = enemyStrength;

        WeaponStart(extraStrength);

        if (weaponEquipped) actualStrength += weaponEquipped.threatLevel; // add the threat of the selected weapon

        // TODO actual strength calculation and balancing
        // basically this will increase some basic parameters of the enemy and possibly give them a better weapon, but will not give them a massive boost
        if (actualStrength < targetStrength)
        {
            // add power ups
        }

        return actualStrength;
    }

    protected override void Death()
    {
        base.Death();
        StageManager.instance.EnemyDead(this, actualStrength);
        if (weaponEquipped)
        {
            float dropChance = Global.DROPCHANCEBASE;
            if (weaponEquipped.ammoMax > 0)
            {
                dropChance += Global.DROPCHANCEBYAMMO * weaponEquipped.ammo / weaponEquipped.ammoMax;
            }
            if (Random.Range(0f, 1f) < dropChance)
            {
                weaponEquipped.DropToFloor();
            }
            else
                weaponEquipped.DiscardWeapon();
            weaponEquipped = null;
        }

        Destroy(gameObject);
    }

    // remove this enemy silently from the level
    public void SilentRemove()
    {
        Destroy(gameObject);
    }

    public override void RoundPrep()
    {
        if (!IsAlive()) return;

        Vector3 playerPos = StageManager.instance.playerPawn.transform.position; // going to need this a few times
        Vector3 pointSelected = Vector3.zero;
        bool pointFound = false; // this is needed to flag when the pointSelected is actually intentional (because any pointSelected value might be valid)
        route.Clear();

        // pathfind to target
        if (weaponEquipped)
        {
            List<Vector3> targetPoints = weaponEquipped.GetAllPossibleAttackLocations(playerPos);
            if (targetPoints.Count > 0)
            {
                List<Vector3> targetPointsClear = targetPoints; // this starts as a duplicate of the above, then has obstructed points removed

                for (int i = targetPointsClear.Count - 1; i >= 0; i--)
                {
                    bool reject = false;
                    if (Global.OrthogonalDist(targetPointsClear[i], transform.position) > movePointsMax)
                    {
                        // this point is out of reach this round, so it doesn't matter if it's clear it's not a priority
                        reject = true;
                    }
                    else
                    {
                        Vector3 offset = targetPointsClear[i] - playerPos; // this SHOULD be in a straight line

                        for (int j = 1; j <= Mathf.CeilToInt(offset.magnitude); j++)
                        {
                            Vector3 posTest = playerPos + offset.normalized * j;
                            Collider2D pawn = Physics2D.OverlapPoint(posTest, Global.LayerPawn());

                            if (pawn)
                            {
                                EnemyPawn pawnFound = pawn.GetComponent<EnemyPawn>();
                                if (pawnFound && pawnFound != this)
                                {
                                    // if any point contains a pawn EXCEPT THIS PAWN, the path is blocked
                                    reject = true;
                                    j = Mathf.CeilToInt(offset.magnitude);
                                }
                            }
                        }
                    }

                    if (reject)
                    {
                        targetPointsClear.Remove(targetPointsClear[i]);
                    }
                    else
                    {
                        // this point is actually a viable position to move to and attack from this round!
                        // now we start doing the expensive pathfinding
                        // for these ATTACK THIS ROUND points, we ONLY care about points with a clear path that are within movement range
                        // for anything else we might as just move straight to the player
                        if (Global.ApproxVector(transform.position, targetPointsClear[i]))
                        {
                            // already standing in place, so it's good! but only accept it if there isn't already another place, as a point that requires moving is better
                            if (!pointFound)
                            {
                                pointSelected = targetPointsClear[i];
                                pointFound = true;
                            }
                        }
                        else
                        {
                            // check the pathfinding to this point
                            List<Vector3> routePossible = StageManager.instance.Pathfind(transform.position, targetPointsClear[i], false, movePointsMax);
                            if (routePossible.Count > 0)
                            {
                                // then we have a viable path to this point!
                                // so this becomes our new point IF
                                // this point is further away from the player than the existing point
                                // on a tie: if the current point has a shorter route (encourage more enemy movement)
                                // on a tie: random bool
                                if (pointFound)
                                {
                                    // the amount of extra distance from the target for this point versus the stored point
                                    int distExtra = Global.OrthogonalDist(targetPointsClear[i], playerPos) - Global.OrthogonalDist(pointSelected, playerPos);
                                    if (distExtra > 0)
                                    {
                                        // this is further from the player than the current point, so it's better
                                        // leaves more room for more enemies to move in and attack
                                        pointSelected = targetPointsClear[i];
                                        route = routePossible;
                                    }
                                    else if (distExtra == 0)
                                    {
                                        // equidistant from the player, so pick this one only if it's a longer route (to make more dynamic enemies)
                                        if (routePossible.Count > route.Count)
                                        {
                                            pointSelected = targetPointsClear[i];
                                            route = routePossible;
                                        }
                                        else if (routePossible.Count == route.Count)
                                        {
                                            if (Global.RandomBool())
                                            {
                                                // points are equally good, randomly pick
                                                pointSelected = targetPointsClear[i];
                                                route = routePossible;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // this is the first valid point found, accept it and store the route as the current planned route
                                    pointSelected = targetPointsClear[i];
                                    route = routePossible;
                                    pointFound = true;
                                }
                            }
                        }
                    }
                }
                if (!pointFound)
                {
                    // could not find a point that can be reached this round, just move to the player
                    route = StageManager.instance.Pathfind(transform.position, playerPos);
                    // this should ALWAYS generate a route of at least 1 (the move into the player's space) unless it's impossible
                    if (route.Count > 0)
                        pointFound = true;
                }
            }
            else
            {
                // no viable attack positions (terrain blockage or too far), go for melee
                route = StageManager.instance.Pathfind(transform.position, playerPos);
                // this should ALWAYS generate a route of at least 1 (the move into the player's space) unless it's impossible
                if (route.Count > 0)
                    pointFound = true;
            }
        }
        else
        {
            // if there is no weapon, just try to move to the player at all costs
            route = StageManager.instance.Pathfind(transform.position, playerPos);
            // this should ALWAYS generate a route of at least 1 (the move into the player's space) unless it's impossible
            if (route.Count > 0)
                pointFound = true;
        }

        if (!pointFound)
        {
            // target is too far or otherwise no approach of any sort exists
            alert = false;
        }
        else
        {
            base.RoundPrep();
            if (route.Count - movePoints > 8)
                movePoints += 2; // enemies that are a long way away get a little movement boost rather than paying the cost of pathfinding for a double move
        }
    }

    void FindOptimalRange(Vector3 checkDir, Vector3 target, out int rangeOptimal, out float rangeOptimalFF)
    {
        Vector3[] attackSpaces;
        int rangeCheck = 1;
        rangeOptimal = 0;
        rangeOptimalFF = 0;

        // so now we get all attack points for the weapon and if any of them contain the player, its' good!
        // we need to check all possible attack ranges though
        // if there are multiple good answers, take the one that hits the least friendlies
        while (rangeCheck <= weaponEquipped.rangeMax)
        {
            float rangeCheckFF = 0;
            bool rangeHitsPlayer = false;

            attackSpaces = weaponEquipped.GetHitLocations(transform.position, checkDir, rangeCheck);

            // check for the player and for friendlies that will be hit by using this range, we need all of this before making decisions
            for (int i = 0; i < attackSpaces.Length; i++)
            {
                if (Global.ApproxVector(target, attackSpaces[i]))
                {
                    rangeHitsPlayer = true;
                }
                else
                {
                    Collider2D hitEnemy = Physics2D.OverlapPoint(attackSpaces[i], Global.LayerPawn());
                    if (hitEnemy)
                    {
                        EnemyPawn hitEnemyPawn = hitEnemy.GetComponent<EnemyPawn>();
                        if (hitEnemyPawn)
                        {
                            if (hitEnemyPawn.IsAlive())
                            {
                                rangeCheckFF += hitEnemyPawn.enemyStrength;
                            }
                        }
                    }
                }
            }

            if (rangeHitsPlayer)
            {
                bool rangeGood = false;

                // attacking with this range DOES hit the player, now we need to find the range that hits the player and ALSO hits the least allies
                if (rangeOptimal == 0)
                {
                    // doesn't matter this is the first good range found
                    rangeGood = true;
                }
                else
                {
                    if (rangeCheckFF < rangeOptimalFF)
                    {
                        // this hurts less of our buddies, so use this aim instead
                        rangeGood = true;
                    }
                    else if (rangeCheckFF == rangeOptimalFF && Global.RandomBool())
                    {
                        rangeGood = true;
                    }
                }

                if (rangeGood)
                {
                    rangeOptimal = rangeCheck;
                    rangeOptimalFF = rangeCheckFF;
                }
            }

            // check if the current aim space is obstructed by a pawn or wall - if so, it's the maximum range we can try
            Collider2D blockedSpace = Physics2D.OverlapPoint(transform.position + (checkDir * rangeCheck), Global.LayerObstacle());
            if (blockedSpace) rangeCheck = weaponEquipped.rangeMax + 1;
            else rangeCheck++;
        }
    }

    // TODO
    // use action points to move to player by the route defined in RoundPrep()
    // once action points are spent, switch to attack mode
    // if player is in position that can be attacked, attack them
    public override bool PawnUpdate()
    {
        if (moving) return false;

        if (movePoints == 0 && moveActionDone) return true;

        Vector3 playerPos = StageManager.instance.playerPawn.transform.position;
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
            int rangeCurrentMax = 1;
            int rangeCurrent = Global.OrthogonalDist(targetOffset, Vector3.zero);
            bool canAttack = false;

            if (weaponEquipped)
                rangeCurrentMax = weaponEquipped.rangeHitMax;

            if (rangeCurrent <= 1)
            {
                // can definitely attack the player when right next to them
                canAttack = true;
                attackFacing = targetOffset;
                attackRange = 1;
            }
            else if (rangeCurrent <= rangeCurrentMax)
            {
                Vector3 attackDir;
                Vector3 attackDirTryA;
                Vector3 attackDirTryB;
                int rangeOptimal; // the current determined optimal range
                float rangeOptimalFF; // how many friendlies will be hit at the optimal range

                // it might be possible to attack the player, check in more detail
                if (targetOffset.x == 0 || targetOffset.y == 0)
                {
                    attackDir = targetOffset.normalized;
                    // this is a straight up/down or left/right attack
                    FindOptimalRange(attackDir, playerPos, out rangeOptimal, out rangeOptimalFF);
                }
                else
                {
                    int rangeOptimalTest;
                    float rangeOptimalTestFF;
                    // a bit more complicated as there may be two different sets of possible attack points
                    attackDirTryA = new Vector3(0, targetOffset.y).normalized;
                    attackDirTryB = new Vector3(targetOffset.x, 0).normalized;

                    // the first test we can just enter straight into the optimal values
                    FindOptimalRange(attackDirTryA, playerPos, out rangeOptimal, out rangeOptimalFF);
                    if (rangeOptimal > 0)
                    {
                        attackDir = attackDirTryA;
                        // the second test we need to store in test values, to compare to the first set of values
                        FindOptimalRange(attackDirTryB, playerPos, out rangeOptimalTest, out rangeOptimalTestFF);
                        if (rangeOptimalTest > 0)
                        {
                            bool checkBWins = false;
                            // both gave a result, so compare
                            if (rangeOptimalTestFF < rangeOptimalFF)
                            {
                                checkBWins = true;
                            }
                            else if (rangeOptimalTestFF == rangeOptimalFF && Global.RandomBool())
                            {
                                checkBWins = true;
                            }
                            if (checkBWins)
                            {
                                attackDir = attackDirTryB;
                                rangeOptimal = rangeOptimalTest;
                                rangeOptimalFF = rangeOptimalTestFF;
                            }
                        }
                    }
                    else // the first test failed so we can just do the second test without any extra checks
                    {
                        attackDir = attackDirTryB;
                        FindOptimalRange(attackDirTryB, playerPos, out rangeOptimal, out rangeOptimalFF);
                    }
                }
                if (rangeOptimal > 0)
                {
                    // we have found a point to attack from
                    // TODO we could check the rangeOptimalFF value here and maybe reconsider if it hits a ton of our allies
                    canAttack = true;
                    attackFacing = attackDir;
                    attackRange = rangeOptimal;
                }
            }

            if (canAttack)
            {
                StartCoroutine(Attack());
            }
            moveActionDone = true; // attack started or not possible, end go
            // TODO possibly later add enemies double moving?
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
