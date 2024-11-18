using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrePinDataManager : MonoBehaviour
{
    public static ScrePinDataManager Instance;

    private ScrePinUserGameData _userGameData;
    private const string USER_DATA_FILE = "user_game_screw_data";
    private ES3Settings USER_GAME_DATA_SETTINGS;
    private const string DATA_KEY = "data_game_screw";
    private Dictionary<ScrePinResourceType, List<Action<ScrePinResourceType>>> onResourceTypeChanged = new();
    public ScrePinUserGameData UserGameData { get => _userGameData; set => _userGameData = value; }
    public event Action<ScrePinResourceType, bool> OnResourceTypeChangedSuspend;


    protected void Awake()
    {
        USER_GAME_DATA_SETTINGS = new ES3Settings(ES3.EncryptionType.AES, "kdfj3485j");
        USER_GAME_DATA_SETTINGS.location = ES3.Location.File;

        if (Instance == null)
            Instance = this;
        else
            DestroyImmediate(gameObject);
        DontDestroyOnLoad(this);
        
    }

    private IEnumerator Start()
    {
        yield return null;
        LoadUserGameData();
    }

    private void LoadUserGameData()
    {
        Debug.Log("USER_DATA_FILE: " + USER_DATA_FILE);
        if (ES3.KeyExists(DATA_KEY, USER_DATA_FILE, USER_GAME_DATA_SETTINGS))
        {
            Debug.Log("Load Data");
            _userGameData = ES3.Load<ScrePinUserGameData>(DATA_KEY, USER_DATA_FILE, USER_GAME_DATA_SETTINGS);
        }
        else
        {
            Debug.Log("Save Data");
            _userGameData = new();
            SaveUserGameData();
        }

        if (_userGameData == null)
        {
        }
    }


    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            SaveUserGameData();
        }
    }

    public int GetLevel()
    {
        
        return _userGameData.GetLevelGame();
    }
    public void SetLevel(int index)
    {
       
       _userGameData.SetLevelGame(index);
        
    }

    public void SaveUserGameData()
    {
        if (_userGameData != null)
        {
            ES3.Save<ScrePinUserGameData>(DATA_KEY, _userGameData, USER_DATA_FILE, USER_GAME_DATA_SETTINGS);
        }
    }

    public void RegisterResourceTypeChangedListener(ScrePinResourceType type, Action<ScrePinResourceType> listener)
    {
        List<Action<ScrePinResourceType>> actions = GetResourceTypeChangedListeners(type);

        if (!actions.Contains(listener))
        {
            actions.Add(listener);
        }
    }

    public void UnregisterResourceTypeChangedListener(ScrePinResourceType type, Action<ScrePinResourceType> listener)
    {
        List<Action<ScrePinResourceType>> actions = GetResourceTypeChangedListeners(type);

        if (actions.Contains(listener))
        {
            actions.Remove(listener);
        }
    }

    private List<Action<ScrePinResourceType>> GetResourceTypeChangedListeners(ScrePinResourceType type)
    {
        List<Action<ScrePinResourceType>> actions = null;
        if (onResourceTypeChanged.ContainsKey(type))
        {
            actions = onResourceTypeChanged[type];
        }
        else
        {
            actions = new();
            onResourceTypeChanged[type] = actions;
        }

        return actions;
    }

    private void InvokeResourceTypeChangedListener(ScrePinResourceType type)
    {
        List<Action<ScrePinResourceType>> actions = GetResourceTypeChangedListeners(type);
        foreach (Action<ScrePinResourceType> action in actions)
        {
            action?.Invoke(type);
        }
    }

    public int Gold
    {
        get { return GetResourceTypeCount(ScrePinResourceType.Gold); }
        set
        {
            SetResourceTypeCount(ScrePinResourceType.Gold, value);
        }
    }

    public int Life
    {
        get { return GetResourceTypeCount(ScrePinResourceType.Life); }
        set
        {
            SetResourceTypeCount(ScrePinResourceType.Life, value);
        }
    }

    public int Level
    {
        get { return GetResourceTypeCount(ScrePinResourceType.Level); }
        set
        {
            SetResourceTypeCount(ScrePinResourceType.Level, value);
        }
    }
    public bool RemoveAds
    {
        get { return GetResourceTypeCount(ScrePinResourceType.RemoveAds) >= 1; }
        set
        {
            SetResourceTypeCount(ScrePinResourceType.RemoveAds, value ? 1 : 0);
        }
    }

 /*   public void ClaimResourceItems(List<ResourceItem> resourceItems)
    {
        foreach (ResourceItem resourceItem in resourceItems)
        {
            int curValue = GetResourceTypeCount(resourceItem.type);
            curValue += resourceItem.quatity;
            SetResourceTypeCount(resourceItem.type, curValue);
        }
    }*/

    public int GetResourceTypeCount(ScrePinResourceType resourceType)
    {
        return _userGameData.GameResources[resourceType];
    }

    public void SetResourceTypeCount(ScrePinResourceType resourceType, int value)
    {
        _userGameData.GameResources[resourceType] = value;
        InvokeResourceTypeChangedListener(resourceType);
    }

    public void SupspendResourceTypeChanged(ScrePinResourceType resourceType)
    {
        OnResourceTypeChangedSuspend?.Invoke(resourceType, true);
    }

    public void ResumeResourceTypeChanged(ScrePinResourceType resourceType)
    {
        OnResourceTypeChangedSuspend?.Invoke(resourceType, false);
    }

    public void TriggerResourceTypeChanged(ScrePinResourceType resourceType)
    {
        InvokeResourceTypeChangedListener(resourceType);
    }
}

