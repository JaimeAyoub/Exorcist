using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Reflection;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset playerControls;
    
    [Header("Action Map Name Reference")]
    [SerializeField] private string playerActionMapName = "Player";
    [SerializeField] private string uiActionMapName = "UI";
    [SerializeField] private string typingActionMapName = "Typing";

    [Header("Player Action Name References")]
    [SerializeField] private string movement = "Movement";
    [SerializeField] private string rotation = "Rotation";
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string sprint = "Sprint";

    [Header("Type Action Name References")]
    [SerializeField] private string typing = "Type";
    [SerializeField] private string mayus = "Mayus";

    [Header("UI Action Name References")]
    [SerializeField] private string pause = "Pause";
    [SerializeField] private string resume = "Resume";
    [SerializeField] private string click = "Click";

    // Player InputActions
    private InputAction _movementAction;
    private InputAction _rotationAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _pauseAction;
    private InputAction _mayusAction;
    
    // Typing InputActions
    private InputAction _typingAction;

    // Player InputActions
    private InputAction _resumeAction;

    // Player Events
    public static event Action MovementEvent;
    public static event Action StopMovementEvent;
    public event Action<char> KeyTypedEvent;
    public event Action PauseEvent;

    // UI Events
    public event Action ResumeEvent;

    public Vector2 MovementInput { get; private set; }
    public Vector2 RotationInput { get; private set; }
    public bool JumpTriggered { get; private set; }
    public bool SprintTriggered { get; private set; }   
    public bool IsInMayus { get; private set; }    
    
    private void Awake()
    {
        var playerMapReference = playerControls.FindActionMap(playerActionMapName);
        var uiMapReference = playerControls.FindActionMap(uiActionMapName);
        var typingMapReference = playerControls.FindActionMap(typingActionMapName);

        _movementAction = playerMapReference.FindAction(movement);
        _rotationAction = playerMapReference.FindAction(rotation);
        _jumpAction = playerMapReference.FindAction(jump);
        _sprintAction = playerMapReference.FindAction(sprint);
        _pauseAction = playerMapReference.FindAction(pause);

        _resumeAction = uiMapReference.FindAction(resume);

        _typingAction = typingMapReference.FindAction(typing);
        _mayusAction = typingMapReference.FindAction(mayus);
        
        SubscribeActionValuesToInputEvents();
    }

    private void SubscribeActionValuesToInputEvents()
    {
        _movementAction.performed += OnPlayerMove;
        _movementAction.canceled += OnStopPlayerMove;


        _rotationAction.performed += inputInfo => RotationInput = inputInfo.ReadValue<Vector2>();
        _rotationAction.canceled += _ => RotationInput = Vector2.zero;


        _jumpAction.performed += _ => JumpTriggered = true;
        _jumpAction.canceled += _ => JumpTriggered = false;


        _sprintAction.performed += _ => SprintTriggered = true;
        _sprintAction.canceled += _ => SprintTriggered = false;


        _pauseAction.performed += OnPause;
        _resumeAction.performed += OnResume;

        _typingAction.performed += OnKeyTyped;
        _mayusAction.performed += _ => IsInMayus = true;
        _mayusAction.canceled += _ => IsInMayus = false;
    }

    private void OnKeyTyped(InputAction.CallbackContext ctx)
    {
        Debug.Log(ctx.control.name);
        var endChar = ctx.control.name; 
        Debug.Log(endChar);
        if (endChar == "space")
        {
            KeyTypedEvent?.Invoke(' ');
            Debug.Log(endChar);
        }
        KeyTypedEvent?.Invoke(endChar[0]);
        Debug.Log(endChar);
    }

    private void OnPause(InputAction.CallbackContext ctx)
    {
        PauseEvent?.Invoke();
        SetUI();
    }

    private void OnResume(InputAction.CallbackContext ctx)
    {
        ResumeEvent?.Invoke();
        SetGameplay();
    }

    private void OnPlayerMove(InputAction.CallbackContext ctx)
    {
        MovementEvent?.Invoke();
        MovementInput = ctx.ReadValue<Vector2>();
    }

    private void OnStopPlayerMove(InputAction.CallbackContext ctx)
    {
        StopMovementEvent?.Invoke();
        MovementInput = Vector2.zero;
    }

    private void OnEnable()
    {
        SetGameplay();
    }


    private void OnDisable()
    {
        playerControls.FindActionMap(playerActionMapName).Disable();
        playerControls.FindActionMap(uiActionMapName).Disable();
        playerControls.FindActionMap(typingActionMapName).Disable();
    }
    

    private void SetGameplay()
    {
        playerControls.FindActionMap(uiActionMapName).Disable();
        playerControls.FindActionMap(playerActionMapName).Enable();
        playerControls.FindActionMap(typingActionMapName).Enable();
    }
    private void SetUI()
    {
        playerControls.FindActionMap(playerActionMapName).Disable();
        playerControls.FindActionMap(typingActionMapName).Disable();
        playerControls.FindActionMap(uiActionMapName).Enable();
    }
}
