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
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        canvases = new CanvasGroup[] { _mainCanvas, _combatCanvas, _pauseCanvas, _settingsCanvas };
    
    }

    void Start()
    {
        StartSceneCanvas();
    }


    void Pause()
    {
            _isPaused = !_isPaused;
            if (_isPaused == true)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                ActivateCanvas(_pauseCanvas);
                foreach (AnimationUI animation in animationsPauseMenu)
                    animation.StartAnimationAction();
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

    void StartSceneCanvas()
    {
        ActivateCanvas(_mainCanvas);
    }


  

    public void ActivateCanvas(CanvasGroup canvasToActivate)
    {
        foreach (CanvasGroup canvas in canvases)
        {
            if (canvas == canvasToActivate)
            {
                canvas.alpha = 1;
                canvas.blocksRaycasts = true;
                canvas.interactable = true;
            }
            else
            {
                canvas.alpha = 0;
                canvas.blocksRaycasts = false;
                canvas.interactable = false;
            }
        }
    }
}