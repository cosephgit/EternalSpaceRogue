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
    protected bool ready = true;

    public virtual void PrepPowerup(float maxStrength)
    {
        // called when this powerup is instantiated (when a crate places it)
    }

    public virtual bool TouchPowerup(PlayerPawn pawn)
    {
        if (!ready) return true;

        if (!pickupSound.IsNull)
            AudioManager.instance.PlayOneShot(pickupSound, transform.position);
        Consume();
        return true;
    }

    protected virtual void Consume()
    {
        ready = false;
        Destroy(gameObject);
    }

    public virtual float Quality()
    {
        return quality;
    }
}
