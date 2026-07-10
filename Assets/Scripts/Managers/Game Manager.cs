
using System.Collections;
using UnityEngine;
using UnityUtils;

public class GameManager : Singleton<GameManager>
{

    public void EnableCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
        
    }

    public void DisableCursor()
    {
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
       
    }
    
    public void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }



}
