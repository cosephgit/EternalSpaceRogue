using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stores permissible pawn positions for creating pathfinding data
// might do other things later e.g. mark good places for doors/loot/etc

public class NavNode : MonoBehaviour
{
    // nodes to navigate to adjacent nodes
    NavNode upNode;
    NavNode rightNode;
    NavNode downNode;
    NavNode leftNode;

    // this is called after all NavNodes are placed to calculate node adjacency
    public void CheckConnection()
    {
        Collider2D[] nodesAdjacent = Physics2D.OverlapCircleAll(transform.position, 1.1f, Global.LayerNav());

        foreach (Collider2D node in nodesAdjacent)
        {
            NavNode navNode = node.GetComponent<NavNode>();
            if (navNode)
            {
                Vector3 offset = navNode.transform.position - transform.position;

                if (offset.x == 1 && offset.y == 0)
                {
                    rightNode = navNode;
                }
                else if (offset.x == -1 && offset.y == 0)
                {
                    leftNode = navNode;
                }
                else if (offset.x == 0 && offset.y == 1)
                {
                    upNode = navNode;
                }
                else if (offset.x == 0 && offset.y == -1)
                {
                    downNode = navNode;
                }
            }
        }
        Debug.Log("node " + gameObject + " has connections " + rightNode + leftNode + upNode + downNode);
    }
}
