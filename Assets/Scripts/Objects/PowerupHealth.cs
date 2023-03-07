using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// when the player touches this powerup they gain some health

public class PowerupHealth : PowerUpBase
{
    [SerializeField]int healAmount = 4;
    public override void PrepPowerup()
    {

    }

    public override bool TouchPowerup(PlayerPawn pawn)
    {
        if (!ready) return true;

        if (pawn.PickupHeal(healAmount))
        {
            Consume();
        }
        return true;
    }
}
