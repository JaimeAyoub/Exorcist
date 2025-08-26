using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource), typeof(CanvasGroup))]
[DisallowMultipleComponent]
public class Page : MonoBehaviour
{
    private AudioSource _audioSource;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;

    [SerializeField] private float animationSpeed = 1f; 
    public bool exitOnNewPagePush = false;
    [SerializeField] private AudioClip entryClip;
    [SerializeField] private AudioClip exitClip;
    [SerializeField] private EntryMode entryMode = EntryMode.SLIDE;
    [SerializeField] private Direction entryDirection = Direction.LEFT;
    [SerializeField] private EntryMode exitMode = EntryMode.SLIDE;
    [SerializeField] private Direction exitDirection = Direction.LEFT;
    [SerializeField] private UnityEvent prePushAction;
    [SerializeField] private UnityEvent postPushAction;
    [SerializeField] private UnityEvent prePopAction;
    [SerializeField] private UnityEvent postPopAction;

    private Coroutine _animationCoroutine;
    private Coroutine _audioCoroutine;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _audioSource = GetComponent<AudioSource>();

        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
        _audioSource.spatialBlend = 0;
        _audioSource.enabled = false;
    }

    public void Enter(bool playAudio)
    {
        prePushAction?.Invoke();

        switch(entryMode)
        {
            case EntryMode.SLIDE:
                SlideIn(playAudio);
                break;
            case EntryMode.ZOOM:
                ZoomIn(playAudio);
                break;
            case EntryMode.FADE:
                FadeIn(playAudio);
                break;
        }
    }

    public void Exit(bool playAudio)
    {
		prePopAction?.Invoke();

        switch (exitMode)
        {
            case EntryMode.SLIDE:
                SlideOut(playAudio);
                break;
            case EntryMode.ZOOM:
                ZoomOut(playAudio);
                break;
            case EntryMode.FADE:
                FadeOut(playAudio);
                break;
        }
    }

    private void SlideIn(bool playAudio)
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }
        _animationCoroutine = StartCoroutine(AnimationHelper.SlideIn(_rectTransform, entryDirection, animationSpeed, postPushAction));

        PlayEntryClip(playAudio);
    }

    private void SlideOut(bool playAudio)
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }
        _animationCoroutine = StartCoroutine(AnimationHelper.SlideOut(_rectTransform, exitDirection, animationSpeed, postPopAction));

        PlayExitClip(playAudio);
    }
    
    private void ZoomIn(bool playAudio)
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }
        _animationCoroutine = StartCoroutine(AnimationHelper.ZoomIn(_rectTransform, animationSpeed, postPushAction));

        PlayEntryClip(playAudio);
    }

    private void ZoomOut(bool playAudio)
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }
        _animationCoroutine = StartCoroutine(AnimationHelper.ZoomOut(_rectTransform, animationSpeed, postPopAction));

        PlayExitClip(playAudio);
    }

    private void FadeIn(bool playAudio)
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }
        _animationCoroutine = StartCoroutine(AnimationHelper.FadeIn(_canvasGroup, animationSpeed, postPushAction));

        PlayEntryClip(playAudio);
    }

    private void FadeOut(bool playAudio)
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }
        _animationCoroutine = StartCoroutine(AnimationHelper.FadeOut(_canvasGroup, animationSpeed, postPopAction));

        PlayExitClip(playAudio);
    }

    private void PlayEntryClip(bool playAudio)
    {
        if (playAudio && entryClip != null && _audioSource != null)
        {
            if (_audioCoroutine != null)
            {
                StopCoroutine(_audioCoroutine);
            }

            _audioCoroutine = StartCoroutine(PlayClip(entryClip));
        }
    }
    
    private void PlayExitClip(bool playAudio)
    {
        if (playAudio && exitClip != null && _audioSource != null)
        {
            if (_audioCoroutine != null)
            {
                StopCoroutine(_audioCoroutine);
            }

            _audioCoroutine = StartCoroutine(PlayClip(exitClip));
        }
    }

    private IEnumerator PlayClip(AudioClip clip)
    {
        //SoundManager.Instance.CreateSound().WithSoundData(clip).Play();
        _audioSource.enabled = true;

        WaitForSeconds wait = new WaitForSeconds(clip.length);

        _audioSource.PlayOneShot(clip);

        yield return wait;

        _audioSource.enabled = false;
    }
}