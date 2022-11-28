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

    bool Processing { get; set; }
    bool Moved { get; set; }
    bool GameEnd { get; set; }
    bool GameEndToken { get; set; }

    private void Awake()
    {
        GameEndToken = true;

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

        Make2DNodeBoard(4);
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
                //오른쪽 관련 함수들 전부 수정중 ->
                FindRightMatch();
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                FindLeftMatch();
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                FindUpMatch();
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                FindDownMatch();
            }
        }

        Debug.Log("process1" + Pro[0]);
        Debug.Log("process2" + Pro[1]);
        Debug.Log("process3" + Pro[2]);
        Debug.Log("process4" + Pro[3]);
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

    public void FindRightMatch()
    {
        StartCoroutine(SearchAndGenerateR());
    }
    public void FindLeftMatch()
    {
        StartCoroutine(SearchAndGenerateL());
    }
    public void FindUpMatch()
    {
        //왼쪽 끝부터 시작
        for (int i = 1; i < 5; i++)
        {
            StartCoroutine(SearchAndGenerateU());
        }
    }
    public void FindDownMatch()
    {
        //아래 끝부터 시작
        for (int i = 1; i < 5; i++)
        {
            StartCoroutine(SearchAndGenerateD());
        }
    }

    public bool[] Pro = new bool[4];

    #region sync search routine & Generate routine
    IEnumerator SearchAndGenerateR()
    {
        //오른쪽 끝부터 시작
        for (int i = 1; i < 5; i++)
        {
            StartCoroutine(SearchR(node2Ds[i, 4], i-1));
        }

        //과정이 끝날때 까지 대기
        yield return new WaitUntil(() => !Pro[0] && !Pro[1] && !Pro[2] && !Pro[3]);

        Debug.Log("Hi");
        //블록 생성
        if (Moved && !Processing) Generate();

        //EndGameControll
        if (emptyNodes.Count == 0 && GameEndToken)
        {
            StartCoroutine(EndGameCheck());
        }
    }
    IEnumerator SearchAndGenerateL()
    {
        //왼쪽 끝부터 시작
        for (int i = 1; i < 5; i++)
        {
            yield return SearchL(node2Ds[i, 1]);
        }

        //블록 생성
        if (Moved && !Processing) Generate();

        //EndGameControll
        if (emptyNodes.Count == 0 && GameEndToken)
        {
            StartCoroutine(EndGameCheck());
        }
    }
    IEnumerator SearchAndGenerateU()
    {
        //위쪽 끝부터 시작
        for (int i = 1; i < 5; i++)
        {
            yield return SearchU(node2Ds[1, i]);
        }

        //블록 생성
        if (Moved && !Processing) Generate();

        //EndGameControll
        if (emptyNodes.Count == 0 && GameEndToken)
        {
            StartCoroutine(EndGameCheck());
        }
    }
    IEnumerator SearchAndGenerateD()
    {
        //위쪽 끝부터 시작
        for (int i = 1; i < 5; i++)
        {
            yield return SearchD(node2Ds[4, i]);
        }

        //블록 생성
        if (Moved && !Processing) Generate();

        //EndGameControll
        if (emptyNodes.Count == 0 && GameEndToken)
        {
            StartCoroutine(EndGameCheck());
        }
    }
    #endregion

    #region search routine
    public IEnumerator SearchR(Node2D targetNode, int processIndex)
    {
        Pro[processIndex] = true;

        while (true)
        {
            if (targetNode.Wall)
            {
                for (int i = 0; i < 4; i++)
                {
                    targetNode = targetNode.Right;
                    targetNode.Combined = false;
                }
                Pro[processIndex] = false;
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
                    StartCoroutine(ObjectMove(targetNode.Unit.transform, Vector3.right));

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
                        GameEndToken = true;

                        targetNode.Right.Combined = true;
                        targetNode.Right.Value *= 2;

                        GameManager.Inst.Score += targetNode.Right.Value;

                        //
                        Destroy(targetNode.Unit);
                        Destroy(targetNode.Right.Unit);

                        Vector3 newUnitPos = new Vector3(targetNode.Right.XPos, 0.5f, -targetNode.Right.YPos);
                        GameObject newUnit = Instantiate(unit[targetNode.Right.GetUnitIndex], newUnitPos, Quaternion.identity);

                        targetNode.Right.Unit = newUnit;
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
    public IEnumerator SearchL(Node2D targetNode)
    {
        Processing = true;

        while (true)
        {
            if (targetNode.Wall)
            {
                for (int i = 0; i < 4; i++)
                {
                    targetNode = targetNode.Left;
                    targetNode.Combined = false;
                }
                Processing = false;
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
                        GameEndToken = true;

                        targetNode.Left.Combined = true;
                        targetNode.Left.Value *= 2;

                        GameManager.Inst.Score += targetNode.Left.Value;

                        //
                        Destroy(targetNode.Unit);
                        Destroy(targetNode.Left.Unit);

                        Vector3 newUnitPos = new Vector3(targetNode.Left.XPos, 0.5f, -targetNode.Left.YPos);
                        GameObject newUnit = Instantiate(unit[targetNode.Left.GetUnitIndex], newUnitPos, Quaternion.identity);

                        targetNode.Left.Unit = newUnit;
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
    public IEnumerator SearchU(Node2D targetNode)
    {
        Processing = true;

        while (true)
        {
            if (targetNode.Wall)
            {
                for (int i = 0; i < 4; i++)
                {
                    targetNode = targetNode.Up;
                    targetNode.Combined = false;
                }
                Processing = false;
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
                        GameEndToken = true;

                        targetNode.Up.Combined = true;
                        targetNode.Up.Value *= 2;

                        GameManager.Inst.Score += targetNode.Up.Value;

                        //
                        Destroy(targetNode.Unit);
                        Destroy(targetNode.Up.Unit);

                        Vector3 newUnitPos = new Vector3(targetNode.Up.XPos, 0.5f, -targetNode.Up.YPos);
                        GameObject newUnit = Instantiate(unit[targetNode.Up.GetUnitIndex], newUnitPos, Quaternion.identity);

                        targetNode.Up.Unit = newUnit;
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
    public IEnumerator SearchD(Node2D targetNode)
    {
        Processing = true;

        while (true)
        {
            if (targetNode.Wall)
            {
                for (int i = 0; i < 4; i++)
                {
                    targetNode = targetNode.Down;
                    targetNode.Combined = false;
                }
                Processing = false;
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
                        GameEndToken = true;

                        targetNode.Down.Combined = true;
                        targetNode.Down.Value *= 2;

                        GameManager.Inst.Score += targetNode.Down.Value;

                        //
                        Destroy(targetNode.Unit);
                        Destroy(targetNode.Down.Unit);

                        Vector3 newUnitPos = new Vector3(targetNode.Down.XPos, 0.5f, -targetNode.Down.YPos);
                        GameObject newUnit = Instantiate(unit[targetNode.Down.GetUnitIndex], newUnitPos, Quaternion.identity);

                        targetNode.Down.Unit = newUnit;
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

    public IEnumerator ObjectMove(Transform targetTrans, Vector3 dir)
    {
        Vector3 oldPos = targetTrans.position;

        while (true)
        {
            targetTrans.Translate(dir * Time.deltaTime * 0.1f);

            if (targetTrans.position.x >= oldPos.x + dir.x)
            {
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
        GameObject newUnit = Instantiate(unit[0], newUnitPos, Quaternion.identity);

        emptyNodes[pickNum].Unit = newUnit;
        emptyNodes.RemoveAt(pickNum);
    }
    #endregion

    #region gameEndroutine
    IEnumerator EndGameCheck()
    {
        GameEndToken = false;

        for (int i = 1; i < boardSize + 1; i++)
        {
            yield return SearchVertical(node2Ds[i, 1]);
            yield return SearchHorizontal(node2Ds[1, i]);

            if (emptyNodes.Count != 0)
            {
                GameEnd = false;
            }
        }

        if (GameEnd)
        {
            GameManager.Inst.GameEnd = true;
        }
    }
    IEnumerator SearchVertical(Node2D node)
    {
        while(true)
        {
            if (node.Value != node.Right.Value)
            {
                GameEnd = true;
                node = node.Right;
            }
            else
            {
                GameEnd = false;
            }

            yield return null;
            if (node.Wall) yield break;
        }
    }
    IEnumerator SearchHorizontal(Node2D node)
    {
        while (true)
        {
            if (node.Value != node.Down.Value)
            {
                GameEnd = true;
                node = node.Down;
            }
            else
            {
                GameEnd = false;
            }

            yield return null;
            if (node.Wall) yield break;
        }
    }
    #endregion
}
