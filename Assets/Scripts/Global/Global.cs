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
    // loot chances
    public const float DROPCHANCEBASE = 0; // the basic chance of a weapon being dropped by a dying enemy
    public const float DROPCHANCEBYAMMO = 0.5f; // the chance of a 100% loaded weapon being dropped (scaling to 0 at 0% ammo)
    // xp variables
    public const float XPPERSTRENGTH = 2f;
    public const float XPPERLEVELBASE = 10f;
    public const float XPPERLEVELEXPONENT = 1.2f;
    // collision layer index references
    private const string LAYERWALL = "Default";
    private const string LAYERPAWN = "Pawn";
    private const string LAYERFLOOR = "Floor";
    private const string LAYERNAV = "NavNode";
    private const string LAYEROBJ = "Objective";
    private const string LAYERPOWER = "Powerup";
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
    // used to check for powerups in a space
    public static LayerMask LayerPower()
    {
        return LayerMask.GetMask(LAYERPOWER);
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
    public static string VectorListToString(Vector3[] route)
    {
        string output = "";

        for (int i = 0; i < route.Length; i++)
        {
            output += route[i] + " ";
        }

        return output;
    }
    public static string VectorListToString(List<Vector3> route)
    {
        return VectorListToString(route.ToArray());
    }
    public static bool RandomBool()
    {
        return (Random.Range(0, 2) == 0);
    }
}
