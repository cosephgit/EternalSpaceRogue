using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stores permissible pawn positions for creating pathfinding data
// might do other things later e.g. mark good places for doors/loot/etc

public enum Dir
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3,
    Null = 4
}

public class NavNode : MonoBehaviour
{
    // nodes to navigate to adjacent nodes
    public NavNode upNode { get; private set; }
    public NavNode rightNode { get; private set; }
    public NavNode downNode { get; private set; }
    public NavNode leftNode { get; private set; }
    // pathfinding data
    int pathStatus = 0;
    int pathGCost = 0;
    int pathHCost = 0;
    int pathFCost = 0;
    public NavNode pathPrev { get; private set; }


    // this is called after all NavNodes are placed to calculate node adjacency
    // the passed index is the identifying index for THIS node (for debug purposes)
    public void CheckConnection(int index)
    {
        Collider2D[] nodesAdjacent = Physics2D.OverlapCircleAll(transform.position, 1.1f, Global.LayerNav());

        name = "Nav" + index;

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
    }

    public void PathFind(NavNode target, NavNode previous, bool avoidPawns, int distMax)
    {// initiate pathfinding from this square to the target square
        List<int> path = new List<int>();

        // initialise all uninitialised adjacent nodes
        if (upNode) upNode.PathNodeSet(target, this, pathGCost, avoidPawns, distMax);
        if (rightNode) rightNode.PathNodeSet(target, this, pathGCost, avoidPawns, distMax);
        if (downNode) downNode.PathNodeSet(target, this, pathGCost, avoidPawns, distMax);
        if (leftNode) leftNode.PathNodeSet(target, this, pathGCost, avoidPawns, distMax);

        if (previous == null)
        {// this is the first node in the search
            pathStatus = 3;
        }
        else
        {
            pathStatus = 2;
        }

        StageManager.instance.navNodeDirty.Add(this);

        NavNode optimalNode = null;
        int optimalDist = 1000000;
        int optimalHDist = 1000000;
        foreach(NavNode node in StageManager.instance.navNodeMap)
        {
            if (node.pathStatus == 1)
            {
                if (node.pathFCost < optimalDist)
                {
                    optimalNode = node;
                    optimalDist = node.pathFCost;
                    optimalHDist = node.pathHCost;
                }
                else if (node.pathFCost == optimalDist)
                {
                    if (node.pathHCost < optimalHDist || Global.RandomBool())
                    {
                        // tie breaker - pick the node that is the shortest direct distance to the target to prioritise paths that will typically reach the target sooner
                        // or just randomly split if they're identical to create some enemy pathing variation
                        optimalNode = node;
                        optimalDist = node.pathFCost;
                        optimalHDist = node.pathHCost;
                    }
                }
            }
        }

        if (optimalNode)
        {// recursive function that hopefully wont cause an infinite loop
            if (Global.ApproxVector(optimalNode.transform.position, target.transform.position))
            {// found the target!... now what?
            // create the path list and send it to the caller
            }
            else
            {
                optimalNode.PathFind(target, this, avoidPawns, distMax);
            }
        }
    }
    // the pathing has now been created and is stored in the nodes, so the path does not need to be identified here

    // sets the pathfinding data to this node
    public void PathNodeSet(NavNode target, NavNode previous, int currentG, bool avoidPawns, int distMax)
    {// set up f value for this node
        if (pathStatus == 0)
        {// only set it up if it's not been set up already
            StageManager.instance.navNodeDirty.Add(this);
            if (avoidPawns && target != this)
            {
                // this node may have a blocking occupant
                Collider2D blocker = Physics2D.OverlapPoint(transform.position, Global.LayerPawn());
                if (blocker)
                {
                    pathStatus = 2; // blocked, don't consider this node for pathfinding
                    return;
                }
            }
            pathGCost = currentG + 1; // currently always 1 unit but terrain penalties may change this later
            pathHCost = PathHDist(target);
            pathFCost = pathGCost + pathHCost;
            if (pathFCost > distMax)
            {
                pathStatus = 2; // too far, don't consider this node for pathfinding
                return;
            }
            pathStatus = 1; // this square has been initialised
            pathPrev = previous;
        }
    }
    // clears all pathfinding data from this node
    // this needs to be done before all pathfinding checks
    public void PathClear()
    {
        pathStatus = 0;
        pathGCost = 0;
        pathHCost = 0;
        pathFCost = 0;
        pathPrev = null;
    }
    // calculates the H dist from this node to the target point
    // only calculates with orthogonal movement, no diagonals (since there is no diagonal movement allowed)
    public int PathHDist(NavNode target)
    {
        return Global.OrthogonalDist(target.transform.position, transform.position);
    }
    // returns the direction (as an integer) to travel from the origin to the current square
    public Vector3 PathDirFrom(NavNode origin)
    {
        if (origin.upNode == this) return Vector3.up;
        if (origin.rightNode == this) return Vector3.right;
        if (origin.downNode == this) return Vector3.down;
        if (origin.leftNode == this) return Vector3.left;

        Debug.Log("<color=orange>WARNING</color> Invalid SquareDirection");
        return Vector3.zero;
    }
}
