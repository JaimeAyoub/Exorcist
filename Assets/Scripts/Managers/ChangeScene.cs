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
        Level,
        GameOver,
        Credits,
    };
    //Los indices de las escenas son: 0 es MainMeu, 1 es el nivel y 2 es el Game Over
    void Start()
    {
        imageToFade.enabled = true;
        FadeOut();
        if (SceneManager.GetActiveScene().buildIndex == 1) return;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }



    void FadeOut()
    {
        imageToFade.DOFade(0, fadeTime);
    }

    public void Tutorial()
    {
        imageToFade.DOFade(1, fadeTime).OnComplete(() => imageToFade.DOFade(0, fadeTime));
        MainMenu.gameObject.SetActive(false);
        imgTutorial.DOFade(1, fadeTime).OnComplete(() => imgTutorial.SetActive());
    }


    public void SelectSceneT(int sceneIndex)
    {
        Time.timeScale = 1;
        ChangeSceneFunction((SceneToChange)sceneIndex);
    }

    private void ChangeSceneFunction(SceneToChange sceneToChange)
    {
        Time.timeScale = 1;
        switch (sceneToChange)
        {
            case SceneToChange.MainMenu:
                imageToFade.DOFade(1, fadeTime).OnComplete(() => SceneManager.LoadScene(0));
                break;
            case SceneToChange.Level:
                imageToFade.DOFade(1, fadeTime).OnComplete(() => SceneManager.LoadScene(1));
                break;
            case SceneToChange.GameOver:
                imageToFade.DOFade(1, fadeTime).OnComplete(() => SceneManager.LoadScene(2));
                break;
            case SceneToChange.Credits:
                imageToFade.DOFade(1, fadeTime).OnComplete(() => SceneManager.LoadScene(3));
                break;
            default:
                imageToFade.DOFade(1, fadeTime).OnComplete(() => SceneManager.LoadScene(0));
                break;
        }
    }

 

    public void ShowComic()
    {
        imageToFade.DOFade(1, fadeTime).OnComplete(() => imageToFade.DOFade(0, fadeTime));
        MainMenu.gameObject.SetActive(false);
        comicImage.DOFade(1, fadeTime).OnComplete(() => comicImage.SetActive());
    }

    public void ChangeToLevel()
    {
        imageToFade.DOFade(1, fadeTime).OnComplete(() => SceneManager.LoadScene(1));
    }

    public void GameOverScene()
    {
    }
}