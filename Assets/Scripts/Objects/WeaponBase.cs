using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this manages the weapon and contains what the weapon can do
// any weapon animations/effects are handled here
// when a weapon is created it contains a certain amount of ammo
// when the ammo runs out it is discarded and fades out
// while carried the weapon is parented by the pawn controller
// when discarded it has no parent (and should have ZERO references to it) so it can fade out with no problems

public class WeaponBase : MonoBehaviour
{
    [field: Header("Graphics and effects")]
    [field: SerializeField]public string title { get; private set; } = "GUN";
    [SerializeField]SpriteRenderer gun; // the sprite used to show this gun
    [SerializeField]EffectTimed hitEffectPrefab; // the effect placed on each impact location
    [field: Header("Weapon statistics")]
    [field: SerializeField]public int ammoMax { get; private set; } // the ammo this weapon starts with
    [field: SerializeField]public int rangeMin { get; private set; } // the shortest range this weapon can be targeted at (at least 1)
    [field: SerializeField]public int rangeMax { get; private set; } // the maximum range this weapon can be targeted at
    [Header("These two arrays must have identical size")]
    [SerializeField]Vector3[] hitOffsets; // this is the array of all points the weapon hits, relative to the aim point when the attacker is facing up
    [SerializeField]int[] hitDamage; // this is the damage the weapon inflicts to the hit points above - MUST MATCH ARRAY SIZE ABOVE
    int hitPointCount;
    [HideInInspector]public int ammo { get; private set; }
    bool doInit = true;
    PawnControllerBase weapOwner;

    void Awake()
    {
        Initialise();
    }

    // initialise and validate values
    // owing to order of activation, it's possible that this needs to be done before it has a chance to Awake()
    void Initialise()
    {
        if (doInit)
        {
            // validate
            if (rangeMin < 1) rangeMin = 1;
            if (rangeMax < 1) rangeMax = 1;
            if (rangeMax > 6) rangeMax = 6;
            if (rangeMin > rangeMax) rangeMin = rangeMax;
            if (rangeMax < rangeMin) rangeMax = rangeMin;
            if (ammoMax < 1) ammoMax = -1;
            if (hitOffsets.Length != hitDamage.Length)
            {
                Debug.Log("<color=orange>WARNING</color> Weapon " + gameObject + " set up with invalid damage and area arrays");
                hitPointCount = Mathf.Min(hitOffsets.Length, hitDamage.Length);
            }
            else hitPointCount = hitOffsets.Length;

            if (hitPointCount < 1)
            {
                Debug.Log("<color=orange>WARNING</color> weapon has no valid target locations");
            }

            ammo = ammoMax;

            doInit = false;
        }
    }

    // this equips the weapon to the passed owner pawn
    public bool EquipWeapon(PawnControllerBase owner)
    {
        Initialise();
        weapOwner = owner;
        transform.parent = owner.transform;
        transform.position = owner.transform.position;
        return true;
    }

    // this is called automatically after a weapon runs out of ammo
    // the owner may also call it e.g. when they die or when the player chooses to drop their weapon
    public bool DiscardWeapon()
    {
        if (weapOwner)
        {
            weapOwner.UnequipWeapon(this);
        }
        transform.parent = null;
        return true;
    }

    public Vector3[] GetHitLocations(Vector3 origin, Vector3 facing, int range)
    {
        Vector3 target = origin + (facing * range);
        Vector3[] hitLocations = new Vector3[hitPointCount];

        for (int i = 0; i < hitPointCount; i++)
        {
            hitLocations[i] = target;
            if (facing == Vector3.down)
            {
                hitLocations[i] += -hitOffsets[i];
            }
            else if (facing == Vector3.right)
            {
                hitLocations[i].x += hitOffsets[i].y;
                hitLocations[i].y += -hitOffsets[i].x;
            }
            else if (facing == Vector3.left)
            {
                hitLocations[i].x += -hitOffsets[i].y;
                hitLocations[i].y += hitOffsets[i].x;

            }
            else // should always be Vector3.up
            {
                hitLocations[i] += hitOffsets[i];
            }
        }

        return hitLocations;
    }

    // this is called at the start of a pawn's attack
    // it makes the weapon animate
    public void AttackStart(Vector3 facing)
    {
        if (ammo > 0)
        {
            // if ammo is not already positive, then this is an infinite weapon (typically: an unarmed strike)
            ammo--;
        }
    }

    // this is called in the middle of a pawn's attack, it makes the weapon inflict damage on the target location with the orientation 
    public void AttackDamage(Vector3 origin, Vector3 facing, int range)
    {
        Vector3[] hitLocations = GetHitLocations(origin, facing, range);

        for (int i = 0; i < hitLocations.Length; i++)
        {
            Collider2D[] attackHits = Physics2D.OverlapCircleAll(hitLocations[i], 0.1f, Global.LayerPawn());
            foreach (Collider2D hit in attackHits)
            {
                PawnControllerBase hitPawn = hit.GetComponent<PawnControllerBase>();
                if (hitPawn)
                {
                    hitPawn.TakeDamage(hitDamage[i]);
                    Debug.Log("<color=blue>INFO</color> HIT");
                }
            }
            if (hitEffectPrefab)
            {
                EffectTimed impact = Instantiate(hitEffectPrefab, hitLocations[i], hitEffectPrefab.transform.rotation);
                impact.PlayEffect();
            }
        }
    }

    // this is called at the end of a pawn's attack
    // ammo is consumed here and the weapon is discarded if needed
    public void AttackEnd()
    {
        if (ammo == 0)
        {
            DiscardWeapon();
        }
    }

    // make this weapon bounce away and disappear
    void BounceAway()
    {
        // TODO finish
        Destroy(gameObject);
    }
}
