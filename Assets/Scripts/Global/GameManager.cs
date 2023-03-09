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
    public int[] scores { get; private set; } = new int[10];
    private float volumeMaster;
    private float volumeSFX;
    private float volumeMusic;

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

        LoadScores();
    }

    string ScoreKey(int index)
    {
        return (Global.KEYSCORE + index);
    }

    // load high scores from playerprefs (or initialise them with defaults)
    void LoadScores()
    {
        for (int i = 0; i < scores.Length; i++)
        {
            scores[i] = PlayerPrefs.GetInt(ScoreKey(i), 0);
        }
    }

    // save high scores to playerprefs
    void SaveScores()
    {
        for (int i = 0; i < scores.Length; i++)
        {
            PlayerPrefs.SetInt(ScoreKey(i), scores[i]);
        }
        PlayerPrefs.Save();
    }

    // takes in a new score value and returns an index for the score position (or -1 if it fell out of position)
    public int NewScore(int score)
    {
        int ranking = -1;

        // find the new score position in the ranking
        for (int i = 0; i < scores.Length; i++)
        {
            if (ranking == -1)
            {
                if (score > scores[i])
                {
                    ranking = i;
                }
            }
        }

        // update scores if the new score made the top scores
        if (ranking != -1)
        {
            for (int i = scores.Length - 1; i >= ranking; i--)
            {
                if (i == ranking)
                    scores[i] = score;
                else
                    scores[i] = scores[i-1];
            }
            SaveScores();
        }

        return ranking;
    }

    void Start()
    {
        SetVolumeMaster(PlayerPrefs.GetFloat(Global.KEYVOLMASTER, 1));
        SetVolumeSFX(PlayerPrefs.GetFloat(Global.KEYVOLSFX, 1));
        SetVolumeMusic(PlayerPrefs.GetFloat(Global.KEYVOLMUSIC, 1));
    }

    public void SetVolumeMaster(float volume)
    {
        volumeMaster = volume;
        PlayerPrefs.SetFloat(Global.KEYVOLMASTER, volume); // store the Unity-scaled value
        AudioManager.instance.SetMasterVolume(Global.VolToDecibelsScaled(volume)); // the actual volume change to FMOD with the log scale adjustment
    }

    public void SetVolumeSFX(float volume)
    {
        volumeSFX = volume;
        PlayerPrefs.SetFloat(Global.KEYVOLSFX, volume);
        AudioManager.instance.SetSFXVolume(Global.VolToDecibelsScaled(volume));
    }

    public void SetVolumeMusic(float volume)
    {
        volumeMusic = volume;
        PlayerPrefs.SetFloat(Global.KEYVOLMUSIC, volume);
        AudioManager.instance.SetMusicVolume(Global.VolToDecibelsScaled(volume));
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
