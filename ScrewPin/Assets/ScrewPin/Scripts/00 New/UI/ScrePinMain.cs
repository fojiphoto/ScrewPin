using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScrePinMain : ScrePinGameMonobehavior
{
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _coinText;


    

    private void OnEnable()
    {
        Dtm.RegisterResourceTypeChangedListener(ScrePinResourceType.Gold, UpdateTextCoin);
        Dtm.RegisterResourceTypeChangedListener(ScrePinResourceType.Level, UpdateLevelText);


    }

    private void OnDisable()
    {
        Dtm.UnregisterResourceTypeChangedListener(ScrePinResourceType.Gold, UpdateTextCoin);
        Dtm.RegisterResourceTypeChangedListener(ScrePinResourceType.Level, UpdateLevelText);


    }

    private void Start()
    {
        UpdateTextCoin(ScrePinResourceType.Gold);
        UpdateLevelText(ScrePinResourceType.Level); // Initial update
    }

    

    public void UpdateTextCoin(ScrePinResourceType resourceType)
    {
        _coinText.text = Dtm.Gold.ToString();
        Debug.Log(Dtm.Gold + " " + resourceType);
    }


    public void PlayGame()
    {
        Ac.PlaySound(Ac.click);
        gameObject.SetActive(false);
        ScrPinUIManager.Instance.ShowInGameUi();
        Gm.cantClick = false;
        

    }

    public void UpdateLevelText(ScrePinResourceType resourceType)
    {
        _levelText.text = "LEVEL " + Dtm.Level.ToString();
    }
    



}
