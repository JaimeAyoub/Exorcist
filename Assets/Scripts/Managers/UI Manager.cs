using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private PlayerInputHandler PlayerInputHandler;
    [SerializeField] public CanvasGroup _mainCanvas;
    [SerializeField] public CanvasGroup _combatCanvas;
    [SerializeField] private CanvasGroup _pauseCanvas;
    [SerializeField] private CanvasGroup _settingsCanvas;
    private bool _isPaused = false;
    private bool _isInSettings = false;

    public CanvasGroup[] canvases;
    public AnimationUI[] animationsPauseMenu;

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
        canvases = FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None);
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartSceneCanvas();
    }


    void Pause()
    {
        foreach (AnimationUI animation in animationsPauseMenu)
            animation.StartAnimationAction();
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

    public void ActivateCanvas(CanvasGroup canvasToActivate)
    {
        canvasToActivate.enabled = true;
        canvasToActivate.alpha = 1;
        canvasToActivate.blocksRaycasts = true;
        canvasToActivate.interactable = true;
        foreach (CanvasGroup canvas in canvases)
        {
            if (canvas == canvasToActivate)
                continue;
            canvasToActivate.enabled = false;
            canvasToActivate.alpha = 0;
            canvasToActivate.blocksRaycasts = false;
            canvasToActivate.interactable = false;
        }
    }
}