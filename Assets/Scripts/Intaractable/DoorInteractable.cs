using DG.Tweening;
using UnityEngine;

public class DoorInteractable : Interactable
{
    public bool isOpen;
    public GameObject pivot;
    [SerializeField] private bool needKey;
    [SerializeField] private GameObject father;

    private Vector3 _closedRotation;
    private Tween _currentTween;
    private Vector3 _openRotation;

    private void Start()
    {
        _closedRotation = pivot.transform.localEulerAngles;

        _openRotation = _closedRotation + new Vector3(0, -90, 0);
    }


    public override void Interact()
    {
        if (!needKey)
        {
            OpenOrClose();
        }
        else
        {
            if (GameManager.Instance.hasKey)
            {
                OpenOrClose();
                GameManager.Instance.removeKey();
                RaiseMessageChanged("Press E to Interact");
                needKey = false;
            }
            else
            {
                RaiseMessageChanged("Falta llave");
            }
        }
    }


    private void OpenOrClose()
    {
        _currentTween?.Kill();

        if (!isOpen)
        {
            _currentTween = pivot.transform.DORotate(_openRotation, 0.2f);
            isOpen = true;
        }
        else
        {
            _currentTween = pivot.transform.DORotate(_closedRotation, 0.2f);
            isOpen = false;
        }
    }
}