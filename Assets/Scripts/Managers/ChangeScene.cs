using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public float FadeTime;
    public Image imageToFade;



    public enum SceneToChange
    {
        MainMenu,
        GrayBox,
    };
    public SceneToChange sceneToChange;
    void Start()
    {
        //imageToFade.color = Color.clear;
        FadeOut();
    }

    void Update()
    {
    }

    void FadeOut()
    {
        imageToFade.DOFade(0, FadeTime);
    }

 
    public void changeScene()
    {
        switch (sceneToChange)
        {
            case SceneToChange.MainMenu:
                Time.timeScale = 1;
                imageToFade.DOFade(1, FadeTime).OnComplete (()=> SceneManager.LoadScene(0));
                break;
            case SceneToChange.GrayBox:
                imageToFade.DOFade(1, FadeTime).OnComplete (()=> SceneManager.LoadScene(1));;
                break;
        }
    }
}