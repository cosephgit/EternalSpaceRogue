using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is used to indicate squares which the player can interact with (e.g. move, attack)
public enum IndicatorType
{
    Move,
    Run,
    Attack
}

public class SquareIndicator : MonoBehaviour
{
    [SerializeField]Color moveColor = Color.green;
    [SerializeField]Color moveColorFade = Color.green;
    [SerializeField]float moveCycleRate = 1;
    [SerializeField]Color runColor = Color.yellow;
    [SerializeField]Color runColorFade = Color.yellow;
    [SerializeField]float runCycleRate = 1;
    [SerializeField]Color attackColor = Color.red;
    [SerializeField]Color attackColorFade = Color.red;
    [SerializeField]float attackCycleRate = 2;
    [SerializeField]Color attackColorFlash = Color.red;
    [SerializeField]SpriteRenderer indicator;
    Color mainColor = Color.white; // default values to make errors really obvious
    Color fadeColor = Color.clear;
    float pulseCycleRate = 1;
    float pulseCycle = 0;


    public void InitIndicator(IndicatorType type)
    {
        switch (type)
        {
            default:
            case IndicatorType.Move:
            {
                mainColor = moveColor;
                fadeColor = moveColorFade;
                pulseCycleRate = moveCycleRate;
                break;
            }
            case IndicatorType.Run:
            {
                mainColor = runColor;
                fadeColor = runColorFade;
                pulseCycleRate = runCycleRate;
                break;
            }
            case IndicatorType.Attack:
            {
                mainColor = attackColor;
                fadeColor = attackColorFade;
                pulseCycleRate = attackCycleRate;
                break;
            }
        }
        indicator.color = mainColor;
        pulseCycle = 0;
    }

    void LateUpdate()
    {
        pulseCycle += Time.deltaTime * pulseCycleRate;
        indicator.color = Color.Lerp(mainColor, fadeColor, (Mathf.Cos(pulseCycle) - 1f) * -0.5f);
    }

    // used to make the selection square flash for a short time for an attack, then destroy itself
    public void Flash()
    {
        fadeColor = attackColor;
        mainColor = attackColorFlash;
        pulseCycle = 0;
        pulseCycleRate = 10;
    }
}
