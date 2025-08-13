using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;


public class AnimationUI : MonoBehaviour
{
    public AnimationCurve animationCurve;
    public float animationDuration;
    [FormerlySerializedAs("Delay")] public float delay;
    private GameObject _objectToAnimate;
    private RectTransform _rectTransform;
    private Vector3 _startPosObject;
    private Vector3 _endPosObject;
    public bool animateOnStart = true;
    private float _offSetX;
    private float _offSetY;

    public enum AnimationType
    {
        LeftToRight,
        RightToLeft,
        TopToDown,
        DownToTop
    }

    public AnimationType selectAnimationType;

    void Start()
    {
        _offSetX = 1920;
        _offSetY = 1080;
        _objectToAnimate = this.gameObject;
        _rectTransform = _objectToAnimate.GetComponent<RectTransform>();
        _endPosObject = _rectTransform.anchoredPosition;
        if(animateOnStart)
            Animate();
    }


    void Animate()
    {
        Vector2 currrentPosition = _rectTransform.anchoredPosition;
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
        _rectTransform.DOAnchorPos(_endPosObject, animationDuration).SetEase(animationCurve).SetDelay(delay);
    }
}