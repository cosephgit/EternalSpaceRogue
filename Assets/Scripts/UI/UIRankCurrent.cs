using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// used to indicate the current rank and stage

public class UIRankCurrent : MonoBehaviour
{
    [SerializeField]TextMeshProUGUI number;
    [SerializeField]float popScale = 1.5f; // how much does the rank pop box scale up?
    [SerializeField]float popDuration = 1f; // how long does the rank pop box take to pop?
    [SerializeField]Color popColor = Color.yellow;
    [SerializeField]float popColorSwitch = 0.1f;
    bool popColored = false;
    int current = -1;

    public void UpdateNumber(int newNumber)
    {
        if (newNumber != current)
        {
            number.text = "" + newNumber;
            StopAllCoroutines();
            StartCoroutine(PopBox());
        }
    }

    IEnumerator PopBox()
    {
        float popTime = 0;
        float popTimeCOlor = 0;

        do
        {
            popTime += Time.deltaTime;
            popTimeCOlor += Time.deltaTime;
            if (popTimeCOlor > popColorSwitch)
            {
                popTimeCOlor = 0;
                if (popColored) number.color = Color.white;
                else number.color = popColor;
                popColored = !popColored; 
            }
            float scale = Mathf.Clamp(Mathf.Lerp(1f, popScale, popTime / popDuration), 1f, popScale);
            gameObject.transform.localScale = new Vector3(scale, scale, scale);
            yield return new WaitForEndOfFrame();
        }
        while (popTime < popDuration);

        gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        number.color = Color.white;
    }
}
