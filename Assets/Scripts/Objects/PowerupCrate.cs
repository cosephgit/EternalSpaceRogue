using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// when the player moves they check the space they are entering first
// if the space contains a crate, the crate opens!
// this MAY result in rejecting the move back to the player, as an enemy could be spawned by the crate

public class PowerupCrate : PowerUpBase
{
    [SerializeField]PowerUpBase powerGun;
    [SerializeField]PowerUpBase[] powerHealth;
    [SerializeField]PowerUpBase[] powerArmor;
    [SerializeField]PowerUpBase powerReload;

    public override bool TouchPowerup(PlayerPawn pawn)
    {
        if (!ready) return true;

        float weightGun = 0.5f; // Either this or the next line WILL get += 0.25f
        float weightReload = 0.25f; // so total 1.0f, ~50% normally or ~40% on low health
        float weightHealth = 0.5f; // += 0.5f on low health, so normally ~25% or ~40% on low health
        float weightArmor = 0.5f; // ~25% normally, or ~20% on low health
        float weightEnemy;

        Consume();

        if (pawn.LowHealth())
        {
            weightHealth += 0.5f;
        }
        if (pawn.HasWeapon())
        {
            weightReload += 0.25f;
        }
        else
        {
            weightGun += 0.25f;
        }
        if (StageManager.instance.PowerUpPlenty())
        {
            weightEnemy = 0; // still a lot of powerups unclaimed, so no surprises yet
        }
        else if (StageManager.instance.powerPoints < 0)
        {
            // if the player has gotten lots of good powerups, high chance of another enemy
            weightEnemy = 0.5f;
        }
        else
        {
            weightEnemy = 0.1f;
        }

        float weighTotal = weightGun + weightHealth + weightArmor + weightReload + weightEnemy;
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
            dropped = Instantiate(powerHealth[Random.Range(0, powerHealth.Length)], transform.position, Quaternion.identity);
        }
        else if (select < weightGun + weightHealth + weightArmor)
        {
            // spawn an armor boost
            dropped = Instantiate(powerArmor[Random.Range(0, powerArmor.Length)], transform.position, Quaternion.identity);
        }
        else if (select < weightGun + weightHealth + weightArmor + weightReload)
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

        StageManager.instance.PowerUpSpawned(dropped.Quality()); // report the value of the powerup to the stage manager

        dropped.TouchPowerup(pawn); // then the player touches that too
        return true;
    }
}
