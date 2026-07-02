using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class UISelectionCursor : MonoBehaviour
{
    [SerializeField] private RectTransform cursorGraphic;

    [SerializeField] private Vector3 offset;
    [SerializeField] private float smoothSpeed = 20f;
    [SerializeField] private float bobAmount = 15f;
    [SerializeField] private float bobDuration = 0.4f;

    private RectTransform _containerRect;
    private Image _cursorImage;
    private GameObject _currentSelected;
    private bool _isFirstSelection = true;

    public SoundData chanceUISelectorSD;

    private void Awake()
    {
        _containerRect = GetComponent<RectTransform>();
        if (cursorGraphic != null)
        {
            _cursorImage = cursorGraphic.GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        _isFirstSelection = true;

        if (cursorGraphic != null)
        {
            float startX = cursorGraphic.anchoredPosition.x;
            cursorGraphic.DOAnchorPosX(startX + bobAmount, bobDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true); 
        }
    }

    private void OnDisable()
    {
        if (cursorGraphic != null) cursorGraphic.DOKill();
        _currentSelected = null;
    }

    private void Update()
    {
        GameObject newSelected = EventSystem.current.currentSelectedGameObject;

        if (newSelected == null)
        {
            if (_cursorImage != null && _cursorImage.enabled) _cursorImage.enabled = false;
            return; 
        }

        if (_cursorImage != null && !_cursorImage.enabled) _cursorImage.enabled = true;

        if (newSelected != _currentSelected)
        {
            if (!_isFirstSelection)
            {
                SoundManager.Instance.CreateSound().WithSoundData(chanceUISelectorSD).WithRandomPitch().Play();
            }

            _currentSelected = newSelected;
        }

        TrackTarget();
    }

    private void TrackTarget()
    {
        if (_currentSelected == null) return;

        RectTransform targetRect = _currentSelected.GetComponent<RectTransform>();
        if (targetRect == null) return;

        Vector3[] corners = new Vector3[4];
        targetRect.GetWorldCorners(corners);


        Vector3 leftEdgeCenter = (corners[0] + corners[1]) / 2f;

        Vector3 targetPosition = leftEdgeCenter + offset;

        if (_isFirstSelection)
        {
            _containerRect.position = targetPosition;
            _isFirstSelection = false;
        }
        else
        {
            _containerRect.position = Vector3.Lerp(_containerRect.position, targetPosition, Time.unscaledDeltaTime * smoothSpeed);
        }
    }
}