using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public abstract class BasicUnit : MonoBehaviour
{
    [SerializeField] protected UnitScriptableObject UnitData = null;
    public enum Spcies
    {
        Human,
        Monster,
        Max
    }
    protected AudioSource AudioSource = null;

    //variables
    public Spcies spcies;
    public int index = 0;
    public bool combinable;
    public bool LastUnit;

    //property
    public int GetDanger
    {
        get
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
    }

    virtual protected void Awake()
    {
        AudioSource = GetComponent<AudioSource>();
    }
    private void OnEnable()
    {
        EnableProd();
    }

    //============================common function=============================
    protected void EnableProd()
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
    protected void DownGradeProd()
    {
        if (UnitData.downGradeVfx != null)
        {
            ObjectPool.Inst.ObjectPop(UnitData.downGradeVfx, this.transform.position + (Vector3.up * 0.5f), Quaternion.identity, null);
        }
        if (UnitData.downGradeSfx != null)
        {
            ObjectPool.Inst.ObjectPop(UnitData.downGradeSfx, this.transform.position + (Vector3.up * 0.5f), Quaternion.identity, null);
        }
    }
    protected void DeadProd()
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
    /// <summary>
    /// compare to both unit was combinable
    /// </summary>
    /// <param name="unit"> targe unit to compare</param>
    /// <returns> if both unit can be combainable return true</returns>
    public bool CompareCom(BasicUnit unit)
    {
        if (spcies == unit.spcies)
        {
            if (GetDanger == unit.GetDanger)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    public BasicUnit Trans(BasicUnit targetUnit)
    {
        return targetUnit;
    }
    //============================uncommon function===========================
}
