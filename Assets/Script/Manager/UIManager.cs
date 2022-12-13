using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    [Header("Panel")]
    [SerializeField] GameObject losePanel = null;

    [Header("Object UI")]
    [SerializeField] GameObject TimeSlider = null;
    private MeshRenderer TimeSliderRenderer = null;
    [SerializeField] GameObject ScoreSlider = null;

    [Header("Text UI")]
    [SerializeField] TextMeshProUGUI timer = null;
    [SerializeField] TextMeshProUGUI score = null;
    private int ScoreValue = 0;

    private void Awake()
    {
        //chain
        StaticEventReciver.scorePlus += ScorePuls;
        StaticEventReciver.timePlus += TimePlus;
        StaticEventReciver.gameEnd += ActiveLosePanel;

        //production
        TimeSliderRenderer = TimeSlider.GetComponentInChildren<MeshRenderer>();
        losePanel.SetActive(false);
    }

    void Update()
    {
        ScoreControll();
        TimerControll(TimeSlider);
    }

    private void OnDisable()//unchain
    {
        StaticEventReciver.scorePlus -= ScorePuls;
        StaticEventReciver.timePlus -= TimePlus;
        StaticEventReciver.gameEnd -= ActiveLosePanel;
    }

    void ScoreControll()
    {
        ScoreValue = GameManager.Inst.Score;

        if (ScoreValue > 1000)
        {
            score.text = $"{ScoreValue / 1000}.{ScoreValue % 1000 / 100}K";
        }
        else if (ScoreValue > 10000)
        {
            score.text = $"{ScoreValue / 10000}.{ScoreValue % 10000 / 1000}M";
        }
        else
        {
            score.text = ScoreValue.ToString();
        }
    }
    void ActiveLosePanel()
    {
        losePanel.SetActive(true);
    }
    void TimerControll(GameObject targetObj)
    {
        timer.text = string.Format("{0:0.00}", GameManager.Inst.TimeRemain);
        if (targetObj.transform.localScale.y >= 0)
        {
            //scale
            float sliderX = targetObj.transform.localScale.x;
            float sliderY = GameManager.Inst.TimeRemain / GameManager.Inst.TimeLimit;
            if (sliderY > 1f) sliderY = 1f;
            float sliderZ = targetObj.transform.localScale.z;

            //color
            float r = 1 - sliderY;
            float g = 0.2f;
            float b = sliderY;

            //apply
            TimeSliderRenderer.material.color = new Color(r, g, b, 0.7f);
            targetObj.transform.localScale = new Vector3(sliderX, sliderY, sliderZ);
        }
    }
    void ScorePuls(int value)
    {
        float sliderX = ScoreSlider.transform.localScale.x;
        float sliderY = (float)GameManager.Inst.FillScore / (float)GameManager.Inst.TargetFillScore;
        float sliderZ = ScoreSlider.transform.localScale.z;

        ScoreSlider.transform.localScale = new(sliderX, sliderY, sliderZ);
    }
    void TimePlus()
    {
        StartCoroutine(nameof(ScoreReset));
        float sliderX = ScoreSlider.transform.localScale.x;
        float sliderY = GameManager.Inst.TimeRemain + GameManager.Inst.AddRemainTime / GameManager.Inst.TimeLimit;
        float sliderZ = ScoreSlider.transform.localScale.z;

        TimeSlider.transform.localScale = new(sliderX, sliderY, sliderZ);
    }
    IEnumerator ScoreReset()
    {
        float sliderX = ScoreSlider.transform.localScale.x;
        float sliderY = ScoreSlider.transform.localScale.y;
        float sliderZ = ScoreSlider.transform.localScale.z;

        Vector3 origin = new(sliderX, 0f, sliderZ);

        while (true)
        {
            sliderY = Mathf.Lerp(sliderY, 0, Time.deltaTime * 3f);   

            ScoreSlider.transform.localScale = new Vector3(sliderX, sliderY, sliderZ);

            if (ScoreSlider.transform.localScale.y <= 0.1f)
            {
                ScoreSlider.transform.localScale = origin;
                yield break;
            }
            yield return null;
        }
    }
}
