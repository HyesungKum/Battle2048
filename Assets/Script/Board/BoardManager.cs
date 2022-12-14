using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
public class BoardManager : MonoBehaviour
{
    //====================Settings==============================
    [Header("BoardSize=======================================")]
    [SerializeField] int boardSize = 4;

    [Header("BoardEffect=====================================")]
    AudioSource boardAudio = null;
    [Header("move")]
    [SerializeField] GameObject ObjBoardMoveVfx     = null;
    ParticleSystem VfxBoardMove                     = null;
    [SerializeField] AudioClip SfxMove              = null;

    [Header("move fail")]
    [SerializeField] GameObject ObjBoardMoveFailVfx = null;
    ParticleSystem VfxBoardFail                     = null;
    [SerializeField] AudioClip SfxMoveFail          = null;

    [Header("undo")]
    [SerializeField] GameObject HourGlass           = null;
    [SerializeField] GameObject[] ChanceLight       = null;
    Animator HourGlassAnimator = null;
    [SerializeField] GameObject ObjBoardUndoVfx     = null;
    ParticleSystem VfxBoardUndo = null;
    [SerializeField] AudioClip SfxUndo              = null;
    [SerializeField] AudioClip SfxUndoFail          = null;

    [Header("Unit Prefab=====================================")]
    [SerializeField] List<MonsterUnit> units = null;

    [Header("Obstacle Prefab=================================")]
    [SerializeField] List<GameObject> obstaclesUnits = null;

    [Header("Difficulty======================================")]
    [SerializeField] int genBaseMonster = 2;
    [SerializeField] int genEnemy = 2;
    [SerializeField] int ObGenPercent = 10;
    [SerializeField] int heroPercent = 1;
    [SerializeField] int KnightPercent = 5;

    [Header("Obstacle Objective Unit=========================")]
    [SerializeField] MonsterUnit targetUnit = null;

    [Header("debuging each node data=========================")]
    #if UNITY_EDITOR
    [SerializeField] bool showNodeUnit = false;
    [Tooltip("Default Value 0")]
    [SerializeField] int ForcedUnitIndex = 0;
    [SerializeField] bool NoGen = false;
    [SerializeField] bool InfiniteUndo = false;
    #endif

    //===================node Data==============================
    LinkedNode2D nodes2D           = null;
    List<LinkedNode2D> recordNodes = new();
    List<Node> emptyNodes          = new();

    //==============board managing flag=========================
    readonly WaitForSeconds waitTime = new(1f);
    bool Moved { get; set; }
    bool Generated { get; set; }
    bool Processing { get; set; }
    bool UndoProcessing { get; set; }
    bool[] lineProcessing = null;
    int undoCount = 4;
    int lightCount = 0;
    int undoChance { get; set; }

    //====================main Logic============================
    private void Awake()//initializing and board make
    {
        #region debug state check
        #if UNITY_EDITOR
        if (
                showNodeUnit ||
                ForcedUnitIndex != 0 ||
                NoGen
           )
        {
            Debug.LogWarning("BoardManager Warning## Debug mode is not disabled!!!");
        }
#endif
        #endregion

        //board undo vfx & sfx
        lightCount = 0;
        for (int i = 0; i < ChanceLight.Length; i++)
        {
            ChanceLight[i].SetActive(true);
        }
        HourGlassAnimator = HourGlass.GetComponent<Animator>();
        VfxBoardUndo = ObjBoardUndoVfx.GetComponent<ParticleSystem>();

        //board move vfx & sfx
        VfxBoardMove = ObjBoardMoveVfx.GetComponentInChildren<ParticleSystem>();
        VfxBoardFail = ObjBoardMoveFailVfx.GetComponentInChildren<ParticleSystem>();

        //board move sound source
        boardAudio = this.GetComponent<AudioSource>();
        boardAudio.clip = SfxMove;

        //init object pool
        ObjectPool.Clear();

        //processing initialize
        undoChance = 0;
        Generated = false;
        UndoProcessing = false;
        Processing = false;
        lineProcessing = new bool[boardSize];

        //assign node nodeData
        nodes2D = new LinkedNode2D(boardSize);

        //assign each node
        for (int y = 0; y < boardSize + 2; y++)
        {
            for (int x = 0; x < boardSize + 2; x++)
            {
                nodes2D.nodes[y, x] = new Node();
            }
        }

        //apply each node data
        for (int y = 0; y < boardSize + 2; y++)
        {
            for (int x = 0; x < boardSize + 2; x++)
            {
                //input coordinate
                nodes2D.nodes[y, x].XPos = x;
                nodes2D.nodes[y, x].YPos = y;

                //create wall
                if (x == 0 || x == boardSize + 1 || y == 0 || y == boardSize + 1)
                {
                    nodes2D.nodes[x, y].SetWall();

                    //exception
                    if (x == 0) nodes2D.nodes[y, x].Right = nodes2D.nodes[y, x + 1];
                    else if (x == boardSize + 1) nodes2D.nodes[y, x].Left = nodes2D.nodes[y, x - 1];
                    else if (y == 0) nodes2D.nodes[y, x].Down = nodes2D.nodes[y + 1, x];
                    else if (y == boardSize + 1) nodes2D.nodes[y, x].Up = nodes2D.nodes[y - 1, x];
                }
                else
                {
                    //1~4 node connection
                    nodes2D.nodes[y, x].Left = nodes2D.nodes[y, x - 1];
                    nodes2D.nodes[y, x].Right = nodes2D.nodes[y, x + 1];
                    nodes2D.nodes[y, x].Up = nodes2D.nodes[y - 1, x];
                    nodes2D.nodes[y, x].Down = nodes2D.nodes[y + 1, x];

                    //empty node apply
                    emptyNodes.Add(nodes2D.nodes[y, x]);
                }
            }
        }
    }
    void Start()//initial block generate
    {
        for(int i =0; i< genEnemy; i++)
        {
            GenerateObstracle();
        }
        for (int i = 0; i < genBaseMonster; i++)
        {
            GenerateBaseUnit();
        }

        RecordingNodes(nodes2D);
    }
    void Update()//input controll
    {
        if (GameManager.Inst.GameEnd) return;

        if (!Processing && !lineProcessing[0] && !lineProcessing[1] && !lineProcessing[2] && !lineProcessing[3] && !UndoProcessing)
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

        if (Input.GetKeyDown(KeyCode.Q) && !UndoProcessing)
        {
            if (recordNodes.Count > 1)
            {
                #region testmode
                #if UNITY_EDITOR
                if (InfiniteUndo)
                {
                    CallRecordNodes();
                    return; 
                }
                #endif
                #endregion
                GameManager.Inst.TimeStop = true;

                ChanceLight[lightCount].SetActive(false);
                UndoProcessing = true;
                UndoControll();
                boardAudio.clip = SfxUndo;
                boardAudio.Play();
                Invoke(nameof(CallRecordNodes), 2f);
                lightCount++;
            }
            else
            {
                //undo fail production
                //dir next insert movedfail sfx
            }
            #region debuging routine
            #if UNITY_EDITOR
            if (showNodeUnit)
            {
                Debug.Log($"{nodes2D.nodes[1, 1].nodeData.Unit,80}|{nodes2D.nodes[1, 2].nodeData.Unit,80}|{nodes2D.nodes[1, 3].nodeData.Unit,80}|{nodes2D.nodes[1, 4].nodeData.Unit,80}");
                Debug.Log($"{nodes2D.nodes[2, 1].nodeData.Unit,80}|{nodes2D.nodes[2, 2].nodeData.Unit,80}|{nodes2D.nodes[2, 3].nodeData.Unit,80}|{nodes2D.nodes[2, 4].nodeData.Unit,80}");
                Debug.Log($"{nodes2D.nodes[3, 1].nodeData.Unit,80}|{nodes2D.nodes[3, 2].nodeData.Unit,80}|{nodes2D.nodes[3, 3].nodeData.Unit,80}|{nodes2D.nodes[3, 4].nodeData.Unit,80}");
                Debug.Log($"{nodes2D.nodes[4, 1].nodeData.Unit,80}|{nodes2D.nodes[4, 2].nodeData.Unit,80}|{nodes2D.nodes[4, 3].nodeData.Unit,80}|{nodes2D.nodes[4, 4].nodeData.Unit,80}");
                Debug.Log($"empty node count : {emptyNodes.Count}");
                Debug.Log($"Undo List Count : {recordNodes.Count}");
                Debug.Log($"Undo Chance : {undoChance}");
                Debug.Log("==================================================");
            }
            #endif
            #endregion
        }
    }

    #region board vfx sfx production
    void DirArrowControll(Vector3 dir)
    {
        ObjBoardMoveVfx.transform.forward = dir;
        VfxBoardMove.Play();
        boardAudio.clip = SfxMove;
        boardAudio.Play();
    }
    void UndoControll()
    {
        HourGlassAnimator.SetTrigger("Turn");
        VfxBoardUndo.Play();
    }
    #endregion

    #region Line Search routine
    void SearchAndGenerateR()
    {
        for (int i = 1; i < boardSize + 1; i++)
        {
            StartCoroutine(SearchR(nodes2D.nodes[i, boardSize], i - 1));
        }
    }
    void SearchAndGenerateL()
    {
        for (int i = 1; i < boardSize + 1; i++)
        {
            StartCoroutine(SearchL(nodes2D.nodes[i, 1], i - 1));
        }
    }
    void SearchAndGenerateU()
    {
        for (int i = 1; i < boardSize + 1; i++)
        {
            StartCoroutine(SearchU(nodes2D.nodes[1, i], i - 1));
        }
    }
    void SearchAndGenerateD()
    {
        for (int i = 1; i < boardSize + 1; i++)
        {
            StartCoroutine(SearchD(nodes2D.nodes[boardSize, i], i - 1));
        }
    }
    #endregion

    #region search routine
    public IEnumerator SearchR(Node targetNode, int routineCount)
    {
        //processing flag on
        lineProcessing[routineCount] = true;

        //search routine
        while (true)
        {
            if (targetNode.nodeData.Wall)
            {
                for (int i = 0; i < boardSize; i++)
                {
                    targetNode = targetNode.Right;
                    targetNode.nodeData.Combined = false;
                }
                lineProcessing[routineCount] = false;

                if (routineCount == boardSize - 1 && Moved)
                {
                    yield return new WaitUntil(() => !lineProcessing[0] && !lineProcessing[1] && !lineProcessing[2] && !lineProcessing[3]);

                    GenerateBaseUnit();

                    DirArrowControll(Vector3.right);
                    RecordingNodes(nodes2D);
                }
                else if(routineCount == boardSize - 1)
                {
                    VfxBoardFail.Play();
                }

                yield break;
            }

            //targetnode full
            if (targetNode.nodeData.Unit)
            {
                if (targetNode.Right.nodeData.Wall || targetNode.Right.nodeData.Combined)
                {
                    targetNode = targetNode.Left;
                    continue;
                }
                else if (!targetNode.Right.nodeData.Unit)//move
                {
                    targetNode.nodeData.Unit.gameObject.transform.position += Vector3.right;

                    (targetNode.nodeData, targetNode.Right.nodeData) = (targetNode.Right.nodeData, targetNode.nodeData);
                    emptyNodes.Add(targetNode);
                    emptyNodes.Remove(targetNode.Right);

                    Moved = true;
                    targetNode = targetNode.Right;
                    continue;
                }
                else CompareUnit(targetNode, targetNode.Right);
            }
            targetNode = targetNode.Left;
            yield return null;
        }
    }
    public IEnumerator SearchL(Node targetNode, int routineCount)
    {
        lineProcessing[routineCount] = true;

        while (true)
        {
            //search routine ending
            if (targetNode.nodeData.Wall)
            {
                for (int i = 0; i < boardSize; i++)
                {
                    targetNode = targetNode.Left;
                    targetNode.nodeData.Combined = false;
                }
                lineProcessing[routineCount] = false;

                if (routineCount == boardSize - 1 && Moved)
                {
                    yield return new WaitUntil(() => !lineProcessing[0] && !lineProcessing[1] && !lineProcessing[2] && !lineProcessing[3]);
                    GenerateBaseUnit();

                    DirArrowControll(Vector3.left);
                    RecordingNodes(nodes2D);
                }
                else if (routineCount == boardSize - 1)
                {
                    VfxBoardFail.Play();
                }
                yield break;
            }

            if (targetNode.nodeData.Unit)
            {
                if (targetNode.Left.nodeData.Wall || targetNode.Left.nodeData.Combined)//skip testing
                {
                    targetNode = targetNode.Right;
                    continue;
                }
                else if (!targetNode.Left.nodeData.Unit)//move
                {
                    targetNode.nodeData.Unit.gameObject.transform.position += Vector3.left;

                    (targetNode.nodeData, targetNode.Left.nodeData) = (targetNode.Left.nodeData, targetNode.nodeData);
                    emptyNodes.Add(targetNode);
                    emptyNodes.Remove(targetNode.Left);

                    Moved = true;
                    targetNode = targetNode.Left;
                    continue;
                }
                else CompareUnit(targetNode, targetNode.Left);
            }
            targetNode = targetNode.Right;
            yield return null;
        }
    }
    public IEnumerator SearchU(Node targetNode, int routineCount)
    {
        lineProcessing[routineCount] = true;

        //search routine
        while (true)
        {
            if (targetNode.nodeData.Wall)
            {
                for (int i = 0; i < boardSize; i++)
                {
                    targetNode = targetNode.Up;
                    targetNode.nodeData.Combined = false;
                }
                lineProcessing[routineCount] = false;

                if (routineCount == boardSize - 1 && Moved)
                {
                    yield return new WaitUntil(() => !lineProcessing[0] && !lineProcessing[1] && !lineProcessing[2] && !lineProcessing[3]);
                    GenerateBaseUnit();

                    DirArrowControll(Vector3.forward);
                    RecordingNodes(nodes2D);
                }
                else if (routineCount == boardSize - 1)
                {
                    VfxBoardFail.Play();
                }
                yield break;
            }

            //targetnode full
            if (targetNode.nodeData.Unit)
            {
                if (targetNode.Up.nodeData.Wall || targetNode.Up.nodeData.Combined)//skip testing
                {
                    targetNode = targetNode.Down;
                    continue;
                }
                else if (!targetNode.Up.nodeData.Unit)//move
                {
                    targetNode.nodeData.Unit.gameObject.transform.position += Vector3.forward;

                    (targetNode.nodeData, targetNode.Up.nodeData) = (targetNode.Up.nodeData, targetNode.nodeData);
                    emptyNodes.Add(targetNode);
                    emptyNodes.Remove(targetNode.Up);

                    Moved = true;
                    targetNode = targetNode.Up;
                    continue;
                }
                //검사노드 왼쪽에 무언가 있음
                else CompareUnit(targetNode, targetNode.Up);
            }
            targetNode = targetNode.Down;
            yield return null;
        }
    }
    public IEnumerator SearchD(Node targetNode, int routineCount)
    {
        lineProcessing[routineCount] = true; //processing flag on

        while (true)
        {
            //search routine ending
            if (targetNode.nodeData.Wall)
            {
                for (int i = 0; i < boardSize; i++)
                {
                    targetNode = targetNode.Down;
                    targetNode.nodeData.Combined = false;
                }
                lineProcessing[routineCount] = false;

                //last routine
                if (routineCount == boardSize - 1 && Moved )
                {
                    //캐싱 실험하기
                    yield return new WaitUntil(() => !lineProcessing[0] && !lineProcessing[1] && !lineProcessing[2] && !lineProcessing[3]);
                    
                    GenerateBaseUnit();

                    DirArrowControll(Vector3.back);
                    RecordingNodes(nodes2D);
                }
                else if (routineCount == boardSize - 1)
                {
                    VfxBoardFail.Play();
                }
                yield break;
            }

            //targetnode full
            if (targetNode.nodeData.Unit)
            {
                if (targetNode.Down.nodeData.Wall || targetNode.Down.nodeData.Combined)//skip testing
                {
                    targetNode = targetNode.Up;
                    continue;
                }
                else if (!targetNode.Down.nodeData.Unit)//move when down empty
                {
                    targetNode.nodeData.Unit.gameObject.transform.position += Vector3.back;

                    //transfer node data target -> down
                    (targetNode.nodeData, targetNode.Down.nodeData) = (targetNode.Down.nodeData, targetNode.nodeData);

                    //empty node controll
                    emptyNodes.Add(targetNode);
                    emptyNodes.Remove(targetNode.Down);

                    targetNode = targetNode.Down;
                    Moved = true;
                    continue;
                }
                else CompareUnit(targetNode, targetNode.Down);
            }
            targetNode = targetNode.Up;
            yield return null;
        }
    }
    private void CompareUnit(Node targetNode, Node CompareNode)
    {
        BasicUnit targetUnit = targetNode.nodeData.Unit;
        BasicUnit CompareUnit = CompareNode.nodeData.Unit;

        if (targetUnit.CompareCom(CompareUnit))
        {

        }
        else
        {
            
        }

        #region legacy
        //if (!targetUnit.combinable) return;

        //if (targetUnit.spcies == CompareUnit.spcies)
        //{
        //    //combine
        //    if (targetUnit.GetDanger == CompareUnit.GetDanger)
        //    {
        //        if (targetUnit.LastUnit) return;

        //        int unitIndex = CompareNode.GetUnitIndex() + 1;

        //        if (unitIndex == units.Count - 1) GameManager.Inst.ClearEvent(); //clear when last index unit appear

        //        ObjectPool.Inst.ObjectPush(targetNode.GetUnit());
        //        ObjectPool.Inst.ObjectPush(CompareNode.GetUnit());

        //        Vector3 newUnitPos = new(CompareNode.XPos, 0.5f, -CompareNode.YPos);
        //        GameObject unitObj;

        //        //select combined unit species 
        //        if (targetUnit.spcies == BasicUnit.Spcies.Monster)
        //        {
        //            unitObj = units[unitIndex].gameObject;
        //            GameManager.Inst.MgrCallGameScore(units[unitIndex].GetDanger);
        //        }
        //        else unitObj = obstaclesUnits[unitIndex];

        //        GameObject instUnit = ObjectPool.Inst.ObjectPop(unitObj, newUnitPos, Quaternion.identity, null);
        //        CompareNode.nodeData.Unit = instUnit.GetComponent<BasicUnit>();

        //        emptyNodes.Remove(CompareNode);
        //        emptyNodes.Add(targetNode);

        //        Moved = true;
        //        CompareNode.nodeData.Combined = true;
        //    }
        //    else return; //if same spcies, not same Damage return
        //}
        //else
        //{
        //    #region dummy node data save
        //    Node humNode;
        //    Node monNode;
        //    HumanUnit humanUnit;
        //    MonsterUnit monsterUnit;

        //    //unit apply each spcies
        //    if (targetUnit.spcies == BasicUnit.Spcies.Human)
        //    {
        //        humNode = targetNode;
        //        monNode = CompareNode;

        //        humanUnit = (HumanUnit)targetUnit;
        //        monsterUnit = (MonsterUnit)CompareUnit;
        //    }
        //    else
        //    {
        //        humNode = CompareNode;
        //        monNode = targetNode;

        //        monsterUnit = (MonsterUnit)targetUnit;
        //        humanUnit = (HumanUnit)CompareUnit;
        //    }
        //    #endregion

        //    switch (humanUnit.Type)
        //    {
        //        case HumanUnit.HumanType.sheildman:
        //            {
        //                if (humanUnit.GetDanger >= monsterUnit.GetDanger) return;
        //                //die
        //                else
        //                {
        //                    monsterUnit.gameObject.transform.position = CompareUnit.transform.position;
        //                    humanUnit.gameObject.transform.position = CompareUnit.transform.position;

        //                    humanUnit.DeadProd();

        //                    Data dummyHumData = humNode.nodeData;
        //                    Data dummyMonData = monNode.nodeData;

        //                    //human dead monster alive
        //                    CompareNode.nodeData = dummyMonData;
        //                    targetNode.nodeData = dummyHumData;

        //                    ObjectPool.Inst.ObjectPush(targetNode.GetUnit());

        //                    emptyNodes.Remove(CompareNode);
        //                    emptyNodes.Add(targetNode);

        //                    Moved = true;
        //                    CompareNode.nodeData.Combined = true;
        //                }
        //            }
        //            break;
        //        case HumanUnit.HumanType.knight:
        //            {
        //                //kill monster unit Damage 2
        //                if (monsterUnit.GetDanger == 2)
        //                {
        //                    monsterUnit.gameObject.transform.position = CompareUnit.transform.position;
        //                    humanUnit.gameObject.transform.position = CompareUnit.transform.position;

        //                    monsterUnit.DeadProd();

        //                    Data dummyHumData = humNode.nodeData;
        //                    Data dummyMonData = monNode.nodeData;

        //                    CompareNode.nodeData = dummyHumData;
        //                    targetNode.nodeData = dummyMonData;

        //                    ObjectPool.Inst.ObjectPush(targetNode.GetUnit());

        //                    emptyNodes.Remove(CompareNode);
        //                    emptyNodes.Add(targetNode);

        //                    Moved = true;
        //                    CompareNode.nodeData.Combined = true;
        //                }
        //                //divide unit and alive
        //                else if (humanUnit.GetDanger >= monsterUnit.GetDanger)
        //                {
        //                    int unitIndex = monNode.GetUnitIndex() - 1;
        //                    Vector3 monPos = new(monNode.XPos, 0.5f, -monNode.YPos);

        //                    monsterUnit.DownGradeProd();

        //                    ObjectPool.Inst.ObjectPush(monNode.GetUnit());
        //                    GameObject instUnit = ObjectPool.Inst.ObjectPop(units[unitIndex].gameObject, monPos, Quaternion.identity, null);
        //                    monNode.nodeData.Unit = instUnit.GetComponent<BasicUnit>();

        //                    monNode.nodeData.Combined = true;
        //                    humNode.nodeData.Combined = true;
        //                    Moved = true;
        //                }
        //                //divide monster unit and die
        //                else
        //                {
        //                    int unitIndex = monNode.GetUnitIndex() - 1;
        //                    //= monsterUnit. index -1;
        //                    Vector3 Pos = new(CompareNode.XPos,0.5f,-CompareNode.YPos);

        //                    monsterUnit.DownGradeProd();
        //                    humanUnit.DeadProd();

        //                    ObjectPool.Inst.ObjectPush(monNode.GetUnit());
        //                    ObjectPool.Inst.ObjectPush(humNode.GetUnit());

        //                    GameObject instUnit = ObjectPool.Inst.ObjectPop(units[unitIndex].gameObject, Pos, Quaternion.identity, null);
        //                    CompareNode.nodeData.Unit = instUnit.GetComponent<BasicUnit>();

        //                    //empty node controll
        //                    emptyNodes.Remove(targetNode);
        //                    emptyNodes.Add(CompareNode);

        //                    //flags
        //                    CompareNode.nodeData.Combined = true;
        //                    Moved = true;
        //                }
        //            }
        //            break;
        //        case HumanUnit.HumanType.hero:
        //            {
        //                //kill large monster
        //                if (monsterUnit.GetDanger >= humanUnit.GetDanger)
        //                {
        //                    humanUnit.DeadProd();
        //                    monsterUnit.DeadProd();

        //                    ObjectPool.Inst.ObjectPush(targetNode.GetUnit());
        //                    ObjectPool.Inst.ObjectPush(CompareNode.GetUnit());

        //                    emptyNodes.Remove(targetNode);
        //                    emptyNodes.Remove(CompareNode);

        //                    CompareNode.nodeData.Combined = true;
        //                    Moved = true;
        //                }
        //                else return; //egnore
        //            }
        //            break;
        //    }
        //}
        #endregion
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
    void GenerateBaseUnit()
    {
        #region Debug
        #if UNITY_EDITOR
        if (NoGen) return;
        #endif
        #endregion
        //empty node controll
        if (emptyNodes.Count == 0) return;

        Moved = false;
        int pickNum = UnityEngine.Random.Range(0, emptyNodes.Count);
        //object create
        Vector3 newUnitPos = new(emptyNodes[pickNum].XPos, 0.5f, -emptyNodes[pickNum].YPos);

        float rand = UnityEngine.Random.Range(1, 10);

        #region Debug Forced Unit Generation
        #if UNITY_EDITOR
        if (ForcedUnitIndex != 0)
        {
            GameObject instUnit = ObjectPool.Inst.ObjectPop(units[(int)ForcedUnitIndex].gameObject, newUnitPos, Quaternion.identity, null);
            emptyNodes[pickNum].nodeData.Unit = instUnit.GetComponent<BasicUnit>();
        }
        else
        #endif
        #endregion

        //create2
        if (rand != 1)
        {
            GameObject instUnit = ObjectPool.Inst.ObjectPop(units[0].gameObject, newUnitPos, Quaternion.identity, null);
            emptyNodes[pickNum].nodeData.Unit = instUnit.GetComponent<BasicUnit>();
        }
        //create4
        else
        {
            GameObject instUnit = ObjectPool.Inst.ObjectPop(units[1].gameObject, newUnitPos, Quaternion.identity, null);
            emptyNodes[pickNum].nodeData.Unit = instUnit.GetComponent<BasicUnit>();
        }

        emptyNodes.RemoveAt(pickNum);


        //EndGameControll
        if (emptyNodes.Count >= 2)
        {
            //generate obstacle
            if (UnityEngine.Random.Range(1, 100) <= ObGenPercent)
            {
                GenerateObstracle();
            }
        }
        else if (emptyNodes.Count == 0)
        {
            StartCoroutine(GameOverCheck());
        }

        #region debuging routine
        #if UNITY_EDITOR
        if (showNodeUnit)
        {
            Debug.Log($"{nodes2D.nodes[1, 1].nodeData.Unit,80}|{nodes2D.nodes[1, 2].nodeData.Unit,80}|{nodes2D.nodes[1, 3].nodeData.Unit,80}|{nodes2D.nodes[1, 4].nodeData.Unit,80}");
            Debug.Log($"{nodes2D.nodes[2, 1].nodeData.Unit,80}|{nodes2D.nodes[2, 2].nodeData.Unit,80}|{nodes2D.nodes[2, 3].nodeData.Unit,80}|{nodes2D.nodes[2, 4].nodeData.Unit,80}");
            Debug.Log($"{nodes2D.nodes[3, 1].nodeData.Unit,80}|{nodes2D.nodes[3, 2].nodeData.Unit,80}|{nodes2D.nodes[3, 3].nodeData.Unit,80}|{nodes2D.nodes[3, 4].nodeData.Unit,80}");
            Debug.Log($"{nodes2D.nodes[4, 1].nodeData.Unit,80}|{nodes2D.nodes[4, 2].nodeData.Unit,80}|{nodes2D.nodes[4, 3].nodeData.Unit,80}|{nodes2D.nodes[4, 4].nodeData.Unit,80}");
            Debug.Log($"empty node count : {emptyNodes.Count}");
            Debug.Log($"Undo List Count : {recordNodes.Count}");
            Debug.Log($"Undo Chance : {undoChance}");
            Debug.Log("==================================================");
        }
#endif
        #endregion

        Generated = true;
    }
    void GenerateObstracle()
    {
        //empty node controll
        if (emptyNodes.Count == 0) return;

        Moved = false;
        int pickNum = UnityEngine.Random.Range(0, emptyNodes.Count);
        //object create
        Vector3 newUnitPos = new(emptyNodes[pickNum].XPos, 0.5f, -emptyNodes[pickNum].YPos);

        int per = UnityEngine.Random.Range(1, 100);
        //creat Hero 5%
        if (per <= heroPercent)
        {
            GameObject instUnit = ObjectPool.Inst.ObjectPop(obstaclesUnits[(int)HumanUnit.HumanType.hero], newUnitPos, Quaternion.identity, null);
            emptyNodes[pickNum].nodeData.Unit = instUnit.GetComponent<BasicUnit>();
        }
        //create soldier 10%
        else if (per <= KnightPercent)
        {
            GameObject instUnit = ObjectPool.Inst.ObjectPop(obstaclesUnits[(int)HumanUnit.HumanType.knight], newUnitPos, Quaternion.identity, null);
            emptyNodes[pickNum].nodeData.Unit = instUnit.GetComponent<BasicUnit>();
        }
        //create shieldman 85%
        else
        {
            GameObject instUnit = ObjectPool.Inst.ObjectPop(obstaclesUnits[(int)HumanUnit.HumanType.sheildman], newUnitPos, Quaternion.identity, null);
            emptyNodes[pickNum].nodeData.Unit = instUnit.GetComponent<BasicUnit>();
        }
        emptyNodes.RemoveAt(pickNum);
    }
    #endregion

    #region gameOverCheckRoutine
    IEnumerator GameOverCheck()
    {
        for (int i = 1; i < boardSize; i++)
        {
            for (int j = 1; j < boardSize + 1; j++)
            {
                if (!Check(nodes2D.nodes[j,i], nodes2D.nodes[j,i].Right) || !Check(nodes2D.nodes[i, j], nodes2D.nodes[i, j].Down))
                {
                    yield break;
                }
            }
        }

        yield return waitTime;
        GameManager.Inst.MgrCallGameEnd();
    }
    bool Check(Node checkNord, Node compareNode)
    {
        BasicUnit checkUnit = checkNord.nodeData.Unit;
        BasicUnit compareUnit = compareNode.nodeData.Unit;

        if (checkUnit.GetDanger == compareUnit.GetDanger)
        {
            if (checkUnit.combinable) return false;//game over not yet
            else return true;
        }
        else return true;
    }
    #endregion

    #region Recording & Undo
    void RecordingNodes(LinkedNode2D targetNodes)
    {
        if (!Generated || undoCount == 0) return;

        Generated = false;

        LinkedNode2D instNodes = new LinkedNode2D(boardSize);

        for (int y = 1; y < boardSize + 1; y++)
        {
            for (int x = 1; x < boardSize + 1; x++)
            {
                instNodes.nodes[y,x] = new Node();
                instNodes.nodes[y, x].nodeData = targetNodes.nodes[y, x].nodeData;
            }
        }

        recordNodes.Add(instNodes);
        undoChance++;

        #region testmode
        #if UNITY_EDITOR
        if (InfiniteUndo)
        {
            undoCount = int.MaxValue;
            return;    
        }
        #endif
        #endregion

        if (recordNodes.Count > undoCount + 1)
        {
            undoChance = undoCount + 1;
            recordNodes.RemoveAt(0);
        }
    }
    void CallRecordNodes()
    {
        if (undoCount == 0) return;
        recordNodes.RemoveAt(undoChance - 1);

        for (int y = 1; y < boardSize + 1; y++)
        {
            for (int x = 1; x < boardSize + 1; x++)
            {
                //current unit clear
                if (nodes2D.nodes[y, x].nodeData.Unit != null)
                {
                    ObjectPool.Inst.ObjectPush(nodes2D.nodes[y, x].GetUnit());

                    emptyNodes.Add(nodes2D.nodes[y, x]);
                }

                //exchange
                if (recordNodes[undoChance - 2].nodes[y, x].nodeData.Unit != null)
                {
                    GameObject unit = recordNodes[undoChance - 2].nodes[y, x].nodeData.Unit.gameObject;
                    Vector3 pos = new (nodes2D.nodes[y,x].XPos, 0.5f, -nodes2D.nodes[y, x].YPos);

                    GameObject instUnit = ObjectPool.Inst.ObjectPop(unit, pos, Quaternion.identity);
                    nodes2D.nodes[y, x].nodeData.Unit = instUnit.GetComponent<BasicUnit>();

                    emptyNodes.Remove(nodes2D.nodes[y, x]);
                }
            }
        }

        undoChance--;
        undoCount--;

        GameManager.Inst.TimeStop = false;
        UndoProcessing = false;
    }
    #endregion
}
