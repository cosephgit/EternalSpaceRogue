using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField]Slider healthSlider;
    float healthValue;

    public void UpdateHealth(float health)
    {
        if (health != healthValue)
        {
            healthValue = health;
            healthSlider.value = health;
        }
    }
}
