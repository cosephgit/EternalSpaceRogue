using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Player Pawn
// DO NOT DELETE (carries the camera as a child)
// called by the StageManager when it is the player's turn and takes player input, these inputs are then passed to the parent PawnControllerBase to move the pawn etc

public class PlayerPawn : PawnControllerBase
{
    [SerializeField]private float moveSensitivty = 0.1f; // how much input is needed to accept a move request

    public override bool PawnUpdate()
    {
        if (moving) return false; // in the middle of a move, take no inputs

        if (movePointsLeft == 0)
            return true;

        Vector3 move = new Vector3();

        if (Input.GetAxis("Horizontal") > moveSensitivty && CanMove(transform.position + new Vector3(1f, 0f, 0f)))
        {
            move.x = 1f;
        }
        else if (Input.GetAxis("Horizontal") < -moveSensitivty && CanMove(transform.position + new Vector3(-1f, 0f, 0f)))
        {
            move.x = -1f;
        }
        else if (Input.GetAxis("Vertical") > moveSensitivty && CanMove(transform.position + new Vector3(0f, 1f, 0f)))
        {
            move.y = 1f;
        }
        else if (Input.GetAxis("Vertical") < -moveSensitivty && CanMove(transform.position + new Vector3(0f, -1f, 0f)))
        {
            move.y = -1f;
        }
        else
        {
            // no move input
            // TODO check for cancel movement option
            return false;
        }

        // some sort of valid move input has been received, start moving
        StartCoroutine(MovePosition(transform.position + move));

        return false;
    }

}
