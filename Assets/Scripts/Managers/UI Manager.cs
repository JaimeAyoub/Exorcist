using System.Runtime.CompilerServices;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public Canvas PauseCanvas;
    public Canvas MainCanvas;
    private bool isPaused = false;

    void Start()
    {
        PauseCanvas.enabled = false;
        MainCanvas.enabled = true;
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
            isPaused = !isPaused;
            if (isPaused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                PauseCanvas.enabled = true;
                MainCanvas.enabled = false;
                Time.timeScale = 0;
            }else if (!isPaused)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                PauseCanvas.enabled = false;
                MainCanvas.enabled = true;
                Time.timeScale = 1;
            }
        }
    }
}