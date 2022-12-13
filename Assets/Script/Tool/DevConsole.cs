using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevConsole : MonoBehaviour
{
    [SerializeField] GameObject testEff = null;
    [SerializeField] GameObject testObj = null;
    public List<GameObject> testObjList = new List<GameObject>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            GameObject instEff = ObjectPool.Inst.ObjectPop(testEff, this.transform.position, Quaternion.identity, null);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            testObjList.Add(ObjectPool.Inst.ObjectPop(testObj, this.transform.position, Quaternion.identity, null));
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            ObjectPool.Inst.ObjectPush(testObjList[^1]);
            testObjList.Remove(testObjList[^1]);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(this.transform.position, Vector3.one);
    }
}
