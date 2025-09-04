using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityUtils;

public class ChangeScene : MonoBehaviour
{
    public float fadeTime;
    public Image imageToFade;
    public Image imgTutorial;
    public RectTransform MainMenu;
    public Image comicImage;
    public enum SceneToChange
    {
        MainMenu,
        GrayBox,
    };
    public SceneToChange sceneToChange;
    void Start()
    {
        imageToFade.enabled = true;
        FadeOut();
    }

    void Update()
    {
    }

    void FadeOut()
    {
        imageToFade.DOFade(0, fadeTime);
    }

    public void Tutorial()
    {
        imageToFade.DOFade(1, fadeTime).OnComplete(()=>imageToFade.DOFade(0, fadeTime));
        MainMenu.gameObject.SetActive(false);
        imgTutorial.DOFade(1, fadeTime).OnComplete(()=>imgTutorial.SetActive());
    }
    
    
    public void ChangeSceneF()
    {
        switch (sceneToChange)
        {
            case SceneToChange.MainMenu:
                Time.timeScale = 1;
                imageToFade.DOFade(1, fadeTime).OnComplete (()=> SceneManager.LoadScene(0));
                break;
            case SceneToChange.GrayBox:
                imageToFade.DOFade(1, fadeTime).OnComplete (()=> SceneManager.LoadScene(1));
                break;
        }
    }

    public void ShowComic()
    {
        imageToFade.DOFade(1, fadeTime).OnComplete(()=>imageToFade.DOFade(0, fadeTime));
        MainMenu.gameObject.SetActive(false);
        comicImage.DOFade(1, fadeTime).OnComplete(()=>comicImage.SetActive());
    }
    
    public void ChangeToLevel()
    {
        imageToFade.DOFade(1, fadeTime).OnComplete (()=> SceneManager.LoadScene(1));
    }
    
}