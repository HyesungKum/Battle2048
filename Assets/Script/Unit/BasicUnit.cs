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
                return this.UnitData.Damage;
            }
            else
            {
                Debug.Log("basic unit get Damage error ## unit data null");
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
        if (UnitData.EnableVfx != null)
        {
            ObjectPool.Inst.ObjectPop(UnitData.EnableVfx, this.transform.position + (Vector3.up * 0.5f), Quaternion.identity, this.transform);
        }

        if (UnitData.EnableSfx != null)
        {
            AudioSource.clip = UnitData.EnableSfx;
            AudioSource.Play();
        }
    }
    protected void DownGradeProd()
    {
        if (UnitData.DownGradeVfx != null)
        {
            ObjectPool.Inst.ObjectPop(UnitData.DownGradeVfx, this.transform.position + (Vector3.up * 0.5f), Quaternion.identity, null);
        }
        if (UnitData.DownGradeSfx != null)
        {
            ObjectPool.Inst.ObjectPop(UnitData.DownGradeSfx, this.transform.position + (Vector3.up * 0.5f), Quaternion.identity, null);
        }
    }
    protected void DeadProd()
    {
        if (UnitData.DeadVfx != null)
        {
            ObjectPool.Inst.ObjectPop(UnitData.DeadVfx, this.transform.position + (Vector3.up * 0.5f), Quaternion.identity);
        }

        if (UnitData.DeadSfx != null)
        {
            ObjectPool.Inst.ObjectPop(UnitData.DeadSfx, this.transform.position + (Vector3.up * 0.5f), Quaternion.identity);
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
    //============================uncommon function===========================
    public abstract BasicUnit CompareReturn();
}
