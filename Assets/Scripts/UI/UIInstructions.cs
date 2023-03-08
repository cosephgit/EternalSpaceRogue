using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// controls the instructions currently shown on the UI

public enum Instruction
{
    Move,
    Attack,
    Run
}

public class UIInstructions : MonoBehaviour
{
    [SerializeField]Image background;
    [SerializeField]GameObject moveText;
    [SerializeField]GameObject moveAttack;
    [SerializeField]GameObject moveRun;
    [SerializeField]GameObject textAny;
    [SerializeField]TextMeshProUGUI textAnyTitle;
    [SerializeField]TextMeshProUGUI textAnyBody;

    void Awake()
    {
        HideInstructions();
    }

    public void ShowInstruction(string anyTitle, string anyBody)
    {
        background.enabled = true;
        moveText.SetActive(false);
        moveAttack.SetActive(false);
        moveRun.SetActive(false);
        textAny.SetActive(true);
        textAnyBody.text = anyBody;
        textAnyBody.text = anyBody;
    }

    public void ShowInstruction(Instruction type)
    {
        background.enabled = true;
        switch (type)
        {
            default:
            case Instruction.Move:
            {
                moveText.SetActive(true);
                moveAttack.SetActive(false);
                moveRun.SetActive(false);
                textAny.SetActive(false);
                break;
            }
            case Instruction.Attack:
            {
                moveText.SetActive(false);
                moveAttack.SetActive(true);
                moveRun.SetActive(false);
                textAny.SetActive(false);
                break;
            }
            case Instruction.Run:
            {
                moveText.SetActive(false);
                moveAttack.SetActive(false);
                moveRun.SetActive(true);
                textAny.SetActive(false);
                break;
            }
        }
    }

    public void HideInstructions()
    {
        background.enabled = false;
        moveText.SetActive(false);
        moveAttack.SetActive(false);
        moveRun.SetActive(false);
        textAny.SetActive(false);
    }
}
