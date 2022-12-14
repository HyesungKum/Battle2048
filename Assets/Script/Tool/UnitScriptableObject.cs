using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitStat", menuName = "Scriptable Object Asset/UnitStat")]
public class UnitScriptableObject : ScriptableObject
{
    [field: SerializeField] public GameObject UpperUnit;
    [field: SerializeField] public int Damage;

    [field: SerializeField] public GameObject EnableVfx;
    [field: SerializeField] public AudioClip EnableSfx;

    [field: SerializeField] public GameObject DownGradeVfx;
    [field: SerializeField] public GameObject DownGradeSfx;

    [field: SerializeField] public GameObject DeadVfx;
    [field: SerializeField] public GameObject DeadSfx; 
}
