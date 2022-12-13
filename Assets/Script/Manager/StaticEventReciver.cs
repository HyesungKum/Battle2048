using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void GameEnd();
public delegate void ScorePlus(int value);
public delegate void TimePlus();
public delegate void StageClear();
public static class StaticEventReciver
{
    //Declaration
    static public GameEnd gameEnd = null;
    static public ScorePlus scorePlus = null;
    static public TimePlus timePlus = null;
    static public StageClear stageClear = null;

    //Definition
    static public void CallScorePlus(int value)
    {
        scorePlus?.Invoke(value);
    }
    static public void CallTimePlus()
    {
        timePlus?.Invoke();
    }
    static public void CallGameEnd()
    {
        gameEnd?.Invoke();
    }
    static public void CallStageClear()
    {
        stageClear?.Invoke();
    }

}
