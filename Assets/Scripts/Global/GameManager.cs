using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// game manager
// SINGLETON STRUCTURE
// PERSISTENT BETWEEN SCENES
// handles loading/saving game data
// handles scene transitions
// stores common non-static functions

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int screenCellWidth { get; private set; } = 10; // TODO actually calculate these later from the camera?
    public int screenCellHeight { get; private set; } = 6;

    void Awake()
    {
        if (instance)
        {
            if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        else instance = this;

        DontDestroyOnLoad(gameObject);
    }
}
