using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoSingleTon<GameManager>
{
    public float Timer {get; set;}
    public int Score { get; set; }

    public bool GameEnd { get; set; }

    private void Awake()
    {
        Initializing();
    }

    void Update()
    {
        //timer
        GameManager.Inst.Timer += Time.deltaTime;

        //restart
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("MainScene");
            Initializing();
        }

        //quit game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (GameEnd)
        {
            Time.timeScale = 0;
        }

    }

    void Initializing()
    {
        Time.timeScale = 1f;
        GameManager.Inst.Timer = 0f;

        GameManager.Inst.GameEnd = false;
        GameManager.Inst.Timer = 0f;
        GameManager.Inst.Score = 0;
    }
}
