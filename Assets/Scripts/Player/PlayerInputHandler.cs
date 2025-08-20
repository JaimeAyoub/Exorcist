using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset playerControls;


    [Header("Action Map Name Reference")]
    [SerializeField] private string playerActionMapName = "Player";
    [SerializeField] private string uiActionMapName = "UI";


    [Header("Player Action Name References")]
    [SerializeField] private string movement = "Movement";
    [SerializeField] private string rotation = "Rotation";
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string sprint = "Sprint";
    [SerializeField] private string typing = "Typing";


    [Header("UI Action Name References")]
    [SerializeField] private string pause = "Pause";
    [SerializeField] private string resume = "Resume";
    
    // Player InputActions
    private InputAction movementAction;
    private InputAction rotationAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction pauseAction;
    private InputAction typingAction;

    // Player InputActions
    private InputAction resumeAction;

    // Player Events
    public event Action PauseEvent;
    public event Action MovementEvent;
    public event Action StopMovementEvent;
    public event Action<char> KeyTypedEvent;

    // UI Events
    public event Action ResumeEvent;

    public Vector2 MovementInput { get; private set; }
    public Vector2 RotationInput { get; private set; }
    public bool JumpTriggered { get; private set; }
    public bool SprintTriggered { get; private set; }


    private void Awake()
    {
        InputActionMap playerMapReference = playerControls.FindActionMap(playerActionMapName);
        InputActionMap uiMapReference = playerControls.FindActionMap(uiActionMapName);


        movementAction = playerMapReference.FindAction(movement);
        rotationAction = playerMapReference.FindAction(rotation);
        jumpAction = playerMapReference.FindAction(jump);
        sprintAction = playerMapReference.FindAction(sprint);
        pauseAction = playerMapReference.FindAction(pause);
        typingAction = playerMapReference.FindAction(typing);

        resumeAction = uiMapReference.FindAction(resume);

        SubscribeActionValuesToInputEvents();
    }


    private void SubscribeActionValuesToInputEvents()
    {
        movementAction.performed += inputInfo => OnPlayerMove(inputInfo);
        movementAction.canceled += inputInfo => OnStopPlayerMove(inputInfo);


        rotationAction.performed += inputInfo => RotationInput = inputInfo.ReadValue<Vector2>();
        rotationAction.canceled += inputInfo => RotationInput = Vector2.zero;


        jumpAction.performed += inputInfo => JumpTriggered = true;
        jumpAction.canceled += inputInfo => JumpTriggered = false;


        sprintAction.performed += inputInfo => SprintTriggered = true;
        sprintAction.canceled += inputInfo => SprintTriggered = false;


        pauseAction.performed += inputInfo => OnPause(inputInfo);
        resumeAction.performed += inputInfo => OnResume(inputInfo);

        typingAction.performed += inputInfo => OnKeyTyped(inputInfo);

    }

    private void OnKeyTyped(InputAction.CallbackContext ctx)
    {
        KeyTypedEvent?.Invoke(ctx.control.name.ToCharArray()[0]);
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
    }

    public void SetGameplay()
    {
        playerControls.FindActionMap(playerActionMapName).Enable();
        playerControls.FindActionMap(uiActionMapName).Disable();
    }
    public void SetUI()
    {
        playerControls.FindActionMap(playerActionMapName).Disable();
        playerControls.FindActionMap(uiActionMapName).Enable();
    }

}
