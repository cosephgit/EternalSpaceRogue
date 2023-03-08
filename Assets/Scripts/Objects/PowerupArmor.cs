using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupArmor : PowerUpBase
{
    [SerializeField]int armorAmount = 4;

    public override bool TouchPowerup(PlayerPawn pawn)
    {
        if (!ready) return true;

        if (pawn.PickupArmor(armorAmount))
        {
            Consume();
        }
        return true;
    }
}
