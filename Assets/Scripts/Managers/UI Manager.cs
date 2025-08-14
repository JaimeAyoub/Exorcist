using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
   public Canvas pauseCanvas;
   public Canvas mainCanvas;
   public Canvas combatCanvas;
    private bool _isPaused = false;

    void Start()
    {
        pauseCanvas.enabled = false;
        mainCanvas.enabled = true;
        
        pauseCanvas.GetComponent<CanvasGroup>().alpha = 0;
        combatCanvas.GetComponent<CanvasGroup>().alpha = 0;
        mainCanvas.GetComponent<CanvasGroup>().alpha = 1;
    }

    // Update is called once per frame
    void Update()
    {
        Pause();
    }

    void Pause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _isPaused = !_isPaused;
            if (_isPaused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                pauseCanvas.enabled = true;
                pauseCanvas.GetComponent<CanvasGroup>().alpha = 1;
                mainCanvas.enabled = false;
                Time.timeScale = 0;
            }else if (!_isPaused)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                pauseCanvas.enabled = false;
                pauseCanvas.GetComponent<CanvasGroup>().alpha = 0;
                mainCanvas.enabled = true;
                Time.timeScale = 1;
            }
        }
    }
}