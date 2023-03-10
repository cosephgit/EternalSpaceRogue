using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

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
    [field: Header("Animation settings")]
    [field: SerializeField]public bool replaceRecoilWithLunge { get; private set; } // does the owner "lunge" with this weapon and swing it when they attack, instead of using a recoil animation?
    [field: SerializeField]public bool animsIncrement { get; private set; } // do the impact effects trigger incrementally rather than all at once? (e.g. automatic fire vs shotgun)
    [field: Header("Weapon statistics")]
    [field: SerializeField]public int ammoMax { get; private set; } = -1; // the ammo this weapon starts with
    [field: SerializeField]public int rangeMin { get; private set; } = 1; // the shortest range this weapon can be targeted at (at least 1)
    [field: SerializeField]public int rangeMax { get; private set; } = 1; // the maximum range this weapon can be targeted at
    [field: SerializeField]public float threatLevel { get; private set; } = 0; // an estimate of how dangerous this is to the player (between damage and difficulty avoiding)
    [Header("These two arrays must have identical size")]
    [SerializeField]Vector3[] hitOffsets; // this is the array of all points the weapon hits, relative to the aim point when the attacker is facing up
    [SerializeField]int[] hitDamage; // this is the damage the weapon inflicts to the hit points above - MUST MATCH ARRAY SIZE ABOVE
    [SerializeField]EventReference shootSound; // FMOD event reference
    int hitPointCount;
    [HideInInspector]public int ammo { get; private set; }
    bool doInit = true;
    public int rangeHitMax { get; private set; } // the furthest a target could actually be hit by this weapon in a straight line (used for AI calculation)
    PawnControllerBase weapOwner;
    List<Vector3> hitLocations;
    bool freeMoving = true;
    Vector3 holdPosition;
    float holdAngle;
    const int RECOILCOUNT = 4;

    void Awake()
    {
        Initialise();
        hitLocations = new List<Vector3>();
        holdPosition = Vector3.zero;
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

    public void ApplyAmmoUpgrade(float upgradeAmmo)
    {
        float multiplier = (5f + upgradeAmmo) * 0.2f;
        ammo = Mathf.RoundToInt((float)ammo * multiplier);
        ammoMax = Mathf.RoundToInt((float)ammoMax * multiplier);
    }

    // this equips the weapon to the passed owner pawn
    public bool EquipWeapon(PawnControllerBase owner, Vector3 dir)
    {
        if (!freeMoving) return false; // not available!
        Initialise();
        weapOwner = owner;
        transform.parent = owner.transform;
        transform.position = owner.transform.position;
        SetWeaponPosition(dir);
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
        StartCoroutine(BounceAway());
        return true;
    }

    // this is called when a weapon is dropped by an enemy so that it can be picked up again by the player
    // it will only have as much ammo as it did originally (the weapon instance is persistent)
    public void DropToFloor()
    {
        PowerupWeapon weaponHolder = Instantiate(PrefabProvider.inst.weaponHolder, transform.position, Quaternion.identity);
        // this takes care of parent/child relationships
        ammo = Mathf.CeilToInt((float)ammo * 0.5f); // only get half ammo
        weaponHolder.AcceptWeapon(this);
        weapOwner = null;
        freeMoving = true;
        SetWeaponPosition(Vector3.zero);
    }

    // works out where this weapon should be positioned relative to the holder
    public void SetWeaponPosition(Vector3 dir)
    {
        if (dir == Vector3.zero)
        {
            // reset back to normal (has been dropped)
            holdPosition = dir;
            holdAngle = 0;
            return;
        }
        // holdPosition = (a bit to the right) + (a bit forward) + (a bit up from the ground)
        holdPosition = (new Vector3(dir.y, -dir.x) * 0.5f) + (dir * 0.25f) + (Vector3.up * Global.WEAPONHOLDHEIGHT);
        if (dir.x == 1)
        {
            holdAngle = 180f;
        }
        else if (dir.y == -1)
        {
            holdAngle = 90f;
        }
        else if (dir.y == 1)
        {
            holdAngle = -90;
        }
        else
        {
            holdAngle = 0f;
        }
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
    public void AttackStart(Vector3 origin, Vector3 facing, int range)
    {
        SetWeaponPosition(facing);
        hitLocations.Clear();
        hitLocations.AddRange(GetHitLocations(origin, facing, range));
        if (ammo > 0)
        {
            // if ammo is not already positive, then this is an infinite weapon (typically: an unarmed strike)
            ammo--;
        }
        if (!shootSound.IsNull)
        {
            AudioManager.instance.PlayOneShot(shootSound, transform.position);
        }
        if (hitEffectPrefab && animsIncrement && hitLocations.Count > 0)
        {
            // place the impact points one at a time instead of all at once
            StartCoroutine(AnimEffectsIncremented());
        }
        if (replaceRecoilWithLunge)
        {
            // make weapon do a swinging arc
            StartCoroutine(AnimWeaponLunge(facing));
        }
        else if (animsIncrement)
        {
            // make weapon recoil backwards slightly - burst effect with multiple small bumps
            StartCoroutine(AnimWeaponRecoilBurst(facing));
        }
        else
        {
            Vector3 pos = gun.transform.localPosition - (facing * 0.5f); // simple recoil bump, the Update method will smoothly return it to position
            gun.transform.localPosition = pos;
        }
    }

    // lunge is a bit more complicated than the recoil!
    IEnumerator AnimWeaponLunge(Vector3 dir)
    {
        Vector3 posStart = holdPosition; // the move is all completed relative to the holdPosition
        Vector3 posReady = holdPosition + dir * 0.25f;
        Vector3 posEnd = posReady + new Vector3(-dir.y, dir.x); // left from forward dir
        float animTime = 0;
        float animTimeEnd;
        Vector3 pos;

        freeMoving = false;

        // first step of animation: move forward and right and angle to the side
        animTimeEnd = Global.combatStepDelay * 0.5f;
        while (animTime < animTimeEnd)
        {
            animTime += Time.deltaTime;

            pos = Vector3.Lerp(posStart, posReady, animTime / animTimeEnd);
            gun.transform.localPosition = pos;
            yield return new WaitForEndOfFrame();
        }
        
        // second step of animation: sweep right to left
        animTime = 0;
        animTimeEnd = Global.combatStepDelay * 0.5f;
        while (animTime < animTimeEnd)
        {
            animTime += Time.deltaTime;

            pos = Vector3.Lerp(posReady, posEnd, animTime / animTimeEnd);
            gun.transform.localPosition = pos;
            yield return new WaitForEndOfFrame();
        }

        // return control back to the ordinary position management
        freeMoving = true;
    }

    IEnumerator AnimWeaponRecoilBurst(Vector3 dir)
    {
        float recoilDelay = Global.combatStepDelay / RECOILCOUNT; // recoil 5 times
        for (int i = 0; i < RECOILCOUNT; i++)
        {
            Vector3 pos = gun.transform.localPosition - (dir * 0.2f); // simple recoil bump, the Update method will smoothly return it to position
            gun.transform.localPosition = pos;
            yield return new WaitForSeconds(recoilDelay);
        }
    }

    // this coroutine causes the impact points to be triggered one by one rather than all at once
    IEnumerator AnimEffectsIncremented()
    {
        float effectDelay = Global.combatStepDelay / (float)hitPointCount;

        for (int i = 0; i < hitLocations.Count; i++)
        {
            EffectTimed impact = Instantiate(hitEffectPrefab, hitLocations[i], hitEffectPrefab.transform.rotation);
            impact.PlayEffect();

            yield return new WaitForSeconds(effectDelay);
        }
    }

    // this is called in the middle of a pawn's attack, it makes the weapon inflict damage on the target location with the orientation 
    public void AttackDamage(int damageBonus = 0)
    {
        for (int i = 0; i < hitLocations.Count; i++)
        {
            Collider2D[] attackHits = Physics2D.OverlapCircleAll(hitLocations[i], 0.1f, Global.LayerPawn());
            foreach (Collider2D hit in attackHits)
            {
                PawnControllerBase hitPawn = hit.GetComponent<PawnControllerBase>();
                if (hitPawn)
                {
                    hitPawn.TakeDamage(hitDamage[i] + damageBonus);
                }
            }
            if (hitEffectPrefab && !animsIncrement)
            {
                EffectTimed impact = Instantiate(hitEffectPrefab, hitLocations[i], hitEffectPrefab.transform.rotation);
                impact.PlayEffect();
            }
        }
    }

    // this is called at the end of a pawn's attack
    // the weapon is discarded if needed
    public void AttackEnd()
    {
        if (ammo == 0)
        {
            DiscardWeapon();
        }
    }

    // make this weapon bounce away and disappear
    IEnumerator BounceAway()
    {
        if (gun)
        {
            Vector3 bounceDir = Quaternion.Euler(0, 0, Random.Range(-180f, 180f)) * Vector3.up;
            Vector3 groundPos = gun.transform.localPosition;
            float fakeYvee = 1f;
            float fakeY = Global.WEAPONHOLDHEIGHT;
            Vector3 pos;

            groundPos.y -= fakeY;

            freeMoving = false;

            // bounce away
            while (fakeY != 0 || fakeYvee != 0)
            {
                // move in the bounce direction
                groundPos += bounceDir * Time.deltaTime;

                // apply fake gravitational acceleration and velocity
                fakeYvee += Global.FAKEGRAVITY * Time.deltaTime;
                fakeY += fakeYvee * Time.deltaTime;

                pos = groundPos + new Vector3(0, fakeY, 0);
                gun.transform.localPosition = pos;

                if (fakeY < 0)
                {
                    // bounce!
                    fakeY = 0;
                    if (fakeYvee > -0.5f) // let it stop bouncing
                        fakeYvee = 0;
                    else
                    {
                        fakeYvee = -0.8f * fakeYvee;
                        bounceDir *= 0.8f; // slow down slightly
                    }
                }
                yield return new WaitForEndOfFrame();
            }

            // flash and disappear
            for (int i = 0; i < 10; i++)
            {
                if (i % 2 == 0) gun.enabled = false;
                else gun.enabled = true;
                yield return new WaitForSeconds(0.1f);
            }
        }
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }

    // refills the weapon
    public void Reload()
    {
        ammo = ammoMax;
    }

    void Update()
    {
        if (gun && freeMoving)
        {
            if (!Global.ApproxVector(holdPosition, gun.transform.localPosition))
            {
                // gradually move to default position
                Vector3 pos = holdPosition - gun.transform.localPosition;
                if (pos.magnitude > Time.deltaTime)
                {
                    pos = pos.normalized * Time.deltaTime;
                }
                pos += gun.transform.localPosition;
                gun.transform.localPosition = pos;
            }
            gun.transform.rotation = Quaternion.Euler(0, 0, holdAngle);
        }
    }
}
