using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoSingleTon<ObjectPool>
{
    Dictionary<string, Queue<GameObject>> DictionaryPool = new Dictionary<string, Queue<GameObject>>();
    Queue<GameObject> dummyQueue = null;
    GameObject instObj = null;

    public GameObject ObjectPop(GameObject _object, Vector3 pos, Quaternion quat)
    {
        string key = _object.name.Split('.')[0];

        if (DictionaryPool.TryGetValue(key, out dummyQueue))
        {
            instObj = dummyQueue.Dequeue();

            instObj.SetActive(true);

            instObj.transform.position = pos;
            instObj.transform.rotation = quat;

            return instObj;
        }
        else
        {
            instObj = Instantiate(_object, pos, quat);
            instObj.SetActive(true);
            return instObj;
        }
    }
    public void ObjectPush(GameObject _object)
    {
        string key = _object.name.Split('.')[0];

        if (DictionaryPool.TryGetValue(key, out dummyQueue))
        {
            dummyQueue.Enqueue(_object);

            _object.SetActive(false);
        }
        else
        {
            dummyQueue = new Queue<GameObject>();
            dummyQueue.Enqueue(_object);

            _object.SetActive(false);

            DictionaryPool.Add(key, dummyQueue);
        }
    }
}
