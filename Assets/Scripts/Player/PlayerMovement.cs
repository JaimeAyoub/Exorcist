using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    public CinemachineVirtualCameraBase camera;
    private InputSystem_Actions inputActions;
    private Rigidbody rb;

    [Header("Movement")] private float _moveX;
    private float _moveZ;
    private Vector3 _movement;
    public float speed = 5f;
    private bool isMoving;
    private float X;
    private float Z;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();
        inputActions.Player.Movement.performed += Move;
    }

    void Start()
    {
        if (!controller)
            controller = GetComponent<CharacterController>();
        if (!camera)
            camera = FindAnyObjectByType<CinemachineVirtualCameraBase>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    void FixedUpdate()
    {
        if (!CombatManager.instance.isCombat)
        {
            //Move();

            Rotate();
        }
        Vector2 inputVector = inputActions.Player.Movement.ReadValue<Vector2>();
        controller.Move(speed * Time.deltaTime * new Vector3(inputVector.x, 0, inputVector.y) );
    }

    void Move(InputAction.CallbackContext ctx)
    {
        //Vector2 inputVector = ctx.ReadValue<Vector2>();
        //controller.Move(speed * Time.deltaTime * new Vector3(inputVector.x, 0, inputVector.y));

        if (X == 0 && Z == 0)
        {
            isMoving = false;
        }
        else
        {
            isMoving = true;
        }
    }

    void Rotate()
    {
        transform.rotation = Quaternion.Euler(0f, camera.GetComponent<CinemachinePanTilt>().PanAxis.Value, 0f);
    }
    public bool IsMove()
    {
        return isMoving;
    }
    
}