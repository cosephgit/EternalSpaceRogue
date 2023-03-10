using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

// this is the parent class for all powerup objects
/*
options:
new weapon
armour boost
health boost
xp boost
reload weapon
trap! enemy
trap! bomb
trap! alarm (shouts for e.g. 25 spaces)

when is a crate opened?
on entering the space? or on TRYING to enter the space? (needed for trap types - enemy might appear in space)

initial implementation goals:
crate contains one of:
weapon
health
reload
enemy!

if the player is low on health or has no weapon, those get extra weight
*/

public class PowerUpBase : MonoBehaviour
{
    [SerializeField]protected float quality = 1;
    [SerializeField]EventReference pickupSound;
    [SerializeField]Animator pickupAnim;
    [SerializeField]SpriteRenderer pickupSprite;
    protected bool ready = true;

    public virtual void PrepPowerup(float maxStrength)
    {
        // called when this powerup is instantiated (when a crate places it)
    }

    public virtual bool TouchPowerup(PlayerPawn pawn)
    {
        if (!ready) return true;

        Consume();
        return true;
    }

    protected void Consume()
    {
        ready = false;
        if (!pickupSound.IsNull)
            AudioManager.instance.PlayOneShot(pickupSound, transform.position);

        StartCoroutine(ConsumeAnim());
    }

    IEnumerator ConsumeAnim()
    {
        if (pickupAnim)
        {
            pickupAnim.SetBool("Used", true);
            yield return new WaitForSeconds(0.2f);
        }

        if (pickupSprite)
        {
            // flash and disappear
            for (int i = 0; i < 10; i++)
            {
                if (i % 2 == 0) pickupSprite.enabled = false;
                else pickupSprite.enabled = true;
                yield return new WaitForSeconds(0.1f);
            }
        }

        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }

    public virtual float Quality()
    {
        return quality;
    }
}
