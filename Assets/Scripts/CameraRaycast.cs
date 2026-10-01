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


    [SerializeField] private float raycastDistance;

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
        Gizmos.DrawRay(virtualCamera.transform.position, virtualCamera.transform.forward * raycastDistance);
    }


    private void TryInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(virtualCamera.transform.position, virtualCamera.transform.forward, out hit,
                raycastDistance, layerMask))
            if (hit.collider.TryGetComponent(out Interactable interactable))
                interactable.Interact();
    }

    private void ShowInteractableMessage()
    {
        RaycastHit hit;
        var hasValidInteractable = false;

        if (Physics.Raycast(virtualCamera.transform.position, virtualCamera.transform.forward, out hit,
                raycastDistance, layerMask))
            if (hit.collider.TryGetComponent(out Interactable interactable))
            {
                hasValidInteractable = true;

                if (interactable != _currentInteractable && menuController != null && PressEPage != null)
                {
                    UnsubscribeCurrent();

                    _currentInteractable = interactable;
                    PressEPage.GetComponentInChildren<TextMeshProUGUI>().text = _currentInteractable.messageToShow;
                    menuController.PushPage(PressEPage);
                    _currentInteractable.OnMessageChanged += ChangeMessageToShow;
                    isShowingMessage = true;
                }
            }

        if (!hasValidInteractable && isShowingMessage)
        {
            UnsubscribeCurrent();
            menuController.PopPage();
            isShowingMessage = false;
        }
    }

    private void UnsubscribeCurrent()
    {
        if (_currentInteractable != null)
        {
            _currentInteractable.OnMessageChanged -= ChangeMessageToShow;
            _currentInteractable = null;
        }
    }


    private void ChangeMessageToShow(string messageToShow)
    {
        var pagetext = PressEPage.GetComponentInChildren<TextMeshProUGUI>();
        if (pagetext != null)
            pagetext.text = messageToShow;
    }
}