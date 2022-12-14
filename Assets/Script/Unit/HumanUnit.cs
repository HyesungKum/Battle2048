using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanUnit : BasicUnit
{   
    public enum HumanType
    {
        sheildman,
        knight,
        hero,
        max
    }

    public HumanType Type;

    protected override void Awake()
    {
        base.Awake();
    }

    //public override bool Compare(BasicUnit compUnit)
    //{
    //    if (spcies == compUnit.spcies)
    //    {
    //        if (GetDanger == compUnit.GetDanger)
    //        {

    //        }
    //        else
    //        {
                
    //        }
    //    }
    //    return true;
    //}

    private BasicUnit UpGrade()
    {
        return (BasicUnit)GetComponent<BasicUnit>();
    }
}
