using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// the PrefabProvider singleton is used to store commonly needed prefabs

public class PrefabProvider : MonoBehaviour
{
    public static PrefabProvider inst;
    [field: SerializeField]public SquareIndicator indicator { get; private set; }

    void Awake()
    {
        if (inst)
        {
            if (inst != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        inst = this;
    }
}
