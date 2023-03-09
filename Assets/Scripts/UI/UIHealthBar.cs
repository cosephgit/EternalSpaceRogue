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
    [SerializeField]float popScale = 1.25f; // how much does the rank pop box scale up?
    [SerializeField]float popDuration = 1f; // how long does the rank pop box take to pop?
    [SerializeField]Color popColor = Color.yellow;
    [SerializeField]float popColorSwitch = 0.1f;
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
        if (healthNew < 0) healthNew = 0;


        if (healthValue != healthNew)
        {
            healthValue = healthNew;
            StopCoroutine(PopBar());
            StartCoroutine(PopBar());
            healthSlider.value = healthNew;
            if (healthView < 0) healthView = 0;

            if (showOffset > 0)
            {
                healthCount.text = "" + healthView + "/" + healthMax;
            }
            else
            {
                healthCount.text = "" + healthView;
            }
        }
    }

    public void UpdateTitle(string title = "")
    {
        if (title != "")
            sliderTitle.text = title;
        StopCoroutine(PopTitle());
        StartCoroutine(PopTitle());
    }


    // what needs to happen here? make the text flash, make the box pop?
    IEnumerator PopBar()
    {
        float popTime = 0;
        float popTimeCOlor = 0;
        bool popColored = false;

        do
        {
            popTime += Time.deltaTime;
            popTimeCOlor += Time.deltaTime;
            if (popTimeCOlor > popColorSwitch)
            {
                popTimeCOlor = 0;
                if (popColored)
                {
                    healthCount.color = Color.white;
                }
                else
                {
                    healthCount.color = popColor;
                }
                popColored = !popColored; 
            }
            float scale = Mathf.Clamp(Mathf.Lerp(1f, popScale, popTime / popDuration), 1f, popScale);
            gameObject.transform.localScale = new Vector3(scale, scale, scale);
            yield return new WaitForEndOfFrame();
        }
        while (popTime < popDuration);

        gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        healthCount.color = Color.white;
    }

    IEnumerator PopTitle()
    {
        float popTime = 0;
        float popTimeCOlor = 0;
        bool popColored = false;

        do
        {
            popTime += Time.deltaTime;
            popTimeCOlor += Time.deltaTime;
            if (popTimeCOlor > popColorSwitch)
            {
                popTimeCOlor = 0;
                if (popColored)
                {
                    sliderTitle.color = Color.white;
                }
                else
                {
                    sliderTitle.color = popColor;
                }
                popColored = !popColored; 
            }
            float scale = Mathf.Clamp(Mathf.Lerp(1f, popScale, popTime / popDuration), 1f, popScale);
            sliderTitle.transform.localScale = new Vector3(scale, scale, scale);
            yield return new WaitForEndOfFrame();
        }
        while (popTime < popDuration);

        sliderTitle.transform.localScale = new Vector3(1f, 1f, 1f);
        sliderTitle.color = Color.white;
    }
}
