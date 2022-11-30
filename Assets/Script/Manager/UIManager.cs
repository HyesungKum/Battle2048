using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject losePanel = null;

    [SerializeField] TextMeshProUGUI timer = null;
    [SerializeField] TextMeshProUGUI score = null;

    private void Awake()
    {
        losePanel.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Inst.GameEnd)
        {
            losePanel.SetActive(true);
        }

        timer.text = string.Format("{0:0.00}", GameManager.Inst.Timer);
        score.text = GameManager.Inst.Score.ToString();
    }
}
