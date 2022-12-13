using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoSingleTon<GameManager>
{
    [Header("Child Manager")]
    [SerializeField] public SceneTransManager sceneTransManagers = null;

    [Header("Game Managing Value")]
    [SerializeField] public float TimeLimit = 60f;

    [SerializeField] private float InitTime = 30f;
    [SerializeField] private int UpRequireSocre = 100;
    [SerializeField] public int TargetFillScore = 500;
    [SerializeField] public float AddRemainTime = 15f;

    public bool GameEnd { get; set; }
    public int FillScore = 0;

    public float TimeRemain { get; set; }
    public float Timer {get; set;}
    public int Score { get; set; }
    public int OldScore { get; set; }
    
    public bool TimeStop { get; set; }

    private void Awake()
    {
        if (FindObjectsOfType<GameManager>().Length == 2) Destroy(this.gameObject);
        sceneTransManagers = FindObjectOfType<SceneTransManager>();
        DontDestroyOnLoad(this);
        Initializing();
        DelegateChain();
    }

    void Update()
    {
        //timer
        if (!GameManager.Inst.TimeStop)
        {
            GameManager.Inst.Timer += Time.deltaTime;
            GameManager.Inst.TimeRemain -= Time.deltaTime;
        }

        //time out
        if (GameManager.Inst.TimeRemain <= 0)
        {
            GameManager.Inst.MgrCallGameEnd();
        }

        //restart
        if (Input.GetKeyDown(KeyCode.R))
        {
            ObjectPool.Clear();
            SceneManager.LoadScene("ForestScene");
            Initializing();
        }

        //pause Game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void DelegateChain()
    {
        StaticEventReciver.stageClear += ClearEvent;
        StaticEventReciver.scorePlus += ScoreControll;
        StaticEventReciver.timePlus += FillTime;
    }
    void Initializing()
    {
        Time.timeScale = 1f;

        GameManager.Inst.GameEnd = false;

        //time
        GameManager.Inst.TimeRemain = GameManager.Inst.InitTime;
        GameManager.Inst.Timer = 0f;
        GameManager.Inst.TimeStop = false;

        //score
        GameManager.Inst.Score = 0;
        GameManager.Inst.OldScore = 0;
        GameManager.Inst.FillScore = 0;

        //game condition
        GameManager.Inst.GameEnd = false;
    }
    public void ScoreControll(int value)
    {
        //total score controll
        GameManager.Inst.Score += value;

        //time fill in score controll
        GameManager.Inst.FillScore += value;
        if (GameManager.Inst.FillScore > GameManager.Inst.TargetFillScore)
        {
            MgrCallFillTime();
            GameManager.Inst.TargetFillScore += UpRequireSocre;
            GameManager.Inst.FillScore = 0;
        }
    }
    public void ClearEvent()//call when clear stage
    {
        if (sceneTransManagers != null)
        {
            sceneTransManagers.EndSceneTransProd();
        }
    }
    public void FillTime()
    {
        GameManager.Inst.TimeRemain += AddRemainTime;
    }

    //=================================================public Call===================================
    public void MgrCallGameEnd()
    {
        StaticEventReciver.CallGameEnd();
        GameManager.Inst.GameEnd = true;
        Time.timeScale = 0;
    }
    public void MgrCallGameScore(int value)
    {
        StaticEventReciver.CallScorePlus(value);
    }
    public void MgrCallFillTime()
    {
        StaticEventReciver.CallTimePlus();
    }
}
