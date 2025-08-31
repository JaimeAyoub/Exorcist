using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class DoorScript : MonoBehaviour
{
    public bool isOpen;
    public GameObject pivot;

    private Vector3 _closedRotation;
    private Vector3 _openRotation;
    private Tween _currentTween;


    private void Start()
    {
        _closedRotation = pivot.transform.localEulerAngles;

        _openRotation = _closedRotation + new Vector3(0, -90, 0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && CameraRaycast.Instance.canOpen)
        {
            ToggleDoor();
        }
    }

    public void ToggleDoor()
    {
        _currentTween?.Kill();
      
        if (!isOpen)
        {
            _currentTween = pivot.transform.DORotate(_openRotation, 0.2f, RotateMode.Fast);
            isOpen = true;
        }
        else
        {
            _currentTween = pivot.transform.DORotate(_closedRotation, 0.2f, RotateMode.Fast);
            isOpen = false;
        }
    }
}