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
    [SerializeField]protected float moveSpeed = 8f; // rate at which this pawn moves from space to space (units per second)
    [SerializeField]protected int movePointsMax = 4; // how many moves can this pawn make each round?
    [SerializeField]protected  int healthMax = 5; // how many points of damage this pawn can take before death
    [SerializeField]WeaponList weaponOptions; // if this list is present the pawn will have one of this list as their equipped weapon
    [SerializeField]protected WeaponBase weaponUnarmed; // this is the unarmed weapon that this enemy uses when not carrying a weapon
    [SerializeField]EffectTimed damageEffect; // the timed effect which is created when this pawn is damaged
    protected bool moving = false; // is this pawn currently moving between cells?
    protected int movePoints;
    protected bool moveActionDone;
    protected int health;
    protected Vector3 attackFacing = Vector3.up; // always a unit in either axis
    protected int attackRange = 1; // the currently selected attack range
    protected WeaponBase weaponEquipped;

    protected virtual void Awake()
    {
        movePoints = movePointsMax;
        moveActionDone = false;
        health = healthMax;

        // equip one of the 
        if (weaponOptions)
        {
            WeaponBase weaponSelection = weaponOptions.Select();
            if (weaponSelection)
            {
                weaponEquipped = Instantiate(weaponSelection);
                weaponEquipped.EquipWeapon(this);
                Debug.Log("<color=blue>INFO</color> weapon " + weaponEquipped + " equipped on " + gameObject + " has ammo " + weaponEquipped.ammo + " of " + weaponEquipped.ammoMax);
            }
        }
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

    // this checks if it is possible for this pawn to move in direction dir (checks colliders)
    // returns true if possible, else returns false
    protected bool CanMove(Vector3 dir)
    {
        bool blocked = Physics2D.OverlapCircle(transform.position + dir, 0.2f, Global.LayerObstacle());

        return !blocked;
    }

    // this is for when specific pawns have a thing they should always do after a move
    protected virtual void PostMove() { }

    protected virtual float MoveSpeedScalar()
    {
        return moveSpeed * Global.scalePawnSpeed;
    }

    // this Coroutine manages moving a pawn from it's old position to a new position
    protected virtual IEnumerator MovePosition(Vector3 direction)
    {
        Vector3 target = transform.position + direction;

        moving = true;
        attackFacing = direction;
        attackRange = 1;

        while (moving)
        {
            bool lastMove = false;
            Vector3 moveStep = target - transform.position; // the actual movement for this frame
            float moveDistanceFrame = MoveSpeedScalar() * Time.deltaTime; // the maximum movement allowed for this Update frame

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
        moving = true;
    }
    // this is called at the end of an attack for any pawn-specific handling
    protected virtual void PostAttack()
    {
        moving = false;
    }

    // this coroutine manages performing the current attack
    // the attack direction, range and weapon are already stored in the class, this manages the timing and execution of those presets
    protected virtual IEnumerator Attack()
    {
        // TODO swing sound and animation
        if (weaponEquipped) weaponEquipped.AttackStart(attackFacing);
        PreAttack();
        yield return new WaitForSeconds(0.2f);
        // TODO attack hit and damage inflicting
        if (weaponEquipped) weaponEquipped.AttackDamage(transform.position, attackFacing, attackRange);
        else if (weaponUnarmed) weaponUnarmed.AttackDamage(transform.position, attackFacing, 1); // unarmed is always range 1
        else
        {
            Debug.Log("<color=orange>WARNING</color> pawn " + gameObject + " does not have a WeaponUnarmed!");
            Vector3 attackPoint = transform.position + attackFacing;
            Collider2D[] attackHits = Physics2D.OverlapCircleAll(attackPoint, 0.1f, Global.LayerPawn());
            foreach (Collider2D hit in attackHits)
            {
                PawnControllerBase hitPawn = hit.GetComponent<PawnControllerBase>();
                if (hitPawn)
                {
                    hitPawn.TakeDamage(1);
                    Debug.Log("<color=blue>INFO</color> HIT!");
                }
            }
        }
        yield return new WaitForSeconds(0.2f);
        if (weaponEquipped) weaponEquipped.AttackEnd();
        // TODO return weapon to ready position
        yield return new WaitForSeconds(0.2f);
        PostAttack();
    }

    // reduce health and detect death
    public virtual void TakeDamage(int amount)
    {
        if (damageEffect)
        {
            EffectTimed blood = Instantiate(damageEffect, transform.position, transform.rotation);
            blood.PlayEffect();
        }
        health -= amount;
        if (health <= 0)
        {
            Death();
        }
    }

    protected virtual void Death()
    {

    }

    public bool IsAlive()
    {
        return (health > 0);
    }

    // this is called by a weapon which wants this pawn to unequip it
    public virtual void UnequipWeapon(WeaponBase weaponUnequip)
    {
        Debug.Log("<color=blue>INFO</color> " + gameObject + " Unequip called");
        if (weaponEquipped)
        {
            if (weaponEquipped == weaponUnequip)
            {
                weaponEquipped = null;
            }
        }
    }

    public int DistanceTo(Vector3 targ)
    {
        return Global.OrthogonalDist(transform.position, targ);
    }
}
