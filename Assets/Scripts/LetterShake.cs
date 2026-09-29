using DG.Tweening;
using UnityEngine;

public class LetterShake : MonoBehaviour
{
    [SerializeField] private RectTransform visual;       // Si es null, usa este mismo objeto
    [SerializeField] private float angle = 5f;           // Grados de rotación máxima
    [SerializeField] private float positionAmountX = 2f; // Píxeles (solo si visual es hijo)
    [SerializeField] private float positionAmountY = 2f; // Píxeles (solo si visual es hijo)
    [SerializeField] private float speed = 0.5f;          // Duración de un ciclo de shake
    [SerializeField] private int vibrato = 10;          // Duración de un ciclo de shake

    private bool _canMove;
    private Vector2 _basePos;
    private Quaternion _baseRot;

    private void Awake()
    {
        if (visual == null) visual = (RectTransform)transform;
        _canMove = visual != (RectTransform)transform;
        //_canMove = true;
        _baseRot = visual.localRotation;
        _basePos = visual.anchoredPosition;
    }

    public void StartShake()
    {
        _baseRot = visual.localRotation;
        _basePos = visual.anchoredPosition;

        visual.DOKill();
        visual.DOShakeRotation(speed, new Vector3(0f, 0f, angle), vibrato, 90f)
            .SetLoops(-1, LoopType.Yoyo);

        if (_canMove)
        {
            
            visual.DOShakeAnchorPos(speed, new Vector2(positionAmountX, positionAmountY), vibrato, 90f)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void StopShake()
    {
        visual.DOKill();
        visual.localRotation = _baseRot;
        if (_canMove) visual.anchoredPosition = _basePos;
    }

    private void OnDisable()
    {
        if (visual == null) return;
        StopShake();
    }
    
}