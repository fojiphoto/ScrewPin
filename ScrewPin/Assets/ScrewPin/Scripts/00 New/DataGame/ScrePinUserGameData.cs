using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ES3Serializable]
public class ScrePinUserGameData
{
    [ES3Serializable]
    private Dictionary<ScrePinResourceType, int> gameResources;
    public Dictionary<ScrePinResourceType, int> GameResources { get => gameResources; }

    [ES3Serializable]
    private ScrePinDataLevel dataLevel;
    private bool isFirstInGame = false;
    private string version_game;

    public ScrePinUserGameData()
    {
        gameResources = new Dictionary<ScrePinResourceType, int>();
        DefaultFirstInGame();
    }

    private void DefaultFirstInGame()
    {
        if (!isFirstInGame)
        {
            if (dataLevel == null)
            {
                dataLevel = new ScrePinDataLevel();
                dataLevel.CurrentLevel = 1;
                gameResources.Add(ScrePinResourceType.RemoveAds, 0);
                gameResources.Add(ScrePinResourceType.Gold, 100);
                gameResources.Add(ScrePinResourceType.Life, 5);
                gameResources.Add(ScrePinResourceType.Level, 1);


                isFirstInGame = true;
                version_game = Application.version.ToString(); 
            }
        }
    }

    public int GetLevelGame()
    {
        return dataLevel.GetLevel();
    }

    public void SetLevelGame(int level)
    {
        dataLevel.SetLevel(level);
    }


}

public class ScrePinDataLevel
{

    public int CurrentLevel;

    public void SetLevel(int level)
    {
        CurrentLevel = level;
    }

    public int GetLevel()
    {
        return CurrentLevel;
    }


}
