using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is a simple class which contains a list of weapons
// used to have pre-generated lists of a certain type, which can be applied to enemy or loot prefabs to centralise the weapon lists

public class WeaponList : MonoBehaviour
{
    [SerializeField]WeaponBase[] weaponOptions;

    public WeaponBase Select()
    {
        if (weaponOptions.Length > 0)
        {
            return weaponOptions[Random.Range(0, weaponOptions.Length)];
        }
        return null;
    }
}
