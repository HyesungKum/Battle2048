using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransManager : MonoBehaviour
{
    [SerializeField] string managingScene = null;
    [SerializeField] string targetScene = null;

    [SerializeField] GameObject EnterVirCam = null;
    [SerializeField] GameObject MainVirCam = null;
    [SerializeField] GameObject ExitVirCam = null;

    bool startedProd = false;

    WaitUntil waitPosition = null;

    private void Awake()
    {
        float timer = Time.deltaTime;
        waitPosition = new WaitUntil(() => Camera.main.transform.position == ExitVirCam.transform.position);
        GameManager.Inst.sceneTransManagers = this.gameObject.GetComponent<SceneTransManager>();
    }

    private void Start()
    {
        Invoke(nameof(StartSceneTransProd), 0.2f);
    }

    public void StartSceneTransProd()
    {
        if (EnterVirCam != null)
            EnterVirCam.SetActive(false);

        startedProd = true;
    }
    public void EndSceneTransProd()
    {
        if(ExitVirCam != null)
            ExitVirCam.SetActive(true);

        StartCoroutine(WatchPos());
    }

    IEnumerator WatchPos()
    {
        yield return waitPosition;

        SceneManager.LoadScene(targetScene);
    }
}
