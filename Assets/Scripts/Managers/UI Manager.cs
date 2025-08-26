using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityUtils;
using System.Reflection;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private PlayerInputHandler PlayerInputHandler;
    [SerializeField] public Canvas _mainCanvas;
    [SerializeField] public Canvas _combatCanvas;
    [SerializeField] private Canvas _pauseCanvas;
    [SerializeField] private Canvas _settingsCanvas;
    private bool _isPaused = false;
    private bool _isInSettings = false;
    
    public Canvas[] canvases;

    private void OnEnable()
    { 
        
    }

    private void OnDisable()
    {
        Instance.PlayerInputHandler.PauseEvent -= Pause;
        Instance.PlayerInputHandler.ResumeEvent -= Pause;
    }

    private new void Awake()
    {
        Instance.PlayerInputHandler.PauseEvent += Pause;
        Instance.PlayerInputHandler.ResumeEvent += Pause;
        base.Awake();
        canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
    }

    private void Start()
    {
        StartSceneCanvas();
    }


    private void Pause()
    {
        if (!_isInSettings)
        {
            _isPaused = !_isPaused;
            if (_isPaused == true)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                ActivateCanvas(_pauseCanvas);
                Time.timeScale = 0;
            }
            else if (_isPaused == false)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1;
                ActivateCanvas(_mainCanvas);
            }
        }
        else
        {
            _isInSettings = false;
            ActivateCanvas(_pauseCanvas);
        }
    }
    
    void StartSceneCanvas()
    {
        ActivateCanvas(_mainCanvas);
    }

    public void ActivateSettingsCanvas()
    {
        ActivateCanvas(_settingsCanvas);
        _isInSettings = true;
    }

    public void ActivateCanvas(Canvas canvasToActivate)
    {
        canvasToActivate.enabled = true;
        foreach (Canvas canvas in canvases)
        {
            if (canvas == canvasToActivate)
                continue;
            canvas.enabled = false;
        }
    }
}