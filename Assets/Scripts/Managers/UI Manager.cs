using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

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
        PlayerInputHandler.PauseEvent += Pause;
        PlayerInputHandler.ResumeEvent += Pause;
    }



    private void OnDisable()
    {
        PlayerInputHandler.PauseEvent -= Pause;
        PlayerInputHandler.ResumeEvent -= Pause;
    }

    private void Awake()
    {
        canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
    }

    void Start()
    {
        StartSceneCanvas();
    }


    void Pause()
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