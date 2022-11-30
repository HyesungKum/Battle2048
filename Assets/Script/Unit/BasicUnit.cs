using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BasicUnit : MonoBehaviour
{
    [SerializeField] UnitScriptableObject Data = null;
    AudioSource AudioSource = null;

    private void Awake()
    {
        AudioSource = GetComponent<AudioSource>();
    }
    private void OnEnable()
    {
        if(Data.enableVfx != null)
            ObjectPool.Inst.ObjectPop(Data.enableVfx, this.transform.position + (Vector3.up * 0.5f), Quaternion.identity);

        if (Data.disableSfx != null)
        {
            AudioSource.clip = Data.enableSfx;
            AudioSource.Play();
        }
    }
}
