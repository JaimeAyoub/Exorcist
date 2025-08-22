using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    [SerializeField] private string typing = "Typing";

    [Header("UI Action Name References")]
    [SerializeField] private string pause = "Pause";
    [SerializeField] private string resume = "Resume";
    [SerializeField] private string click = "Click";

    // Player InputActions
    private InputAction movementAction;
    private InputAction rotationAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction pauseAction;

    // Typing InputActions
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

    private void EnablePlayerInput()
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
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        SetGameplay();
    }


    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        playerControls.FindActionMap(playerActionMapName).Disable();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnablePlayerInput();
    }

    private void OnActiveSceneChanged(Scene previousScene, Scene newScene)
    {
        EnablePlayerInput();
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
