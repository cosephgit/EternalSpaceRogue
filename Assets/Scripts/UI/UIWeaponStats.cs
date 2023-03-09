using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIWeaponStats : MonoBehaviour
{
    [SerializeField]TextMeshProUGUI title;
    [SerializeField]TextMeshProUGUI damage;
    [SerializeField]TextMeshProUGUI range;
    [SerializeField]GameObject[] areaMarkersNeg1;
    [SerializeField]GameObject[] areaMarkersZero;
    [SerializeField]GameObject[] areaMarkersPos1;
    [SerializeField]GameObject[] areaMarkersPos2;

    public void UpdateWeapon(string titleNew, int damageNew, int rangeNew, Vector3[] offsetsNew)
    {
        title.text = titleNew;
        damage.text = "" + damageNew;
        range.text = "" + rangeNew;


    }
}
