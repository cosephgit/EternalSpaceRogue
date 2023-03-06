using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// when the player moves they check the space they are entering first
// if the space contains a crate, the crate opens!
// this MAY result in rejecting the move back to the player, as an enemy could be spawned by the crate

public class PowerupCrate : PowerUpBase
{
    [SerializeField]PowerUpBase powerGun;
    [SerializeField]PowerUpBase powerHealth;
    [SerializeField]PowerUpBase powerReload;

    public override bool TouchPowerup(PlayerPawn pawn)
    {
        if (!ready) return true;

        float weightGun = 0.5f; // Either this or the next line WILL get doubled
        float weightReload = 0.5f; // so total 1.5f, ~75% normally or ~60% on low health
        float weightHealth = 0.5f; // Doubled on low health, so normally ~25% or ~40% on low health
        float weightEnemy = 0.1f; // defaults to ~4% chance of a trap

        Consume();

        if (pawn.LowHealth())
        {
            weightHealth *= 2;
        }
        if (pawn.HasWeapon())
        {
            weightReload *= 2;
        }
        else
        {
            weightGun *= 2;
        }
        if (StageManager.instance.powerPoints < 0)
        {
            // if the player has gotten lucky with powerups, higher chance of another enemy
            weightEnemy *= 2;
        }

        float weighTotal = weightGun + weightHealth + weightReload + weightEnemy;
        float select = Random.Range(0, weighTotal);
        PowerUpBase dropped;
        if (select < weightGun)
        {
            // spawn a gun
            dropped = Instantiate(powerGun, transform.position, Quaternion.identity);
        }
        else if (select < weightGun + weightHealth)
        {
            // spawn a health pack
            dropped = Instantiate(powerHealth, transform.position, Quaternion.identity);
        }
        else if (select < weightGun + weightHealth + weightReload)
        {
            // spawn a reload pack
            dropped = Instantiate(powerReload, transform.position, Quaternion.identity);
        }
        else
        {
            // spawn an extra enemy!
            // note there should NEVER be more than one crate in a space, so there's no risk of an overlap
            StageManager.instance.EnemySpawn(transform.position);
            return false; // block player entering the space
        }

        dropped.PrepPowerup(); // set up the specific item of this type (e.g. which weapon)
        dropped.TouchPowerup(pawn); // then the player touches that too
        return true;
    }
}
