using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Action Asset")] [SerializeField]
    private InputActionAsset playerControls;

    [Header("Action Map Name Reference")] [SerializeField]
    private string playerActionMapName = "Player";

    [SerializeField] private string uiActionMapName = "UI";
    [SerializeField] private string typingActionMapName = "Typing";
    [SerializeField] private string noteActionMapName = "Note";

    [Header("Player Action Name References")] [SerializeField]
    private string movement = "Movement";

    [SerializeField] private string rotation = "Rotation";
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string sprint = "Sprint";

    [Header("Type Action Name References")] [SerializeField]
    private string typing = "Type";

    [SerializeField] private string mayus = "Mayus";

    [Header("UI Action Name References")] [SerializeField]
    private string pause = "Pause";

    [SerializeField] private string resume = "Resume";

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
    public event Action PauseEvent;
    public static event Action MovementEvent;
    public static event Action StopMovementEvent;
    public event Action<char> KeyTypedEvent;

    // Typing exacto (case-sensitive) para combate por palabras.
    // Usa Keyboard.onTextInput: entrega el caracter real (mayúsculas,
    // minúsculas, espacios, comas, tildes, etc.) en vez del nombre del control.
    public event Action<char> ExactCharEvent;
    public event Action SubmitEvent;

    // UI Events
    public event Action ResumeEvent;

    public Vector2 MovementInput { get; private set; }
    public Vector2 RotationInput { get; private set; }
    public bool JumpTriggered { get; private set; }
    public bool SprintTriggered { get; private set; }
    public bool IsInMayus { get; private set; }

    private void EnablePlayerInput()
    {
        var playerMapReference = playerControls.FindActionMap(playerActionMapName);
        var uiMapReference = playerControls.FindActionMap(uiActionMapName);
        var typingMapReference = playerControls.FindActionMap(typingActionMapName);
        var noteMapReference = playerControls.FindActionMap(noteActionMapName);

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
        // Idempotente: cada LoadScene dispara sceneLoaded + activeSceneChanged,
        // y sin este guard los callbacks se duplicaban (pausa que se abre y
        // cierra sola) y quedaban handlers fantasma tras cambiar de escena.
        UnsubscribeActionValuesFromInputEvents();

        if (_movementAction != null)
        {
            _movementAction.performed += OnPlayerMove;
            _movementAction.canceled += OnStopPlayerMove;
        }

        if (_rotationAction != null)
        {
            _rotationAction.performed += OnRotationPerformed;
            _rotationAction.canceled += OnRotationCanceled;
        }

        if (_jumpAction != null)
        {
            _jumpAction.performed += OnJumpPerformed;
            _jumpAction.canceled += OnJumpCanceled;
        }

        if (_sprintAction != null)
        {
            _sprintAction.performed += OnSprintPerformed;
            _sprintAction.canceled += OnSprintCanceled;
        }

        if (_pauseAction != null)
            _pauseAction.performed += OnPause;
        if (_resumeAction != null)
            _resumeAction.performed += OnResume;

        if (_typingAction != null)
            _typingAction.performed += OnKeyTyped;
        if (_mayusAction != null)
        {
            _mayusAction.performed += OnMayusPerformed;
            _mayusAction.canceled += OnMayusCanceled;
        }
    }

    private void UnsubscribeActionValuesFromInputEvents()
    {
        if (_movementAction != null)
        {
            _movementAction.performed -= OnPlayerMove;
            _movementAction.canceled -= OnStopPlayerMove;
        }

        if (_rotationAction != null)
        {
            _rotationAction.performed -= OnRotationPerformed;
            _rotationAction.canceled -= OnRotationCanceled;
        }

        if (_jumpAction != null)
        {
            _jumpAction.performed -= OnJumpPerformed;
            _jumpAction.canceled -= OnJumpCanceled;
        }

        if (_sprintAction != null)
        {
            _sprintAction.performed -= OnSprintPerformed;
            _sprintAction.canceled -= OnSprintCanceled;
        }

        if (_pauseAction != null)
            _pauseAction.performed -= OnPause;
        if (_resumeAction != null)
            _resumeAction.performed -= OnResume;

        if (_typingAction != null)
            _typingAction.performed -= OnKeyTyped;
        if (_mayusAction != null)
        {
            _mayusAction.performed -= OnMayusPerformed;
            _mayusAction.canceled -= OnMayusCanceled;
        }
    }

    private void OnRotationPerformed(InputAction.CallbackContext inputInfo) =>
        RotationInput = inputInfo.ReadValue<Vector2>();

    private void OnRotationCanceled(InputAction.CallbackContext _) =>
        RotationInput = Vector2.zero;

    private void OnJumpPerformed(InputAction.CallbackContext _) => JumpTriggered = true;
    private void OnJumpCanceled(InputAction.CallbackContext _) => JumpTriggered = false;

    private void OnSprintPerformed(InputAction.CallbackContext _) => SprintTriggered = true;
    private void OnSprintCanceled(InputAction.CallbackContext _) => SprintTriggered = false;

    private void OnMayusPerformed(InputAction.CallbackContext _) => IsInMayus = true;
    private void OnMayusCanceled(InputAction.CallbackContext _) => IsInMayus = false;

    private void OnKeyTyped(InputAction.CallbackContext ctx)
    {
//        Debug.Log(ctx.control.name);
        var endChar = ctx.control.name;
        //      Debug.Log(endChar);
        if (endChar == "space")
        {
            KeyTypedEvent?.Invoke(' ');
            Debug.Log(endChar);
        }

        KeyTypedEvent?.Invoke(endChar[0]);
       // Debug.Log(endChar);
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
        if (Keyboard.current != null)
            Keyboard.current.onTextInput += HandleTextInput;
        // Bindear aquí también: la escena inicial no dispara sceneLoaded,
        // así que sin esto la primera escena nacía sin callbacks.
        EnablePlayerInput();
        SetGameplay();
    }


    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= HandleTextInput;
        // Sin esto, el handler destruido seguía recibiendo input del asset
        // compartido y apagaba el mapa Player en la escena nueva.
        UnsubscribeActionValuesFromInputEvents();
        playerControls.FindActionMap(playerActionMapName).Disable();
    }

    private void Update()
    {
        // Enter por polling: no existe como binding de texto
        // y onTextInput no lo reporta de forma fiable en todas las plataformas.
        // Solo cuando el mapa Typing está activo (mirar al libro lo desactiva).
        if (_typingAction == null || !_typingAction.enabled) return;
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
            SubmitEvent?.Invoke();
    }

    /// <summary>
    /// Caracter exacto pulsado (respeta mayúsculas, espacios y puntuación).
    /// Los controles (Enter, etc.) se ignoran aquí: van por Update.
    /// </summary>
    private void HandleTextInput(char c)
    {
        if (c == '\n' || c == '\r' || c == '\b' || char.IsControl(c)) return;
        if (_typingAction == null || !_typingAction.enabled) return;
        ExactCharEvent?.Invoke(c);
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
        playerControls.FindActionMap(typingActionMapName).Enable();
        playerControls.FindActionMap(uiActionMapName).Disable();
        playerControls.FindActionMap(noteActionMapName).Disable();
        
    }

    public void SetUI()
    {
        playerControls.FindActionMap(playerActionMapName).Disable();
        playerControls.FindActionMap(typingActionMapName).Disable();
        playerControls.FindActionMap(noteActionMapName).Disable();
        playerControls.FindActionMap(uiActionMapName).Enable();
    }

    public void SetCombat()
    {
        playerControls.FindActionMap(typingActionMapName).Enable();
        playerControls.FindActionMap(playerActionMapName).Disable();
        playerControls.FindActionMap(uiActionMapName).Disable();
        playerControls.FindActionMap(noteActionMapName).Disable();
    }

    public void DesactivateTyping()
    {
        playerControls.FindActionMap(typingActionMapName).Disable();
    }

    public void EnableTyping()
    {
        playerControls.FindActionMap(typingActionMapName).Enable();
    }

    public void SetNote()
    {
        playerControls.FindActionMap(noteActionMapName).Enable();
        playerControls.FindActionMap(typingActionMapName).Disable();
        playerControls.FindActionMap(playerActionMapName).Disable();
        playerControls.FindActionMap(uiActionMapName).Disable();
        
    }

}
