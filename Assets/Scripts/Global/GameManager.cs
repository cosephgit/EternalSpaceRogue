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
