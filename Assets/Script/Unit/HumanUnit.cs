using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanUnit : BasicUnit
{   
    public enum Type
    {
        sheildman,
        knight,
        hero,
        max
    }

    public Type type;

    protected override void Awake()
    {
        base.Awake();
    }
}
