using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : UnityUtils.Singleton<UIManager>
{
    [SerializeField] private PlayerInputHandler PlayerInputHandler;
    [SerializeField] public CanvasGroup _mainCanvas;
    [SerializeField] public CanvasGroup _combatCanvas;
    [SerializeField] private CanvasGroup _pauseCanvas;
    [SerializeField] private CanvasGroup _settingsCanvas;
    [SerializeField] public CanvasGroup _GameOverCanvas;
    [SerializeField] private CanvasGroup _noteCanvas;

    public TextMeshProUGUI toogleDoorText;
    private bool _isPaused = false;
    private bool _isNoteOpen = false;
    public float _numberOfEnemies;

    public CanvasGroup[] canvases;
    public AnimationUI[] animationsPauseMenu;

    [SerializeField] private Image noteImage;

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
        _numberOfEnemies = 4;
        base.Awake();
        canvases = new CanvasGroup[] { _mainCanvas, _combatCanvas, _pauseCanvas, _settingsCanvas, _noteCanvas };
    }

    void Start()
    {
        StartSceneCanvas();
    }

    void Update()
    {
        // Cerrar nota con Escape (igual que pausa)
        if (_isNoteOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseNote();
        }
    }

    void Pause()
    {
        if (_isNoteOpen) return;

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
    public void CheckEnd()
    {
        if (_numberOfEnemies > 0)
        {
            _numberOfEnemies--;
            ActivateCanvas(_mainCanvas);
        }
        else if (_numberOfEnemies <= 0)
        {
            SceneManager.LoadScene(0);
        }
    }
    // Métodos para el sistema de notas
    public void ShowNote(Sprite noteSprite)
    {
        if (noteImage != null && _noteCanvas != null)
        {
            noteImage.sprite = noteSprite;
            _isNoteOpen = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ActivateCanvas(_noteCanvas);
            Time.timeScale = 0;
        }
    }

    public void CloseNote()
    {
        _isNoteOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
        ActivateCanvas(_mainCanvas);
    }

    public bool IsNoteOpen()
    {
        return _isNoteOpen;
    }

    public void ShowTextDoor()
    {
        if (toogleDoorText != null)
            toogleDoorText.enabled = true;
    }

    public void HideTextDoor()
    {
        if (toogleDoorText != null)
            toogleDoorText.enabled = false;
    }

    
    public void OnCloseNoteButton()
    {
        CloseNote();
    }
}