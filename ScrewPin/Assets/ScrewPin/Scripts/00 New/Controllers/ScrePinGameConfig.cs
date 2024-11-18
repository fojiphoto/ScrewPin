using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrePinGameConfig : MonoBehaviour
{
    public static ScrePinGameConfig Instance;
    public ScrePinLevelConfig levelConfig;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            DestroyImmediate(gameObject);
        DontDestroyOnLoad(this);
        Application.targetFrameRate = 60;
    }


}
