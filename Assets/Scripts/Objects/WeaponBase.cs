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
    [field: SerializeField]public int ammoMax { get; private set; } = -1; // the ammo this weapon starts with
    [field: SerializeField]public int rangeMin { get; private set; } = 1; // the shortest range this weapon can be targeted at (at least 1)
    [field: SerializeField]public int rangeMax { get; private set; } = 1; // the maximum range this weapon can be targeted at
    [field: SerializeField]public float threatLevel { get; private set; } = 0; // an estimate of how dangerous this is to the player (between damage and difficulty avoiding)
    [Header("These two arrays must have identical size")]
    [SerializeField]Vector3[] hitOffsets; // this is the array of all points the weapon hits, relative to the aim point when the attacker is facing up
    [SerializeField]int[] hitDamage; // this is the damage the weapon inflicts to the hit points above - MUST MATCH ARRAY SIZE ABOVE
    int hitPointCount;
    [HideInInspector]public int ammo { get; private set; }
    bool doInit = true;
    public int rangeHitMax { get; private set; } // the furthest a target could actually be hit by this weapon in a straight line (used for AI calculation)
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

            rangeHitMax = rangeMax;
            if (hitPointCount < 1)
            {
                Debug.Log("<color=orange>WARNING</color> weapon has no valid target locations");
            }
            else
            {
                int rangeMaxBonus = 0;
                for (int i = 0; i < hitPointCount; i++)
                {
                    if (hitOffsets[i].x == 0)
                    {
                        if (hitOffsets[i].y > rangeMaxBonus)
                        {
                            if (hitDamage[i] > 0)
                            {
                                rangeMaxBonus = (int)hitOffsets[i].y;
                            }
                        }
                    }
                }
                rangeHitMax += rangeMaxBonus;
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

    // returns a list of all the spaces that the target space could be attacked from with this weapon
    // n.b. NOT REALLY
    // it only actually looks at straight line orthogonal attacks, because:
    // 1) far less spaces to pathfind to
    // 2) produces nicer enemy behaviour, where they always try to line the player up at the centre of a burst
    // the only invalidation done here is if there's a wall in the way of the point or any space between the target and the point
    // it doesn't check for obstructing pawns, that needs to be done in pathfinding next
    public List<Vector3> GetAllPossibleAttackLocations(Vector3 target)
    {
        List<Vector3> targetList = new List<Vector3>();

        for (int dir = 0; dir < 4; dir++)
        {
            Vector3 dirVector;
            switch (dir)
            {
                default:
                case 0:
                    dirVector = Vector3.up;
                    break;
                case 1:
                    dirVector = Vector3.right;
                    break;
                case 2:
                    dirVector = Vector3.down;
                    break;
                case 3:
                    dirVector = Vector3.left;
                    break;
            }

            Vector3 pos = target;
            for (int i = 0; i < rangeHitMax; i++)
            {
                pos += dirVector;
                Collider2D wall = Physics2D.OverlapPoint(pos, Global.LayerWall());
                Collider2D nav = Physics2D.OverlapPoint(pos, Global.LayerNav());
                if (wall) i = rangeHitMax; // this point is blocked by a wall, so all further points must also be blocked
                else if (nav)
                {
                    // there's no blocking wall and it's in the nav network, so this is a possible place to move to!
                    targetList.Add(pos);
                }
            }
        }

        return targetList;
    }

    // returns an array of the spaces that will be hit by an attack from this origin, facing and range
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
