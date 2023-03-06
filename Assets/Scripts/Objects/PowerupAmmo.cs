using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupAmmo : PowerUpBase
{
    public override void PrepPowerup()
    {
        
    }

    public override bool TouchPowerup(PlayerPawn pawn)
    {
        if (!ready) return true;

        if (pawn.Reload())
        {
            Consume();
            return true;
        }
        return true;
    }
}
