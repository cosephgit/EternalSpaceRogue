using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIWeaponManager : MonoBehaviour
{
    [SerializeField]UIHealthBar weaponCurrent;
    [SerializeField]TextMeshProUGUI fistText; // shows the fist equipped when the weapon is currently holstered or absent
    [SerializeField]RectTransform rect;
    [SerializeField]RectTransform fistReadyPlaceholder; // this is an empty UI object that marks the position the weapon manager should be in while the player is using fists
    [SerializeField]float popScale = 1.25f; // how much does the rank pop box scale up?
    [SerializeField]float popDuration = 1f; // how long does the rank pop box take to pop?
    [SerializeField]Color popColor = Color.yellow;
    [SerializeField]float popColorSwitch = 0.1f;
    bool fistActive;
    Vector3 weaponDefault;
    bool getReady = true;

    void Start()
    {
        Initialise();
    }
    void Initialise()
    {
        if (getReady)
        {
            weaponDefault = rect.position;
            getReady = false;
            fistActive = false;
            fistText.enabled = false;
        }
    }

    public void UpdateAmmo(int ammo, int ammoMax)
    {
        weaponCurrent.UpdateHealth(ammo, ammoMax);
    }
    public void UpdateWeapon(string gunName)
    {
        weaponCurrent.UpdateTitle(gunName);
    }
    public void UpdateFist(string fistName)
    {
        fistText.text = fistName;
    }
    // put the first into the foreground (either because player is saving ammo or has run out)
    public void ReadyFist(bool ready)
    {
        Initialise();
        if (fistActive == ready) return;
        fistActive = ready;
        if (fistActive)
        {
            fistText.enabled = true;
            StopCoroutine(PopFist());
            StartCoroutine(PopFist());
            rect.position = fistReadyPlaceholder.position;
        }
        else
        {
            fistText.enabled = false;
            weaponCurrent.UpdateTitle();// pop the weapon title
            rect.position = weaponDefault;
        }
    }

    IEnumerator PopFist()
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
                    fistText.color = Color.white;
                }
                else
                {
                    fistText.color = popColor;
                }
                popColored = !popColored; 
            }
            float scale = Mathf.Clamp(Mathf.Lerp(1f, popScale, popTime / popDuration), 1f, popScale);
            fistText.transform.localScale = new Vector3(scale, scale, scale);
            yield return new WaitForEndOfFrame();
        }
        while (popTime < popDuration);

        fistText.transform.localScale = new Vector3(1f, 1f, 1f);
        fistText.color = Color.white;
    }
}
