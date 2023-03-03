using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// the central UI manager
// this is a singleton
// it contains references to all other UI elements

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public UIHealthBar healthBar;

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
        instance = this;
    }
}
