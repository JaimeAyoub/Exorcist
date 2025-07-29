using Unity.Cinemachine;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    public CinemachineVirtualCameraBase camera;

    [Header("Movement")] private float MoveX;
    private float MoveZ;
    private Vector3 movement;
    public float speed;


    void Start()
    {
        if (!controller)
            controller = GetComponent<CharacterController>();
        if (!camera)
            camera = FindAnyObjectByType<CinemachineVirtualCameraBase>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Update()
    {
        if (!CombatManager.instance.isCombat)
        {
            Move();

            Rotate();
        }
    }

    void Move()
    {
        MoveX = Input.GetAxisRaw("Horizontal");
        MoveZ = Input.GetAxisRaw("Vertical");
        movement = transform.right * MoveX + transform.forward * MoveZ;
        controller.Move(movement.normalized * (speed * Time.deltaTime));
    }

    void Rotate()
    {
        transform.rotation = Quaternion.Euler(0f, camera.GetComponent<CinemachinePanTilt>().PanAxis.Value, 0f);
    }
}