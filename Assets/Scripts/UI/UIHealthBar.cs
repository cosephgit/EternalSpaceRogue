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
    float healthValue = -1; // make sure it's updated on level load

    // takes the maximum and current values of health and updates the bar and counter
    public void UpdateHealth(float health, float healthMax, int showOffset = 0)
    {
        float healthView = Mathf.Ceil(health * 10) * 0.1f; // round to 1 d.p
        float healthNew;
        if (showOffset > 0)
        {
            // this is used just for the xp bar
            // it adjusts the scale of the bar from just amount out of max to amount out of max for just the current level
            // e.g. if level 1 requires 10xp and level 2 requires 26 xp, it will show your progress from 10 (0 on the bar) to 26 (1 on the bar)
            healthNew = (health - showOffset) / (healthMax - showOffset);
        }
        else
        {
            healthNew = health / healthMax;
        }

        healthValue = healthNew;
        healthSlider.value = healthNew;
        if (healthView < 0) healthView = 0;

        if (showOffset > 0)
        {
            healthCount.text = "" + healthView + " / " + healthMax;
        }
        else
        {
            healthCount.text = "" + healthView;
        }
    }

    public void UpdateTitle(string title)
    {
        sliderTitle.text = title;
    }
}
