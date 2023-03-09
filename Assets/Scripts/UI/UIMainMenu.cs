using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// manages the main menu screen

public class UIMainMenu : MonoBehaviour
{
    [SerializeField]GameObject menuMain;
    [SerializeField]GameObject menuOptions;
    [SerializeField]GameObject menuInstructions;
    [SerializeField]UIScoreBoard scoreBoard;
    [SerializeField]Slider sliderMasterVol;
    [SerializeField]Slider sliderSFXVol;
    [SerializeField]Slider sliderMusicVol;

    void Start()
    {
        PressReturn();
        sliderMasterVol.value = GameManager.instance.GetVolumeMaster();
        sliderSFXVol.value = GameManager.instance.GetVolumeSFX();
        sliderMusicVol.value = GameManager.instance.GetVolumeMusic();
        scoreBoard.UpdateScores();
    }
    public void PressNewGame()
    {
        SceneManager.LoadScene(1);
    }

    public void PressInstructions()
    {
        menuMain.SetActive(false);
        menuOptions.SetActive(false);
        menuInstructions.SetActive(true);
    }

    public void PressOptions()
    {
        menuMain.SetActive(false);
        menuOptions.SetActive(true);
        menuInstructions.SetActive(false);
    }

    public void PressQuit()
    {
        #if UNITY_EDITOR
        Debug.Log("QUIT!");
        #else
        Application.Quit();
        #endif
    }

    public void PressReturn()
    {
        menuMain.SetActive(true);
        menuOptions.SetActive(false);
        menuInstructions.SetActive(false);
    }

    public void SliderMasterVolume(System.Single vol)
    {
        GameManager.instance.SetVolumeMaster(vol);
    }
    public void SliderMusicVolume(System.Single vol)
    {
        GameManager.instance.SetVolumeMusic(vol);
    }
    public void SliderSFXVolume(System.Single vol)
    {
        GameManager.instance.SetVolumeSFX(vol);
    }
}
