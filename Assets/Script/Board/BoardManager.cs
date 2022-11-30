using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviour
{
    [Header("BoardSize")]
    [SerializeField] int boardSize = 4;

    [Header("unit prefab")]
    [SerializeField] List<GameObject> unit = null;

    Node2D[,] node2Ds = null;
    List<Node2D> emptyNodes = new List<Node2D>();

    WaitForSeconds waitTime = new WaitForSeconds(0.7f);

    bool Processing { get; set; }
    bool Moved { get; set; }
    bool GameEnd { get; set; }

    public bool[] lineProcessing = new bool[4];

    [Header("Effect")]
    [SerializeField] GameObject eff = null;

    private void Awake()
    {

        node2Ds = new Node2D[boardSize + 2, boardSize + 2];

        for (int y = 0; y < boardSize + 2; y++)
        {
            for (int x = 0; x < boardSize + 2; x++)
            {
                node2Ds[y, x] = new Node2D();
                if (0 < x && x < boardSize + 1 && 0 < y && y < boardSize + 1)
                {
                    node2Ds[y, x].XPos = x;
                    node2Ds[y, x].YPos = y;
                    emptyNodes.Add(node2Ds[y, x]);
                }
            }
        }

        Make2DNodeBoard(boardSize);
    }
    void Start()
    {
        //basic 2 block generate
        Generate();
        Generate();
    }
    
    void Update()
    {
        //controll
        if (!Processing)
        {
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                SearchAndGenerateR();
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SearchAndGenerateL();
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                SearchAndGenerateU();
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                SearchAndGenerateD();
            }
        }
    }

    void Make2DNodeBoard(int size)
    {
        //create 4 by 4 node array
        for (int y = 0; y < size + 2; y++)
        {
            for (int x = 0; x < size + 2; x++)
            {
                //create wall
                if (x == 0 || x == size + 1 || y == 0 || y == size + 1)
                {
                    node2Ds[x, y].Wall = true;

                    //exception
                    if (x == 0) node2Ds[y, x].Right = node2Ds[y, x + 1];
                    else if (x == size + 1) node2Ds[y, x].Left = node2Ds[y, x - 1];
                    else if (y == 0) node2Ds[y, x].Down = node2Ds[y + 1, x];
                    else if (y == size + 1) node2Ds[y, x].Up = node2Ds[y - 1, x];
                }
                else
                {
                    //1~4 node connection
                    node2Ds[y, x].Left = node2Ds[y, x - 1];
                    node2Ds[y, x].Right = node2Ds[y, x + 1];
                    node2Ds[y, x].Up = node2Ds[y - 1, x];
                    node2Ds[y, x].Down = node2Ds[y + 1, x];
                }
            }
        }
    }

    #region sync search routine & Generate routine
    void SearchAndGenerateR()
    {
        for (int i = 1; i < 5; i++)
        {
            StartCoroutine(SearchR(node2Ds[i, 4], i - 1));
        }
    }
    void SearchAndGenerateL()
    {
        for (int i = 1; i < 5; i++)
        {
            StartCoroutine(SearchL(node2Ds[i, 1], i - 1));
        }
    }
    void SearchAndGenerateU()
    {
        for (int i = 1; i < 5; i++)
        {
            StartCoroutine(SearchU(node2Ds[1, i], i - 1));
        }
    }
    void SearchAndGenerateD()
    {
        for (int i = 1; i < 5; i++)
        {
            StartCoroutine(SearchD(node2Ds[4, i], i - 1));
        }
    }
    #endregion

    #region search routine
    public IEnumerator SearchR(Node2D targetNode, int index)
    {
        lineProcessing[index] = true;

        while (true)
        {
            if (targetNode.Wall)
            {
                for (int i = 0; i < 4; i++)
                {
                    targetNode = targetNode.Right;
                    targetNode.Combined = false;
                }
                lineProcessing[index] = false;

                //������ ��ƾ�� ���� ��
                if (index == 3 && Moved)
                {
                    Generate();
                }

                yield break;
            }

            if (!targetNode.Empty)
            {
                if (targetNode.Right.Wall || targetNode.Right.Combined)//stop
                {
                    targetNode = targetNode.Left;
                    continue;
                }
                else if (targetNode.Right.Empty)//move
                {
                    targetNode.Right.Value = targetNode.Value;
                    targetNode.Right.Empty = false;

                    targetNode.Value = 0;
                    targetNode.Empty = true;
                    emptyNodes.Add(targetNode);
                    //
                    targetNode.Unit.transform.position += Vector3.right;

                    targetNode.Right.Unit = targetNode.Unit;
                    targetNode.Unit = null;
                    //
                    targetNode = targetNode.Right;
                    emptyNodes.Remove(targetNode);

                    Moved = true;
                    continue;
                }
                else
                {
                    if (targetNode.Value == targetNode.Right.Value)//combine
                    {
                        targetNode.Right.Combined = true;
                        targetNode.Right.Value *= 2;

                        GameManager.Inst.Score += targetNode.Left.Value;

                        //object
                        ObjectPool.Inst.ObjectPush(targetNode.Unit);
                        ObjectPool.Inst.ObjectPush(targetNode.Right.Unit);

                        Vector3 newUnitPos = new Vector3(targetNode.Right.XPos, 0.5f, -targetNode.Right.YPos);
                        targetNode.Right.Unit = ObjectPool.Inst.ObjectPop(unit[targetNode.Right.GetUnitIndex], newUnitPos, Quaternion.identity);
                        //

                        targetNode.Empty = true;
                        targetNode.Value = 0;

                        emptyNodes.Add(targetNode);

                        Moved = true;

                        targetNode = targetNode.Left;

                        continue;
                    }
                }
            }

            targetNode = targetNode.Left;
            yield return null;
        }
    }
    public IEnumerator SearchL(Node2D targetNode, int index)
    {
        lineProcessing[index] = true;

        while (true)
        {
            if (targetNode.Wall)
            {
                for (int i = 0; i < 4; i++)
                {
                    targetNode = targetNode.Left;
                    targetNode.Combined = false;
                }
                lineProcessing[index] = false;

                //������ ��ƾ�� ���� ��
                if (index == 3 && Moved)
                {
                    Generate();
                }

                yield break;
            }

            if (!targetNode.Empty)
            {
                if (targetNode.Left.Wall || targetNode.Left.Combined)//stop
                {
                    targetNode = targetNode.Right;
                    continue;
                }
                else if (targetNode.Left.Empty)//move
                {
                    targetNode.Left.Value = targetNode.Value;
                    targetNode.Left.Empty = false;

                    targetNode.Value = 0;
                    targetNode.Empty = true;
                    emptyNodes.Add(targetNode);
                    //
                    targetNode.Unit.transform.position += Vector3.left;

                    targetNode.Left.Unit = targetNode.Unit;
                    targetNode.Unit = null;
                    //
                    targetNode = targetNode.Left;
                    emptyNodes.Remove(targetNode);

                    Moved = true;
                    continue;
                }
                else
                {
                    if (targetNode.Value == targetNode.Left.Value)//combine
                    {
                        targetNode.Left.Combined = true;
                        targetNode.Left.Value *= 2;

                        GameManager.Inst.Score += targetNode.Right.Value;

                        //object
                        ObjectPool.Inst.ObjectPush(targetNode.Unit);
                        ObjectPool.Inst.ObjectPush(targetNode.Left.Unit);

                        Vector3 newUnitPos = new Vector3(targetNode.Left.XPos, 0.5f, -targetNode.Left.YPos);
                        targetNode.Left.Unit = ObjectPool.Inst.ObjectPop(unit[targetNode.Left.GetUnitIndex], newUnitPos, Quaternion.identity);
                        //

                        targetNode.Empty = true;
                        targetNode.Value = 0;

                        emptyNodes.Add(targetNode);

                        Moved = true;

                        targetNode = targetNode.Right;
                        continue;
                    }
                }
            }
            targetNode = targetNode.Right;
            yield return null;
        }
    }
    public IEnumerator SearchU(Node2D targetNode, int index)
    {
        lineProcessing[index] = true;

        while (true)
        {
            if (targetNode.Wall)
            {
                for (int i = 0; i < 4; i++)
                {
                    targetNode = targetNode.Up;
                    targetNode.Combined = false;
                }
                lineProcessing[index] = false;

                //������ ��ƾ�� ���� ��
                if (index == 3 && Moved)
                {
                    Generate();
                }

                yield break;
            }

            if (!targetNode.Empty)
            {
                if (targetNode.Up.Wall || targetNode.Up.Combined)//stop
                {
                    targetNode = targetNode.Down;
                    continue;
                }
                else if (targetNode.Up.Empty)//move
                {
                    targetNode.Up.Value = targetNode.Value;
                    targetNode.Up.Empty = false;

                    targetNode.Value = 0;
                    targetNode.Empty = true;

                    emptyNodes.Add(targetNode);
                    //
                    targetNode.Unit.transform.position += Vector3.forward;

                    targetNode.Up.Unit = targetNode.Unit;
                    targetNode.Unit = null;
                    //
                    targetNode = targetNode.Up;
                    emptyNodes.Remove(targetNode);

                    Moved = true;

                    continue;
                }
                else
                {
                    if (targetNode.Value == targetNode.Up.Value)//combine
                    {
                        targetNode.Up.Combined = true;
                        targetNode.Up.Value *= 2;

                        GameManager.Inst.Score += targetNode.Up.Value;

                        //object
                        ObjectPool.Inst.ObjectPush(targetNode.Unit);
                        ObjectPool.Inst.ObjectPush(targetNode.Up.Unit);

                        Vector3 newUnitPos = new Vector3(targetNode.Up.XPos, 0.5f, -targetNode.Up.YPos);
                        targetNode.Up.Unit = ObjectPool.Inst.ObjectPop(unit[targetNode.Up.GetUnitIndex], newUnitPos, Quaternion.identity);
                        //

                        targetNode.Empty = true;
                        targetNode.Value = 0;

                        emptyNodes.Add(targetNode);

                        Moved = true;

                        targetNode = targetNode.Down;
                        continue;
                    }
                }
            }

            targetNode = targetNode.Down;
            yield return null;
        }
    }
    public IEnumerator SearchD(Node2D targetNode, int index)
    {
        lineProcessing[index] = true;

        while (true)
        {
            if (targetNode.Wall)
            {
                for (int i = 0; i < 4; i++)
                {
                    targetNode = targetNode.Down;
                    targetNode.Combined = false;
                }
                lineProcessing[index] = false;

                //������ ��ƾ�� ���� ��
                if (index == 3 && Moved)
                {
                    Generate();
                }
                yield break;
            }

            if (!targetNode.Empty)
            {
                if (targetNode.Down.Wall || targetNode.Down.Combined)//stop
                {
                    targetNode = targetNode.Up;
                    continue;
                }
                else if (targetNode.Down.Empty)//move
                {
                    targetNode.Down.Value = targetNode.Value;
                    targetNode.Down.Empty = false;

                    targetNode.Value = 0;
                    targetNode.Empty = true;

                    emptyNodes.Add(targetNode);
                    //
                    targetNode.Unit.transform.position += Vector3.back;

                    targetNode.Down.Unit = targetNode.Unit;
                    targetNode.Unit = null;
                    //
                    targetNode = targetNode.Down;
                    emptyNodes.Remove(targetNode);

                    Moved = true;
                    continue;
                }
                else
                {
                    if (targetNode.Value == targetNode.Down.Value)//combine
                    {
                        targetNode.Down.Combined = true;
                        targetNode.Down.Value *= 2;

                        GameManager.Inst.Score += targetNode.Down.Value;

                        //object
                        ObjectPool.Inst.ObjectPush(targetNode.Unit);
                        ObjectPool.Inst.ObjectPush(targetNode.Down.Unit);

                        Vector3 newUnitPos = new Vector3(targetNode.Down.XPos, 0.5f, -targetNode.Down.YPos);
                        targetNode.Down.Unit = ObjectPool.Inst.ObjectPop(unit[targetNode.Down.GetUnitIndex], newUnitPos, Quaternion.identity);
                        //

                        targetNode.Empty = true;
                        targetNode.Value = 0;

                        emptyNodes.Add(targetNode);

                        Moved = true;

                        targetNode = targetNode.Up;
                        continue;
                    }
                }
            }

            targetNode = targetNode.Up;
            yield return null;
        }
    }
    #endregion

    #region Object Move
    public IEnumerator ObjectMove(Transform targetTrans, Vector3 dir, int count)
    {
        if(count == 0)
            yield break;

        Vector3 fixedPos = targetTrans.position;
        Vector3 desPos = targetTrans.position + (dir * count);

        while (true)
        {
            targetTrans.position = Vector3.Lerp(targetTrans.position, desPos, 0.1f);

            fixedPos.x = Mathf.Round(targetTrans.position.x*10f)/10f;
            fixedPos.z = Mathf.Round(targetTrans.position.z*10f)/10f;

            if (desPos.Equals(fixedPos))
            {
                targetTrans.position = desPos;
                yield break;
            }
            yield return null;
        }
    }

    #endregion

    #region generate routine
    void Generate()
    {
        //logic
        if (emptyNodes.Count == 0) return;
        int pickNum = Random.Range(0, emptyNodes.Count);

        emptyNodes[pickNum].Value = 2;
        emptyNodes[pickNum].Empty = false;

        Moved = false;

        //object create
        Vector3 newUnitPos = new Vector3(emptyNodes[pickNum].XPos, 0.5f, -emptyNodes[pickNum].YPos);
        
        emptyNodes[pickNum].Unit = ObjectPool.Inst.ObjectPop(unit[0], newUnitPos, Quaternion.identity);
        emptyNodes.RemoveAt(pickNum);

        //EndGameControll
        if (emptyNodes.Count == 0)
        {
            StartCoroutine(EndGameCheck());
        }
    }
    #endregion

    #region gameEndroutine
    IEnumerator EndGameCheck()
    {
        for (int i = 1; i < boardSize + 1; i++)
        {
            StartCoroutine(SearchVertical(node2Ds[i, 1], i - 1));
            StartCoroutine(SearchHorizontal(node2Ds[1, i], i - 1));
        }
        
        yield return new WaitUntil(() => processed1 && processed2);

        if (GameEnd)
        {
            GameManager.Inst.GameEnd = true;
        }
        else
        {
            processed1 = false;
            processed2 = false;
        }
    }
    bool processed1 = false;
    bool processed2 = false;
    IEnumerator SearchVertical(Node2D node, int index)
    {
        bool sameDetect = false;
        while(true)
        {
            if (node.Value == node.Right.Value)
            {
                sameDetect = true;
            }

            node = node.Right;

            if (node.Wall)
            {
                if (sameDetect) GameEnd = false; 
                else GameEnd = true;

                if (index == 3)
                {
                    processed1 = true;
                }
                yield break;
            }

            yield return null;
        }
    }
    IEnumerator SearchHorizontal(Node2D node, int index)
    {
        bool sameDetect = false;
        while (true)
        {
            if (node.Value == node.Down.Value)
            {
                sameDetect = true;
            }

            node = node.Down;

            if (node.Wall)
            {
                if (sameDetect) GameEnd = false; 
                else GameEnd = true;

                if (index == 3)
                {
                    processed2 = true;
                }

                yield break;
            }

            yield return null;
        }
    }
    #endregion
}