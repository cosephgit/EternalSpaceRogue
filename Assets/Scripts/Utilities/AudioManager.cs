using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

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
        RuntimeManager.StudioSystem.setParameterByName("VolumeMaster", volume);
    }

    public void SetMusicVolume(float volume)
    {
        RuntimeManager.StudioSystem.setParameterByName("VolumeMusic", volume);
    }

    public void SetSFXVolume(float volume)
    {
        RuntimeManager.StudioSystem.setParameterByName("VolumeSFX", volume);
    }

    public void UpdateIntensity(float intensity)
    {
        Debug.Log("New intensity: " + intensity);
        RuntimeManager.StudioSystem.setParameterByName("Intensity", Mathf.Clamp(intensity, 0f, 1f));
    }

    public void UpdateHealth(float health)
    {
        Debug.Log("New health: " + health);
        RuntimeManager.StudioSystem.setParameterByName("Health", Mathf.Clamp(health, 0f, 1f));
    }

    public void PlayOneShot(EventReference sound, Vector3 pos)
    {
        RuntimeManager.PlayOneShot(sound, pos);
    }

    public EventInstance CreateEventInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        return eventInstance;
    }
}
