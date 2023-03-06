using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// manages the objective zone and detects when the player enters it
// has a collider only for blocking enemy movement and navigation - don't want enemies standing in it

public class ObjectiveZone : MonoBehaviour
{
    [SerializeField]float touchWithinX = 1.1f;
    [SerializeField]float touchWithinY = 1.1f;
    [SerializeField]int objectiveXP = 10;
    bool unTouched = true;

    // checks if the player has reached the objective and tells the player pawn if they do so
    // (this should only happen during player movement, so it is the player's active turn and their pawn will receive the message)
    void Update()
    {
        if (unTouched)
        {
            if (StageManager.instance.playerPawn)
            {
                if (StageManager.instance.playerPawn.IsAlive())
                {
                    Vector3 offset = transform.position - StageManager.instance.playerPawn.transform.position;

                    if (Mathf.Abs(offset.x) <= touchWithinX && Mathf.Abs(offset.y) <= touchWithinY)
                    {
                        StageManager.instance.ObjectiveReached(objectiveXP);
                        unTouched = false;
                        Debug.Log("<color=blue>INFO</color> Stage complete!");
                    }
                }
            }
        }
    }
}
