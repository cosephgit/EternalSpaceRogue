using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPawn : PawnControllerBase
{
    // this is called after an enemy is instantiated
    // it attempts to calibrate the enemy strength (damage, health, speed, etc) to the provided targetStrength
    // it returns the ACTUAL strength of the enemy (resulting from e.g. rounding) for the StageManager to adjust the level balance
    public float SetStrength(float targetStrength)
    {
        float actualStrength = targetStrength;

        // TODO actual strength calculation and balancing

        return actualStrength;
    }
}
