using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
   public Canvas pauseCanvas;
   public Canvas mainCanvas;
    private bool _isPaused = false;

    void Start()
    {
        pauseCanvas.enabled = false;
        mainCanvas.enabled = true;
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
                mainCanvas.enabled = false;
                Time.timeScale = 0;
            }else if (!_isPaused)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                pauseCanvas.enabled = false;
                mainCanvas.enabled = true;
                Time.timeScale = 1;
            }
        }
    }
}