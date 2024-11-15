using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrePinGameMonobehavior : MonoBehaviour
{
    private ScrePinGameManager _gameManager;
    private ScrPinUIManager _uiManager;
    private ScrePinDataManager _dataManager;
    private GameConfig _gameConfig;
    private ScrePinSceneController _sceneController;
    private ScrePinGamePlayManager _gamePlayManager;
    private InputHandler _inputHandler;
    private ScrewPinAudioController _audioController;

    public ScrePinGameManager Gm
    {
        get 
        {
            if(_gameManager == null)
            {
                _gameManager = ScrePinGameManager.Instance;
            }
            return _gameManager;
        }
    }

    public ScrewPinAudioController Ac
    {
        get
        {
            if (_audioController == null)
            {
                _audioController = ScrewPinAudioController.Instance;
            }
            return _audioController;
        }
    }

    public ScrePinGamePlayManager Gpm
    {
        get
        {
            if (_gamePlayManager == null)
            {
                _gamePlayManager = ScrePinGamePlayManager.Instance;
            }
            return _gamePlayManager;
        }
    }
    public ScrPinUIManager Um
    {
        get
        {
            if (_uiManager == null)
            {
                _uiManager = ScrPinUIManager.Instance;
            }
            return _uiManager;
        }
    }

    

    public ScrePinDataManager Dtm
    {
        get
        {
            if (_dataManager == null)
            {
                _dataManager = ScrePinDataManager.Instance;
            }
            return _dataManager;
        }
    }

    public GameConfig Gc
    {
        get
        {
            if (_gameConfig == null)
            {
                _gameConfig = GameConfig.Instance;
            }
            return _gameConfig;
        }
    }

    public ScrePinSceneController Sc
    {
        get
        {
            if (_sceneController == null)
            {
                _sceneController = ScrePinSceneController.Instance;
            }
            return _sceneController;
        }
    }

    public InputHandler Iph
    {
        get
        {
            if (_inputHandler == null)
            {
                _inputHandler = InputHandler.Instance;
            }
            return _inputHandler;
        }
    }
}
