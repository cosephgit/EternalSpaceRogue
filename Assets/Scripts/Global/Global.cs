using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// global static class
// stores globally used variables and static methods

public static class Global
{
    public const int TILEMAPDIMS = 14; // this is the required x and y dimension for all tilemap segments
    // collision layer index references
    private const string LAYERWALL = "Default";
    private const string LAYERPAWN = "Pawn";
    private const string LAYERFLOOR = "Floor";
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
    // used to check for anything at all (e.g. the presence of a tilemap)
    public static LayerMask LayerAll()
    {
        return LayerMask.GetMask(new string[2] { LAYERWALL, LAYERFLOOR });
    }
    public static LayerMask LayerObstacle()
    {
        return LayerMask.GetMask(new string[2] { LAYERWALL, LAYERPAWN });
    }
}
