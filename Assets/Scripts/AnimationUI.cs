using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;
using UnityEngine.Events;

public class AnimationUI : MonoBehaviour
{

    public AnimationCurve animationCurve;
    public float animationDuration;
    [FormerlySerializedAs("Delay")] public float delay;
    private GameObject _objectToAnimate;
    private RectTransform _rectTransform;
    private Vector3 _startPosObject;
    private Vector3 _endPosObject;
    private Vector2 currrentPosition;
    public bool animateOnStart = true;
    private float _offSetX;
    private float _offSetY;

    public UnityAction StartAnimationAction;

    public enum AnimationType
    {
        LeftToRight,
        RightToLeft,
        TopToDown,
        DownToTop
    }

    private void Awake()
    {
        StartAnimationAction += Animate;
    }

    public AnimationType selectAnimationType;

    void Start()
    {
        _offSetX = 1920;
        _offSetY = 1080;
        _objectToAnimate = this.gameObject;
        _rectTransform = _objectToAnimate.GetComponent<RectTransform>();
        _endPosObject = _rectTransform.anchoredPosition;
        currrentPosition = _rectTransform.anchoredPosition;
    }


    void Animate()
    {
        switch (selectAnimationType)
        {
            case AnimationType.LeftToRight:
                _startPosObject = new Vector2(currrentPosition.x - _offSetX,
                    _rectTransform.anchoredPosition.y);
                break;
            case AnimationType.RightToLeft:
                _startPosObject = new Vector2(currrentPosition.x + _offSetX,
                    currrentPosition.y);
                break;
            case AnimationType.DownToTop:
                _startPosObject = _startPosObject = new Vector2(currrentPosition.x,
                    currrentPosition.y - _offSetY);
                break;
            case AnimationType.TopToDown:
                _startPosObject = _startPosObject = new Vector2(currrentPosition.x,
                    currrentPosition.y + _offSetY);
                break;
            default:
                Debug.Log("No se eligio ningun animacion");
                break;
        }

        _rectTransform.anchoredPosition = _startPosObject;
        _rectTransform.DOAnchorPos(_endPosObject, animationDuration).SetEase(animationCurve)
            .SetDelay(delay).SetUpdate(true);
    }
}