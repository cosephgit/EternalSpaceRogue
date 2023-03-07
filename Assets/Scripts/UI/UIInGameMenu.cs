using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// this manages the defeat, victory and pause menu behaviour
// OPTION 1: start over/next stage/continue
// OPTION 2: options menu
// OPTION 3: instructions
// OPTION 4: quit to menu

public class UIInGameMenu : MonoBehaviour
{
    [SerializeField]TextMeshProUGUI menuTitle;
    [SerializeField]TextMeshProUGUI continueText;
    [SerializeField]GameObject menuMainHolder;
    [SerializeField]GameObject menuInstructHolder;
    [SerializeField]GameObject menuOptionsHolder;
    [SerializeField]GameObject scoreBoard;
    [SerializeField]Slider sliderMasterVol;
    [SerializeField]Slider sliderSFXVol;
    [SerializeField]Slider sliderMusicVol;
    int state = 0; // 0: paused, 1: victory, 2: defeat
    int menu = 0; // 0: main, 1: instructions, 2: options

    void Start()
    {
        sliderMasterVol.value = GameManager.instance.GetVolumeMaster();
        sliderSFXVol.value = GameManager.instance.GetVolumeSFX();
        sliderMusicVol.value = GameManager.instance.GetVolumeMusic();
    }

    // called when the menu opens
    public void MenuOpen(int gameState)
    {
        gameObject.SetActive(true);
        state = gameState;
        menuMainHolder.SetActive(true);
        menuInstructHolder.SetActive(false);
        menuOptionsHolder.SetActive(false);
        menu = 0;
        switch (state)
        {
            default:
            case 0: // paused game
            {
                menuTitle.text = "PAUSED";
                continueText.text = "CONTINUE";
                break;
            }
            case 1: // victory
            {
                menuTitle.text = "SUCCESS";
                continueText.text = "NEXT SECTOR";
                break;
            }
            case 2: // defeat
            {
                menuTitle.text = "DEATH";
                continueText.text = "START AGAIN";
                break;
            }
        }
    }

    // button hook, flexible
    public void PressContinue()
    {
        // if defeated: start again
        // if victorious: next sector
        // if paused: continue game
        switch (state)
        {
            default:
            case 0: // paused game
            {
                // resume
                StageManager.instance.MenuClosed();
                gameObject.SetActive(false);
                break;
            }
            case 1: // victory
            {
                // start new sector
                StageManager.instance.NewStage();
                gameObject.SetActive(false);
                // TODO increment difficulty!
                //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
            }
            case 2: // defeat
            {
                // start new game
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
            }
        }
    }

    // button hook, show options
    public void PressOptions()
    {
        menuMainHolder.SetActive(false);
        menuInstructHolder.SetActive(false);
        menuOptionsHolder.SetActive(true);
        menu = 2;
    }

    // button hook, show instructions
    public void PressInstructions()
    {
        menuMainHolder.SetActive(false);
        menuInstructHolder.SetActive(true);
        menuOptionsHolder.SetActive(false);
        menu = 1;
    }

    // button hook, return to main menu
    public void PressQuit()
    {
        SceneManager.LoadScene(0);
    }

    // button hook, return to in-game menu (closes instructions/options/confirm dialogue and returns to default menu state)
    // also called by the StageManager if Escape is pressed - so cascades down to closing the menu (if in the pause state ONLY, don't quit just from escape key!)
    public void PressReturn()
    {
        if (menu == 0 && state == 0)
        {
            // only called by pressing Escape, so only close menu if we're on the base menu and the game HASNT ended
            PressContinue();
        }
        else
        {
            if (menu == 2)
            {
                // leaving options menu, save any changes
                PlayerPrefs.Save();
            }
            menuMainHolder.SetActive(true);
            menuInstructHolder.SetActive(false);
            menuOptionsHolder.SetActive(false);
            menu = 0;
        }
    }

    public void SliderMasterVolume(System.Single vol)
    {
        GameManager.instance.SetVolumeMaster(vol);
    }
    public void SliderMusicVolume(System.Single vol)
    {
        GameManager.instance.SetVolumeMaster(vol);
    }
    public void SliderSFXVolume(System.Single vol)
    {
        GameManager.instance.SetVolumeSFX(vol);
    }
}
