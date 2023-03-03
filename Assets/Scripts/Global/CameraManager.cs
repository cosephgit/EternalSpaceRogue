using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// moves the camera with the player, lagging slightly to give sense of movement

public class CameraManager : MonoBehaviour
{
    [SerializeField]float speedRatio = 0.5f; // amount of the distance between the camera and the player to move each second
    [SerializeField]float speedMin = 1f; // minimum movement speed of the camera per second (to close small distances)

    // check each LateUpdate after all movement is proceesed
    void LateUpdate()
    {
        // make sure there's a player
        if (StageManager.instance.playerPawn)
        {
            Vector3 offset = StageManager.instance.playerPawn.transform.position - transform.position;

            offset.z = 0; // never move in the z axis

            if (Mathf.Approximately(offset.magnitude, 0f)) return; // on position already, no move needed

            Vector3 movement;

            if (offset.magnitude < speedMin * Time.deltaTime)
            {
                // if the distance is less than the minimum camera speed, just move that amount
                movement = offset;
            }
            else if (speedRatio * offset.magnitude < speedMin) // Time.deltaTime would appear on both sides of this equation so has been cancelled out
            {
                // if the distance * speedRatio is less than the minimum camera speed, move the minimum camera speed
                movement = offset.normalized * speedMin * Time.deltaTime;
            }
            else
            {
                // move the distance * speedRationamount
                movement = offset * speedRatio * Time.deltaTime; // adjust movement to speed ratio and frame time
            }

            transform.Translate(movement);
        }
    }
}
