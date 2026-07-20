using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRaycast : UnityUtils.Singleton<CameraRaycast>
{
    public CinemachineVirtualCameraBase virtualCamera;
    LayerMask layerMask;
    InputAction interactAction;

    public MenuController menuController;
    public Page PressEPage;

    private bool isShowingMessage = false;


    protected override void Awake()
    {
        layerMask = LayerMask.GetMask("Interactable", "Player");
    }

    void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    void Update()
    {
        if (interactAction.WasPerformedThisFrame())
        {
            TryInteract();
        }

        ShowInteractableMessage();
    }


    private void TryInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(virtualCamera.transform.position, virtualCamera.transform.forward, out hit, 5.0f,
                layerMask))
        {
            if (hit.collider.TryGetComponent<Interactable>(out Interactable interactable))
            {
                menuController.PopPage();
                interactable.Interact();
            }
        }
    }

    private void ShowInteractableMessage()
    {
        RaycastHit hit;
        if (Physics.Raycast(virtualCamera.transform.position, virtualCamera.transform.forward, out hit, 5.0f,
                layerMask))
        {
            if (hit.collider.TryGetComponent<Interactable>(out Interactable interactable) && !isShowingMessage)
            {
                if (menuController != null && PressEPage != null)
                {
                    menuController.PushPage(PressEPage);
                    isShowingMessage = true;
                }
            }
        }
        else
        {
            menuController.PopPage();
            isShowingMessage = false;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(virtualCamera.transform.position, virtualCamera.transform.forward * 5.0f);
    }
}