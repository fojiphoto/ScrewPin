using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ScrePinGameManager : ScrePinGameMonobehavior
{
    public static ScrePinGameManager Instance;
    [SerializeField] ScrePinBoxController _boxController;
    [SerializeField] HoleStorageController _holeStorageController;
    [SerializeField] private ScrePinLevelManager _levelManager;

    public ScrePinLevelManager LevelManager => _levelManager;
    public ScrePinBoxController BoxController { get => _boxController; set => _boxController = value; }
    public HoleStorageController HoleStorageController { get => _holeStorageController; set => _holeStorageController = value; }
    //public int levelPlaying;
    


    public bool cantClick;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            DontDestroyOnLoad(gameObject);
        cantClick = false;
        
    }


    private void Start()
    {
        ScrePinObserve.OnWin += OnWin;
        ScrePinObserve.OnLose += OnLose;
    }
    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    public void OnWin()
    {
        Debug.Log("onWin");
        IncrseaseLevel();
        StartCoroutine(ScrPinUIManager.Instance.ShowWinPopup());

    }

    public bool LimitLevel()
    {
        if(Dtm.Level >= Gc.levelConfig.GetTotalLevel())
        {
            return true;
        }
        return false;
    }
    public void OnLose()
    {

        StartCoroutine(ScrPinUIManager.Instance.ShowLosePopup());

    }
   
    public void IncrseaseLevel()
    {
        if (Dtm.Level > Gc.levelConfig.GetTotalLevel()) return;
        int indexLevel = Dtm.Level + 1;
        
        Dtm.SetLevel(indexLevel);
    }
    private void OnDestroy()
    {
        ScrePinObserve.OnWin -= OnWin;
        ScrePinObserve.OnLose -= OnLose;
    }

}
