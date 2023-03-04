using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// the basic class for any pawn (player or enemy)
// behaviour:
// health monitoring
// death detection
// update (called by stage manager while this pawn is active)
// report action completion (to stage manager)
// movement and attack behaviour
// animation management

public class PawnControllerBase : MonoBehaviour
{
    [SerializeField]private float moveSpeed = 8f; // rate at which this pawn moves from space to space (units per second)
    [SerializeField]protected int movePointsMax = 4; // how many moves can this pawn make each round?
    [SerializeField]protected  int healthMax = 5; // how many points of damage this pawn can take before death
    protected bool moving = false; // is this pawn currently moving between cells?
    protected int movePoints;
    protected bool moveActionDone;
    protected int health;
    protected Vector3 attackFacing; // always a unit in either axis
    protected int attackRange; // the currently selected attack range

    protected virtual void Awake()
    {
        movePoints = movePointsMax;
        moveActionDone = false;
        health = healthMax;
    }

    // tells the pawn it's about to start a new round
    // set the movement points to full
    // prepare the "action" (for enemies this means "attack", for the player there are options)
    public virtual void RoundPrep()
    {
        movePoints = movePointsMax;
        moveActionDone = false;
    }

    // this is the main action loop that is called during Update for as long as this pawn is the active pawn
    // it returns true ONLY when it has completed ALL actions (or has died) and the game can proceed to the next pawn
    public virtual bool PawnUpdate()
    {
        return true; // temp
    }

    // this checks if it is possible for this pawn to move to the target (checks colliders)
    // returns true if possible, else returns false
    protected bool CanMove(Vector2 target)
    {
        bool blocked = Physics2D.OverlapCircle(target, 0.2f, Global.LayerObstacle());

        return !blocked;
    }

    // this is for when specific pawns have a thing they should always do after a move
    protected virtual void PostMove() { }
    
    // this Coroutine manages moving a pawn from it's old position to a new position
    protected virtual IEnumerator MovePosition(Vector3 direction)
    {
        Vector3 target = transform.position + direction;

        moving = true;
        attackFacing = direction;

        while (moving)
        {
            bool lastMove = false;
            Vector3 moveStep = target - transform.position; // the actual movement for this frame
            float moveDistanceFrame = moveSpeed * Global.scalePawnSpeed * Time.deltaTime; // the maximum movement allowed for this Update frame

            if (moveStep.magnitude > moveDistanceFrame)
                moveStep = moveStep.normalized * moveDistanceFrame;
            else
                lastMove = true; // allow it to leave the loop and finish the coroutine

            transform.Translate(moveStep);
            yield return new WaitForEndOfFrame();

            if (lastMove) moving = false;
        }

        PostMove();
    }

    // this is called at the start of an attack for any pawn-specific handling
    protected virtual void PreAttack()
    {
        
    }
    // this is called at the end of an attack for any pawn-specific handling
    protected virtual void PostAttack()
    {
        
    }

    // this coroutine manages performing the current attack
    // the attack direction, range and weapon are already stored in the class, this manages the timing and execution of those presets
    protected virtual IEnumerator Attack()
    {
        PreAttack();
        // TODO swing sound and animation
        yield return new WaitForSeconds(0.2f);
        // TODO attack hit and damage inflicting
        Vector3 attackPoint = transform.position + attackFacing;
        Collider2D[] attackHits = Physics2D.OverlapCircleAll(attackPoint, 0.1f, Global.LayerPawn());
        foreach (Collider2D hit in attackHits)
        {
            PawnControllerBase hitPawn = hit.GetComponent<PawnControllerBase>();
            if (hitPawn)
            {
                Debug.Log("HIT!");
            }
        }

        yield return new WaitForSeconds(0.2f);
        // TODO return weapon to ready position
        yield return new WaitForSeconds(0.2f);
        PostAttack();
    }

    // reduce health and detect death
    public virtual void TakeDamage(int amount)
    {
        health -= amount;
    }
}
