using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIWeaponManager : MonoBehaviour
{
    [SerializeField]UIHealthBar weaponCurrent;
    [SerializeField]TextMeshProUGUI fistText; // shows the fist equipped when the weapon is currently holstered
    [SerializeField]float weaponIdleXShift = -0.1f;
    [SerializeField]RectTransform rect;
    bool fistActive;
    Vector3 weaponDefault;
    Vector3 weaponOffset;
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
            weaponOffset = weaponDefault;
            weaponOffset.x = weaponOffset.x * (1 + weaponIdleXShift);
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
            rect.position = weaponOffset;
        }
        else
        {
            fistText.enabled = false;
            rect.position = weaponDefault;
        }
    }
}
