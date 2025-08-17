using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public Canvas _mainCanvas;
    public Canvas _combatCanvas;
    [SerializeField] private Canvas _pauseCanvas;
    [SerializeField] private Canvas _settingsCanvas;
    private bool _isPaused = false;
    private bool _isInSettings = false;

    public Canvas[] canvases;

    private void Awake()
    {
        canvases = FindObjectsOfType<Canvas>(true);
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartSceneCanvas();
    }

    // Update is called once per frame
    void Update()
    {
        Pause();
        if (Input.GetKeyDown(KeyCode.Escape) && _isInSettings)
        {
            _isInSettings = false;
            ActivateCanvas(_pauseCanvas);
        }
    }

    void Pause()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !_isInSettings)
        {
            _isPaused = !_isPaused;
            if (_isPaused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                ActivateCanvas(_pauseCanvas);
                Time.timeScale = 0;
            }
            else if (!_isPaused)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1;
                ActivateCanvas(_mainCanvas);
            }
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