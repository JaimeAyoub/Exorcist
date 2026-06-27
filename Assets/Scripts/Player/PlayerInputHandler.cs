using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    [Header("UI Action Name References")] 
    [SerializeField] private string pause = "Pause";
    [SerializeField] private string resume = "Resume";

    // Player InputActions
    private InputAction _movementAction;
    private InputAction _rotationAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    
    // UI InputActions
    private InputAction _pauseAction;
    private InputAction _resumeAction;

    // Eventos
    public event Action PauseEvent;
    public event Action ResumeEvent;
    public static event Action MovementEvent;
    public static event Action StopMovementEvent;
    
    // El evento principal de tipeo
    public event Action<char> KeyTypedEvent;

    // Propiedades de estado
    public Vector2 MovementInput { get; private set; }
    public Vector2 RotationInput { get; private set; }
    public bool JumpTriggered { get; private set; }
    public bool SprintTriggered { get; private set; }
    
    private bool _canType = false;

    private void EnablePlayerInput()
    {
        var playerMapReference = playerControls.FindActionMap(playerActionMapName);
        var uiMapReference = playerControls.FindActionMap(uiActionMapName);

        _movementAction = playerMapReference.FindAction(movement);
        _rotationAction = playerMapReference.FindAction(rotation);
        _jumpAction = playerMapReference.FindAction(jump);
        _sprintAction = playerMapReference.FindAction(sprint);
        _pauseAction = playerMapReference.FindAction(pause);
        _resumeAction = uiMapReference.FindAction(resume);

        SubscribeActionValuesToInputEvents();
    }

    private void SubscribeActionValuesToInputEvents()
    {
        _movementAction.performed += OnPlayerMove;
        _movementAction.canceled += OnStopPlayerMove;

        _rotationAction.performed += ctx => RotationInput = ctx.ReadValue<Vector2>();
        _rotationAction.canceled += _ => RotationInput = Vector2.zero;

        _jumpAction.performed += _ => JumpTriggered = true;
        _jumpAction.canceled += _ => JumpTriggered = false;

        _sprintAction.performed += _ => SprintTriggered = true;
        _sprintAction.canceled += _ => SprintTriggered = false;

        _pauseAction.performed += OnPause;
        _resumeAction.performed += OnResume;
    }

    private void OnTextInput(char character)
    {
        if (!_canType) return;

        KeyTypedEvent?.Invoke(character);
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
        
        Keyboard.current.onTextInput += OnTextInput;
        
        SetGameplay();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        
        Keyboard.current.onTextInput -= OnTextInput;
        
        playerControls.FindActionMap(playerActionMapName).Disable();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => EnablePlayerInput();
    private void OnActiveSceneChanged(Scene previousScene, Scene newScene) => EnablePlayerInput();


    public void SetGameplay()
    {
        playerControls.FindActionMap(playerActionMapName).Enable();
        playerControls.FindActionMap(uiActionMapName).Disable();
        DesactivateTyping();
    }

    public void SetUI()
    {
        playerControls.FindActionMap(playerActionMapName).Disable();
        playerControls.FindActionMap(uiActionMapName).Enable();
        DesactivateTyping();
    }

    public void SetCombat()
    {
        playerControls.FindActionMap(playerActionMapName).Disable();
        playerControls.FindActionMap(uiActionMapName).Disable();
        EnableTyping();
    }

    public void EnableTyping()
    {
        _canType = true;
    }

    public void DesactivateTyping()
    {
        _canType = false;
    }
}