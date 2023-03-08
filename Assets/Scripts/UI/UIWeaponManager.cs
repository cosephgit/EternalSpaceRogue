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
            rect.position = fistReadyPlaceholder.position;
        }
        else
        {
            fistText.enabled = false;
            rect.position = weaponDefault;
        }
    }
}
