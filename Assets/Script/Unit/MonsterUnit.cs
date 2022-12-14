using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterUnit : BasicUnit
{
    public enum MonsterType
    {
        Normal,
        HeroSlayer,
        Max
    }

    public MonsterType Type;

    protected override void Awake()
    {
        base.Awake();
    }

    //public override BasicUnit Compare(BasicUnit compUnit)
    //{
    //    throw new System.NotImplementedException();
    //}
}
