using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : UnityUtils.Singleton<UIManager>
{
    [SerializeField] private PlayerInputHandler PlayerInputHandler;
    [SerializeField] public CanvasGroup _mainCanvas;
    [SerializeField] public CanvasGroup _combatCanvas;
    [SerializeField] private CanvasGroup _pauseCanvas;
    [SerializeField] private CanvasGroup _settingsCanvas;
    [SerializeField] private CanvasGroup _GameOverCanvas;
    public TextMeshProUGUI toogleDoorText;
    private bool _isPaused = false;
    public float _numberOfEnemies;

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
        _numberOfEnemies = 2; 
        base.Awake();
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

    public void ShowTextDoor()
    {
        toogleDoorText.enabled = true;
    }

    public void HideTextDoor()
    {
        toogleDoorText.enabled = false;
    }

    public void CheckEnd()
    {
        if (_numberOfEnemies > 0)
        {
            _numberOfEnemies--;
        }
        else if (_numberOfEnemies <= 0)
        {
            ActivateCanvas(_GameOverCanvas);
        }
    }
}