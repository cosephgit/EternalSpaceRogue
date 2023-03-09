using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField]StudioEventEmitter audioMusic;

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

    public void SetMasterVolume(float volume)
    {
    }

    public void SetMusicVolume(float volume)
    {

    }

    public void SetSFXVolume(float volume)
    {

    }

    public void UpdateIntensity(float intensity)
    {
        Debug.Log("New intensity: " + intensity);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Intensity", Mathf.Clamp(intensity, 0f, 1f));
    }

    public void UpdateHealth(float health)
    {
        Debug.Log("New health: " + health);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Health", Mathf.Clamp(health, 0f, 1f));
    }
}
