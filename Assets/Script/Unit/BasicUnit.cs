using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BasicUnit : MonoBehaviour
{
    [SerializeField] protected UnitScriptableObject UnitData = null;
    protected AudioSource AudioSource = null;

    public bool combinable;
    public bool LastUnit;

    public enum Spcies
    {
        Human,
        Monster,
        Max
    }

    public Spcies spcies;

    public int index = 0;

    virtual protected void Awake()
    {
        AudioSource = GetComponent<AudioSource>();
    }
    private void OnEnable()
    {
        if (UnitData != null)
        {
            if (UnitData.enableVfx != null)
            {
                ObjectPool.Inst.ObjectPop(UnitData.enableVfx, this.transform.position + (Vector3.up * 0.5f), Quaternion.identity, this.transform);
            }

            if (UnitData.enableSfx != null)
            {
                AudioSource.clip = UnitData.enableSfx;
                AudioSource.Play();
            }
        }
    }

    public int GetDamage()
    {
        if (UnitData != null)
        {
            return this.UnitData.damage;
        }
        else
        {
            Debug.Log("basic unit get damage error ## unit data null");
            return 0;
        }
    }
    public void DeadProd()
    {
        if (UnitData.deadVfx != null)
        {
            ObjectPool.Inst.ObjectPop(UnitData.deadVfx, this.transform.position + (Vector3.up * 0.5f), Quaternion.identity);
        }
        
        if (UnitData.deadSfx != null)
        {
            ObjectPool.Inst.ObjectPop(UnitData.deadSfx, this.transform.position + (Vector3.up * 0.5f), Quaternion.identity);
        }
    }
}
