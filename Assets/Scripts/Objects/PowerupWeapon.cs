using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupWeapon : PowerUpBase
{
    [SerializeField]WeaponList weapons;
    [SerializeField]Vector3 weapOffset;
    WeaponBase weaponActual;

    public override void PrepPowerup()
    {
        weaponActual = Instantiate(weapons.Select(), transform.position + weapOffset, Quaternion.identity);
        weaponActual.transform.parent = transform;
    }

    // this is used to instruct this power up to accept a pre-existing weapon
    public void AcceptWeapon(WeaponBase weapon)
    {
        weaponActual = weapon;
        weaponActual.transform.parent = transform;
        weaponActual.transform.position = transform.position;
    }

    public override bool TouchPowerup(PlayerPawn pawn)
    {
        if (!ready) return true;

        if (pawn.PickupWeapon(weaponActual))
        {
            Consume();
            return true;
        }
        return true;
    }

    public override float Quality()
    {
        return (weaponActual.threatLevel + quality);
    }
}
