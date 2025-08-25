using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;
using UnityEngine.Events;

public class AnimationUI : MonoBehaviour
{
    [Header("Animation Settings")] public AnimationCurve animationCurve;
    public float animationDuration = 0.5f;
    [FormerlySerializedAs("Delay")] public float delay = 0f;
    public bool animateOnStart = true;

    private RectTransform _rectTransform;
    private Vector2 _endPosObject;
    private Tween _currentTween;

    public UnityAction StartAnimationAction;

    public enum AnimationType
    {
        LeftToRight,
        RightToLeft,
        TopToDown,
        DownToTop
    }

    public AnimationType selectAnimationType;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _endPosObject = _rectTransform.anchoredPosition;
        StartAnimationAction += Animate;
    }

    private void Start()
    {
        if (animateOnStart)
            Animate();
    }

    public void Animate()
    {
        float offSetX = 1920;
        float offSetY = 1080;

        Vector2 startPos = _endPosObject;

        switch (selectAnimationType)
        {
            case AnimationType.LeftToRight:
                startPos = new Vector2(_endPosObject.x - offSetX, _endPosObject.y);
                break;
            case AnimationType.RightToLeft:
                startPos = new Vector2(_endPosObject.x + offSetX, _endPosObject.y);
                break;
            case AnimationType.DownToTop:
                startPos = new Vector2(_endPosObject.x, _endPosObject.y - offSetY);
                break;
            case AnimationType.TopToDown:
                startPos = new Vector2(_endPosObject.x, _endPosObject.y + offSetY);
                break;
            default:
                Debug.LogWarning("No se eligió ninguna animación");
                break;
        }

        _rectTransform.anchoredPosition = startPos;

        _currentTween?.Kill();

        _currentTween = _rectTransform.DOAnchorPos(_endPosObject, animationDuration)
            .SetEase(animationCurve)
            .SetDelay(delay)
            .SetUpdate(true); // Importante para que siga funcionando aunque Time.timeScale = 0
    }
}