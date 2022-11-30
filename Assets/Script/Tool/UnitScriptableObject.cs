using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitStat", menuName = "Scriptable Object Asset/UnitStat")]
public class UnitScriptableObject : ScriptableObject
{
    [field: SerializeField] public int damage;

    [field: SerializeField] public GameObject enableVfx;
    [field: SerializeField] public AudioClip enableSfx;

    [field: SerializeField] public GameObject disableVfx;
    [field: SerializeField] public AudioClip disableSfx; 
}
