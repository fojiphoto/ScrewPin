using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScrePinLoadingManager : MonoBehaviour
{
    private void Start()
    {
        
        Invoke(nameof(LoadScene), 7f);
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(1);
    }
}