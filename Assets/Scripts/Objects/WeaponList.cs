using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is a simple class which contains a list of weapons
// used to have pre-generated lists of a certain type, which can be applied to enemy or loot prefabs to centralise the weapon lists

public class WeaponList : MonoBehaviour
{
    [SerializeField]WeaponBase[] weaponOptions;

    public WeaponBase Select(float maxStrength = -1)
    {
        if (weaponOptions.Length > 0)
        {
            List<WeaponBase> weaponsLimited = new List<WeaponBase>();

            if (maxStrength >= 0)
            {
                WeaponBase weaponWeakest = weaponOptions[0];
                // if strength is provided, this means there's a limit on how powerful a weapon can spawn
                for (int i = 0; i < weaponOptions.Length; i++)
                {
                    if (weaponOptions[i].threatLevel < weaponWeakest.threatLevel)
                        weaponWeakest = weaponOptions[i];

                    if (weaponOptions[i].threatLevel <= maxStrength)
                        weaponsLimited.Add(weaponOptions[i]);
                }
                if (weaponsLimited.Count == 0)
                    weaponsLimited.Add(weaponWeakest);
            }
            else weaponsLimited.AddRange(weaponOptions); // add all weapons

            return weaponsLimited[Random.Range(0, weaponsLimited.Count)];
        }
        return null;
    }
}
