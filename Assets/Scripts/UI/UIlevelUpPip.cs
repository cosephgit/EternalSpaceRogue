using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIlevelUpPip : MonoBehaviour
{
    [SerializeField]Image pip; // the pip
    [SerializeField]Sprite pipLit; // the replacement sprite to use when this pip is activated
    [SerializeField]Color pipLitColor = Color.green; // the color when lit
    Sprite pipOriginal;

    void Awake()
    {
        pipOriginal = pip.sprite;
    }

    public void Light()
    {
        pip.sprite = pipLit;
        pip.color = pipLitColor;
    }

    public void Dark()
    {
        pip.sprite = pipOriginal;
        pip.color = Color.white;
    }
}
