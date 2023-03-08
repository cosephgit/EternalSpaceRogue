using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// global static class
// stores globally used variables and static methods

public static class Global
{
    public static float scalePawnSpeed = 1f; // this can be used to adjust the rate at which pawns move from space to space
    public const int TILEMAPDIMS = 14; // this is the required x and y dimension for all tilemap segments
    // pathfinding limitations
    public const int PATHFINDMAX = 25; // the maximum number of cells to ever pathfind for
    // loot chances
    public const float DROPCHANCEBASE = 0; // the basic chance of a weapon being dropped by a dying enemy
    public const float DROPCHANCEBYAMMO = 0.5f; // the chance of a 100% loaded weapon being dropped (scaling to 0 at 0% ammo)
    // difficulty constants
    public const float BONUSLOOTEXPONENT = 1.05f; // base to raise by level for loot calculation (5% more loot each level)
    public const float BONUSENEMYEXPONENT = 1.1f; // base to raise by level for individual enemy strength calculation (10% more average enemy strength each level)
    public const float BONUSENEMIESEXPONENT = 1.2f; // base to raise by level for total enemy strength calculation (20% more total enemy strength each level)
    // xp constants
    public const float XPPERSTAGEBASE = 10f; // XP reward for reaching a stage exit
    public const float XPPERSTAGEEXPONENT = 1.4f; // exponent for stage difficulty
    public const float XPPERSTRENGTH = 2f; // // XP gained per point of enemy strength
    public const float XPPERLEVELBASE = 20f; // base XP needed per level
    public const float XPPERLEVELEXPONENT = 1.4f;
    // save data keys
    public const string VOLMASTER = "VolumeMaster";
    public const string VOLSFX = "VolumeSFX";
    public const string VOLMUSIC = "VolumeMusic";
    // collision layer index references
    private const string LAYERWALL = "Default";
    private const string LAYERPAWN = "Pawn";
    private const string LAYERFLOOR = "Floor";
    private const string LAYERNAV = "NavNode";
    private const string LAYEROBJ = "Objective";
    private const string LAYERPOWER = "Powerup";


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

    public static float VolToDecibels(float vol)
    {
        float decibels;
        if (vol < 0.01f)
        {
            // can't do log 0
            decibels = -80f;
        }
        else
        {
            decibels = Mathf.Log(vol, 2f); // so each halving of volume is -1
            decibels *= 10f; // -10 decibels is approximately half volume
        }
        return decibels;
    }
    // this is specifically to interact with FMOD in a user-friendly way, so when a value from 0 to 1 is sent to FMOD it's ALREADY adjusted so it can be linearly applied to decibels
    public static float VolToDecibelsScaled(float vol)
    {
        float volScaled = VolToDecibels(vol);

        volScaled = Mathf.Clamp((volScaled + 80f) / 80f, 0f, 1f);

        return volScaled;
    }
    public static float DecibelsToVol(float dec)
    {
        float volume;
        if (dec < -65f)
        {
            volume = 0f;
        }
        else
        {
            volume = dec * 0.1f; // a value from -80 to 0 -> -8 to 0
            volume = Mathf.Pow(2, volume); // a value from 0 to 
        }
        return volume;
    }
}
