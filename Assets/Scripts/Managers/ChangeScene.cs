using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityUtils;

public class ChangeScene : MonoBehaviour
{
    public float fadeTime;
    public Image imageToFade;
    public Image imgTutorial;
    public RectTransform MainMenu;
    public CanvasGroup TutorialPanel;
    public Image comicImage;

    public AudioSource audioSource;
    public AudioClip hoverClip;
    public AudioClip clickClip;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    public enum SceneToChange
    {
        MainMenu,
        Level,
        GameOver,
        Credits,
    };

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        foreach (Button b in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            RegisterButton(b);
        }

        imageToFade.enabled = true;
        FadeOut();
        if (SceneManager.GetActiveScene().buildIndex == 1) return;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void RegisterButton(Button b)
    {
        EventTrigger trigger = b.GetComponent<EventTrigger>();
        if (trigger == null) trigger = b.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entry.callback.AddListener(_ =>
        {
            if (b.interactable) PlayHover();
        });
        trigger.triggers.Add(entry);

        b.onClick.AddListener(PlayClick);
    }

    public void PlayHover()
    {
        if (hoverClip != null) audioSource.PlayOneShot(hoverClip, sfxVolume);
    }

    public void PlayClick()
    {
        if (clickClip != null) audioSource.PlayOneShot(clickClip, sfxVolume);
    }

    void FadeOut()
    {
        imageToFade.DOFade(0, fadeTime);
    }

    public void Tutorial()
    {
        imageToFade.DOFade(1, fadeTime).OnComplete(() => imageToFade.DOFade(0, fadeTime));
        MainMenu.gameObject.SetActive(false);
        imageToFade.DOFade(1, fadeTime).OnComplete(() =>
        {
            TutorialPanel.DOFade(1, fadeTime);
            TutorialPanel.gameObject.SetActive(true);
            TutorialPanel.alpha = 1;
            TutorialPanel.interactable = true;
            TutorialPanel.blocksRaycasts = true;
        });
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

    public void ExitGame()
    {
        Application.Quit();
    }
}