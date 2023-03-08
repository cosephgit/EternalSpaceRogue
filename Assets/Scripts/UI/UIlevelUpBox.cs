using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIlevelUpBox : MonoBehaviour
{
    [SerializeField]UIlevelUpPip[] pips;
    [SerializeField]Button button;
    int levelCurrent; // current level of this skill
    public int levelDesired { get; private set; }

    // set the correct initial number of pips and button state
    public void Initialise(int levelSet)
    {
        if (levelSet >= pips.Length)
        {
            button.interactable = false;
            levelSet = pips.Length;
        }
        else button.interactable = true;

        levelCurrent = levelSet;
        levelDesired = levelSet;

        for (int i = 0; i < pips.Length; i++)
        {
            if (i < levelSet)
                pips[i].Light();
            else
                pips[i].Dark();
        }
    }

    public void OutOfRanks()
    {
        button.interactable = false;
    }

    // button interface for choosing this upgrade
    public void ChooseUpgrade()
    {
        if (levelDesired < pips.Length)
        {
            if (UIManager.instance.levelUpMenu.LevelUpAvailable())
            {
                pips[levelDesired].Light();
                levelDesired++;
                // report back to the level up manager that a point has been spent
                UIManager.instance.levelUpMenu.LevelUpSelected();

                if (levelDesired == pips.Length) button.interactable = false;
            }
        }
    }
}
