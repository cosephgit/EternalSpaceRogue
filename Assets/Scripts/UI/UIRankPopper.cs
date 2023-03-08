using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// simple script which makes "RANK UP" text appear and pop when the player ranks up

public class UIRankPopper : MonoBehaviour
{
    [SerializeField]GameObject rankBox;
    [SerializeField]TextMeshProUGUI rankText;
    [SerializeField]float popScale = 2f; // how much does the rank pop box scale up?
    [SerializeField]float popDuration = 1f; // how long does the rank pop box take to pop?
    [SerializeField]float popLoiter = 2f; // how long does the rank pop box loiter after popping?
    [SerializeField]Color popColor = Color.yellow;
    [SerializeField]float popColorSwitch = 0.1f;
    bool popColored = false;

    void Awake()
    {
        rankBox.SetActive(false);
    }

    public void PopRank()
    {
        StopAllCoroutines();
        StartCoroutine(PopBox());
    }

    IEnumerator PopBox()
    {
        float popTime = 0;
        float popTimeCOlor = 0;

        rankBox.SetActive(true);

        do
        {
            popTime += Time.deltaTime;
            popTimeCOlor += Time.deltaTime;
            if (popTimeCOlor > popColorSwitch)
            {
                popTimeCOlor = 0;
                if (popColored) rankText.color = Color.white;
                else rankText.color = popColor;
                popColored = !popColored; 
            }
            float scale = Mathf.Clamp(Mathf.Lerp(1f, popScale, popTime / popDuration), 1f, popScale);
            rankBox.transform.localScale = new Vector3(scale, scale, scale);
            yield return new WaitForEndOfFrame();
        }
        while (popTime < popDuration);

        gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        rankText.color = Color.white;
        yield return new WaitForSeconds(popLoiter);
        rankBox.SetActive(false);
    }
}
