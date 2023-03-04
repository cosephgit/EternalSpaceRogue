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
    [SerializeField]TextMeshProUGUI moveText;
    [SerializeField]TextMeshProUGUI moveAttack;
    [SerializeField]TextMeshProUGUI moveRun;
    [SerializeField]TextMeshProUGUI textAny;

    void Awake()
    {
        HideInstructions();
    }

    public void ShowInstruction(string any)
    {
        background.enabled = true;
        moveText.enabled = false;
        moveAttack.enabled = false;
        moveRun.enabled = false;
        textAny.enabled = true;
        textAny.text = any;
    }

    public void ShowInstruction(Instruction type)
    {
        background.enabled = true;
        switch (type)
        {
            default:
            case Instruction.Move:
            {
                moveText.enabled = true;
                moveAttack.enabled = false;
                moveRun.enabled = false;
                textAny.enabled = true;
                break;
            }
            case Instruction.Attack:
            {
                moveText.enabled = false;
                moveAttack.enabled = true;
                moveRun.enabled = false;
                textAny.enabled = true;
                break;
            }
            case Instruction.Run:
            {
                moveText.enabled = false;
                moveAttack.enabled = false;
                moveRun.enabled = true;
                textAny.enabled = true;
                break;
            }
        }
    }

    public void HideInstructions()
    {
        background.enabled = false;
        moveText.enabled = false;
        moveAttack.enabled = false;
        moveRun.enabled = false;
        textAny.enabled = false;
    }
}
