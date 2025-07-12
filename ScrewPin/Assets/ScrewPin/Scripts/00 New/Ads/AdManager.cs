
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamePix;
using DG.Tweening;

public class AdManager : MonoBehaviour
{
    public static AdManager instance;
    
    
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != instance)
            {
                Destroy(gameObject);
            }
        }
        
    }
    
   

   
    
    public void ShowInter()
    {

        Gpx.Ads.InterstitialAd(OnInterstitalAdSuccess);

    }
    [AOT.MonoPInvokeCallback(typeof(Gpx.gpxCallback))]
    public static void OnInterstitalAdSuccess()
    {
        Gpx.Log("SUCCESS");
    }
    private static Action onSuccess;
    public void ShowReward(Action complete)
    {
        onSuccess = complete;
        Gpx.Ads.RewardAd(OnRewardAdSuccess, OnRewardAdFail);
    }

    [AOT.MonoPInvokeCallback(typeof(Gpx.gpxCallback))]
    public static void OnRewardAdSuccess()
    {
        onSuccess?.Invoke();
        onSuccess = null;
    }

    [AOT.MonoPInvokeCallback(typeof(Gpx.gpxCallback))]
    public static void OnRewardAdFail()
    {
        onSuccess = null;
    }

}
