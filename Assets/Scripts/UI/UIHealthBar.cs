using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHealthBar : MonoBehaviour
{
    [SerializeField]Slider healthSlider;
    [SerializeField]TextMeshProUGUI healthCount;
    [SerializeField]TextMeshProUGUI sliderTitle;
    float healthValue = -1;

    // takes the maximum and current values of health and updates the bar and counter
    public void UpdateHealth(float health, float healthMax)
    {
        float healthNew = health / healthMax;
        if (healthNew != healthValue)
        {
            float healthView = Mathf.Ceil(health * 10) * 0.1f; // round to 1 d.p
            healthValue = healthNew;
            healthSlider.value = healthNew;
            healthCount.text = "" + healthView;
        }
    }

    public void UpdateTitle(string title)
    {
        sliderTitle.text = title;
    }
}
