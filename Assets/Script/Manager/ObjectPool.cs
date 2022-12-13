using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoSingleTon<ObjectPool>
{
    Dictionary<string, List<GameObject>> MainPool = null;

    private void Awake()
    {
        MainPool = new();
    }

    /// <summary>
    /// call object about object pool
    /// </summary>
    /// <param name="_object"></param>
    /// <param name="pos"></param>
    /// <param name="quat"></param>
    /// <returns></returns>
    public GameObject ObjectPop(GameObject _object, Vector3 pos, Quaternion quat)
    {
        string key = _object.name.Split('(')[0];

        if (MainPool.TryGetValue(key, out List<GameObject> SubPool))
        {
            if (SubPool.Count != 0)
            {
                GameObject instObj = SubPool[0];
                SubPool.RemoveAt(0);

                instObj.SetActive(true);

                instObj.transform.SetPositionAndRotation(pos, quat);

                return instObj;
            }
            else
            {
                GameObject instObj = Instantiate(_object, pos, quat);

                instObj.SetActive(true);

                return instObj;
            }
        }
        else
        {
            GameObject instObj = instObj = Instantiate(_object, pos, quat);

            instObj.SetActive(true);

            return instObj;
        }
    }
    /// <summary>
    /// call object about object pool
    /// </summary>
    /// <param name="_object"></param>
    /// <param name="pos"></param>
    /// <param name="quat"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public GameObject ObjectPop(GameObject _object, Vector3 pos, Quaternion quat, Transform parent)
    {
        string key = _object.name.Split('(')[0];

        if (MainPool.TryGetValue(key, out List<GameObject> SubPool))
        {
            if (SubPool.Count != 0)
            {
                GameObject instObj = SubPool[0];
                SubPool.RemoveAt(0);

                instObj.SetActive(true);
                
                if(parent != null) instObj.transform.SetParent(parent);

                instObj.transform.SetPositionAndRotation(pos, quat);

                return instObj;
            }
            else
            {
                GameObject instObj = Instantiate(_object, pos, quat, parent);

                instObj.SetActive(true);

                return instObj;
            }
        }
        else
        {
            GameObject instObj = instObj = Instantiate(_object, pos, quat, parent);

            instObj.SetActive(true);

            return instObj;
        }
    }

    /// <summary>
    /// object put in to object pool
    /// </summary>
    /// <param name="_object"></param>
    public void ObjectPush(GameObject _object)
    {
        if (_object == null) return;

        string key = _object.name.Replace("(Clone)","");

        if (MainPool.TryGetValue(key, out List<GameObject> SubPool))
        {
            SubPool.Add(_object);
            _object.SetActive(false);
        }
        else
        {
            SubPool = new()
            {
                _object
            };

            this.MainPool.Add(key, SubPool);

            _object.SetActive(false);
        }
    }
}
