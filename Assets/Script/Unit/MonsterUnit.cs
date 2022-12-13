using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterUnit : BasicUnit
{
    #if UNITY_EDITOR
    [field:SerializeField] int Damage { get; set; }
    #endif
    protected override void Awake()
    {
        #if UNITY_EDITOR
        Damage = UnitData.damage;
        #endif

        base.Awake();

        combinable = true;
    }
    //몬스터 스크립트

    public void DownGradeProd()
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
}
