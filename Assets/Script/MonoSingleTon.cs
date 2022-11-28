using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleTon<T> : MonoBehaviour where T : class, new()
{
    static private T _inst = null;

    static public T Inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = FindObjectOfType(typeof(T)) as T;

                if (_inst == null)
                {
                    GameObject newInst = new GameObject(typeof(T).ToString(), typeof(T));
                    _inst = newInst.AddComponent(typeof(T)) as T;
                }
            }
            return _inst;
        }
    }
}
