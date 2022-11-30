using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoSingleTon<ObjectPool>
{
    Dictionary<string, Queue<GameObject>> MainPool = null;
    Queue<GameObject> SubPool = null;
    GameObject instObj = null;

    private void Awake()
    {
        MainPool = new();
    }

    public GameObject ObjectPop(GameObject _object, Vector3 pos, Quaternion quat)
    {
        string key = _object.name.Split('(')[0];

        if (MainPool.TryGetValue(key, out SubPool) && SubPool.Count < 1)
        {
            this.instObj = SubPool.Dequeue();

            this.instObj.SetActive(true);

            return this.instObj;
        }
        else
        {
            this.instObj = Instantiate(_object, pos, quat, this.transform);

            this.instObj.SetActive(true);
            this.instObj.transform.SetPositionAndRotation(pos, quat);

            return this.instObj;
        }
    }
    public void ObjectPush(GameObject _object)
    {
        string key = _object.name.Replace("(Clone)","");

        if (MainPool.TryGetValue(key, out SubPool))
        {
            this.SubPool.Enqueue(_object);

            _object.SetActive(false);
        }
        else
        {
            _object.transform.position = Vector3.zero;

            this.SubPool = new Queue<GameObject>();
            this.SubPool.Enqueue(_object);

            this.MainPool.Add(key, SubPool);

            _object.SetActive(false);
        }
    }
}
