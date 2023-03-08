using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

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
    float volumeMaster;
    float volumeSFX;
    float volumeMusic;

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

        SetVolumeMaster(PlayerPrefs.GetFloat(Global.VOLMASTER, 1));
        SetVolumeSFX(PlayerPrefs.GetFloat(Global.VOLSFX, 1));
        SetVolumeMusic(PlayerPrefs.GetFloat(Global.VOLMUSIC, 1));
    }

    void Start()
    {
    }

    public void SetVolumeMaster(float volume)
    {
        volumeMaster = volume;
        // TODO set FMOD volume to this
        Debug.Log("Master volume is " + volumeMaster);
        Global.VolToDecibelsScaled(volumeMaster);
        PlayerPrefs.SetFloat(Global.VOLMASTER, volumeMaster);
    }

    public void SetVolumeSFX(float volume)
    {
        volumeSFX = volume;
        // TODO set FMOD volume to this
        Debug.Log("SFX volume is " + volumeSFX);
        Global.VolToDecibelsScaled(volumeSFX);
        PlayerPrefs.SetFloat(Global.VOLMASTER, volumeSFX);
    }

    public void SetVolumeMusic(float volume)
    {
        volumeMusic = volume;
        // TODO set FMOD volume to this
        Debug.Log("Music volume is " + volumeMusic);
        Global.VolToDecibelsScaled(volumeMusic);
        PlayerPrefs.SetFloat(Global.VOLMASTER, volumeMusic);
    }

    public float GetVolumeMaster()
    {
        return volumeMaster;
    }
    public float GetVolumeSFX()
    {
        return volumeSFX;
    }
    public float GetVolumeMusic()
    {
        return volumeMusic;
    }
}
