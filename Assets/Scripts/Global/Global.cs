using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// global static class
// stores globally used variables and static methods

public static class Global
{
    public const int TILEMAPDIMS = 14; // this is the required x and y dimension for all tilemap segments
    // pathfinding limitations
    public const int PATHFINDMAX = 25; // the maximum number of cells to ever pathfind for
    // collision layer index references
    private const string LAYERWALL = "Default";
    private const string LAYERPAWN = "Pawn";
    private const string LAYERFLOOR = "Floor";
    private const string LAYERNAV = "NavNode";
    private const string LAYEROBJ = "Objective";
    public static float scalePawnSpeed = 1f; // this can be used to adjust the rate at which pawns move from space to space



    // used to check for a place to move
    public static LayerMask LayerFloor()
    {
        return LayerMask.GetMask(LAYERFLOOR);
    }
    // used to check for barriers in movement
    public static LayerMask LayerWall()
    {
        return LayerMask.GetMask(LAYERWALL);
    }
    // used to check for barriers in movement
    public static LayerMask LayerNav()
    {
        return LayerMask.GetMask(LAYERNAV);
    }
    // used to check for barriers in movement
    public static LayerMask LayerPawn()
    {
        return LayerMask.GetMask(LAYERPAWN);
    }
    // used to check for anything at all (e.g. the presence of a tilemap)
    public static LayerMask LayerTerrain()
    {
        return LayerMask.GetMask(new string[2] { LAYERWALL, LAYERFLOOR });
    }
    public static LayerMask LayerObstacle()
    {
        return LayerMask.GetMask(new string[2] { LAYERWALL, LAYERPAWN });
    }
    public static LayerMask LayerObstacleNoActionZones() // returns a layer with all physical obstacles AND all action zones (e.g. the exit zone) blocked too
    {
        return LayerMask.GetMask(new string[3] { LAYERWALL, LAYERPAWN, LAYEROBJ });
    }

    public static bool ApproxVector(Vector2 a, Vector2 b)
    {
        return (Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y));
    }
    public static int OrthogonalDist(Vector3 a, Vector3 b)
    {
        Vector3 offset = a - b;

        return Mathf.CeilToInt(Mathf.Abs(offset.x) + Mathf.Abs(offset.y));
    }
    public static string VectorListToString(List<Vector3> route)
    {
        string output = "";

        for (int i = 0; i < route.Count; i++)
        {
            output += route[i] + " ";
        }

        return output;
    }
    public static bool RandomBool()
    {
        return (Random.Range(0, 2) == 0);
    }
}
