using DG.Tweening;
using UnityEngine;
using UnityUtils;

public class GameManager : Singleton<GameManager>
{
    public Material DitherMat;
    public bool hasKey;

    public void Start()
    {
        if (DitherMat != null)
            DitherMat.SetFloat("_ColorResolution", 16.0f);
    }

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

    public void StartDither()
    {
        if (DitherMat != null)
            DitherMat.DOFloat(0.01f, "_ColorResolution", 0.5f).SetUpdate(true);
    }

    public void ResetDitherToDefault()
    {
        if (DitherMat != null)
            DitherMat.DOFloat(16.0f, "_ColorResolution", 1.0f).SetUpdate(true);
    }

    public void addKey()
    {
        if (!hasKey) hasKey = true;
    }

    public void removeKey()
    {
        if (hasKey) hasKey = false;
    }
}