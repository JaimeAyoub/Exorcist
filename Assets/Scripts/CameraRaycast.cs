using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityUtils;

public class CameraRaycast : Singleton<CameraRaycast>
{
    public CinemachineVirtualCameraBase virtualCamera;

    public MenuController menuController;
    public Page PressEPage;

    private Interactable _currentInteractable;
    private InputAction interactAction;

    private bool isShowingMessage;
    private LayerMask layerMask;


    protected override void Awake()
    {
        layerMask = LayerMask.GetMask("Interactable", "Player");
    }

    private void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    private void Update()
    {
        if (interactAction.WasPerformedThisFrame()) TryInteract();

        ShowInteractableMessage();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(virtualCamera.transform.position, virtualCamera.transform.forward * 5.0f);
    }


    private void TryInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(virtualCamera.transform.position, virtualCamera.transform.forward, out hit, 5.0f,
                layerMask))
            if (hit.collider.TryGetComponent(out Interactable interactable))
            {
                _currentInteractable = interactable;
                _currentInteractable.Interact();
            }
        //menuController.PopPage();
    }

    private void ShowInteractableMessage()
    {
        RaycastHit hit;
        if (Physics.Raycast(virtualCamera.transform.position, virtualCamera.transform.forward, out hit, 5.0f,
                layerMask))
        {
            if (hit.collider.TryGetComponent(out Interactable interactable) && !isShowingMessage)
                if (menuController != null && PressEPage != null)
                {
                    _currentInteractable = interactable;
                    PressEPage.GetComponentInChildren<TextMeshProUGUI>().text = _currentInteractable.messageToShow;
                    menuController.PushPage(PressEPage);
                    _currentInteractable.OnMessageChanged += ChangeMessageToShow;
                    isShowingMessage = true;
                }
        }
        else
        {
            if (_currentInteractable != null)
            {
                _currentInteractable.OnMessageChanged -= ChangeMessageToShow;
                _currentInteractable = null;
            }

            menuController.PopPage();
            isShowingMessage = false;
        }
    }


    private void ChangeMessageToShow(string messageToShow)
    {
        var pagetext = PressEPage.GetComponentInChildren<TextMeshProUGUI>();
        if (pagetext != null)
            pagetext.text = messageToShow;
    }
}