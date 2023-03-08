using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UILevelUp : MonoBehaviour
{
    const int SUPERRANKHP = 2;
    [SerializeField]UIlevelUpBox levelStrength;
    [SerializeField]UIlevelUpBox levelTough;
    [SerializeField]UIlevelUpBox levelAmmo;
    [SerializeField]UIlevelUpBox levelSupply;
    [SerializeField]UIlevelUpBox levelTerror;
    [SerializeField]UIlevelUpBox levelMedic;
    [SerializeField]TextMeshProUGUI levelUpsRank;
    [SerializeField]TextMeshProUGUI levelUpsText;
    [SerializeField]Button buttonReset;
    [SerializeField]Button buttonConfirm;
    [SerializeField]GameObject LevelSuper;
    [SerializeField]TextMeshProUGUI LevelSuperText;
    private int levelUpsToChoose;
    private int levelUpsLeft;

    // open the level up menu with the indicated number of level ups
    public void OpenLevelUps(int levelUps)
    {
        if (levelUps > 0)
        {
            gameObject.SetActive(true);
            levelUpsRank.text = "Rank " + StageManager.instance.playerPawn.GetRank() + " reached";
            levelUpsToChoose = levelUps;
            if (StageManager.instance.playerPawn.GetRank() > 19)
            {
                levelUpsLeft = levelUps;
                if (StageManager.instance.playerPawn.GetRank() - 19 < levelUps)
                {
                    levelUpsLeft = StageManager.instance.playerPawn.GetRank() - 19; // will be >0
                }
                LevelSuper.SetActive(true);
                LevelSuperText.text = "+" + (levelUpsLeft * SUPERRANKHP);
                // all skills are capped and the player gets a special bonus!
                levelStrength.Initialise(3);
                levelTough.Initialise(3);
                levelAmmo.Initialise(3);
                levelSupply.Initialise(3);
                levelTerror.Initialise(3);
                levelMedic.Initialise(3);
                DisableAll();
                buttonConfirm.enabled = true;
                buttonReset.enabled = false;
                levelUpsText.text = "" + levelUpsLeft;
                levelUpsText.enabled = true;
            }
            else
            {
                LevelSuper.SetActive(false);
                PressReset();
            }
        }
        else
        {
            StageManager.instance.LevelUpsDone(); // shouldn't have been called
        }
    }

    // one of the upgrade options has been selected, upgrade the points left to reflect this
    public void LevelUpSelected()
    {
        if (levelUpsLeft > 0)
        {
            levelUpsLeft--;
            levelUpsText.text = "" + levelUpsLeft;
            if (levelUpsLeft == 0)
            {
                DisableAll();
                buttonConfirm.enabled = true;
            }
            buttonReset.enabled = true;
        }
    }

    public bool LevelUpAvailable()
    {
        return (levelUpsLeft > 0);
    }

    // resets all available level ups
    public void PressReset()
    {
        // TODO get all the current player ranks in the skills
        levelUpsLeft = levelUpsToChoose;
        levelStrength.Initialise(StageManager.instance.playerPawn.upgradeStrength);
        levelTough.Initialise(StageManager.instance.playerPawn.upgradeTough);
        levelAmmo.Initialise(StageManager.instance.playerPawn.upgradeAmmo);
        levelSupply.Initialise(StageManager.instance.playerPawn.upgradeSupply);
        levelTerror.Initialise(StageManager.instance.playerPawn.upgradeTerror);
        levelMedic.Initialise(StageManager.instance.playerPawn.upgradeMedic);
        levelUpsText.text = "" + levelUpsLeft;
        levelUpsText.enabled = true;
        buttonReset.enabled = false;
        buttonConfirm.enabled = false;
    }

    public void DisableAll()
    {
        levelStrength.OutOfRanks();
        levelTough.OutOfRanks();
        levelAmmo.OutOfRanks();
        levelSupply.OutOfRanks();
        levelTerror.OutOfRanks();
        levelMedic.OutOfRanks();
    }

    public void PressConfirm()
    {
        if ((StageManager.instance.playerPawn.GetRank() > 19) || levelUpsLeft == 0)
        {
            if (StageManager.instance.playerPawn.GetRank() > 19)
                StageManager.instance.playerPawn.UpgradeRankSuperBonus(levelUpsLeft * SUPERRANKHP);

            StageManager.instance.playerPawn.UpgradeSkills(levelStrength.levelDesired, levelTough.levelDesired, levelAmmo.levelDesired,
                                                                 levelSupply.levelDesired, levelTerror.levelDesired, levelMedic.levelDesired);

            StageManager.instance.LevelUpsDone();
            gameObject.SetActive(false);
        }
    }
}
