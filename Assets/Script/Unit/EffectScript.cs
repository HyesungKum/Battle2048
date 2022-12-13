using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectScript : MonoBehaviour
{
    [field:SerializeField] float Duration { get; set; }

    private void OnEnable()
    {
        Invoke(nameof(Destroy), Duration);
    }

    private void Destroy()
    {
        ObjectPool.Inst.ObjectPush(this.gameObject);
    }
}
