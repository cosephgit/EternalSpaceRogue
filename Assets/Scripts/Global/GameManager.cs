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
    public float screenCellWidth { get; private set; } = 10f;
    public float screenCellHeight { get; private set; } = 6f;

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

        screenCellHeight = Camera.main.orthographicSize + 0.5f;
        screenCellWidth = ((float)Camera.main.pixelWidth * (float)Camera.main.orthographicSize / (float)Camera.main.pixelHeight) + 0.5f;
    }
}
