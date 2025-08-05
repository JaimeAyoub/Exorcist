using Unity.Cinemachine;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    public CinemachineVirtualCameraBase camera;

    [Header("Movement")] private float _moveX;
    private float _moveZ;
    private Vector3 _movement;
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
        _moveX = Input.GetAxisRaw("Horizontal");
        _moveZ = Input.GetAxisRaw("Vertical");
        _movement = transform.right * _moveX + transform.forward * _moveZ;
        controller.Move(_movement.normalized * (speed * Time.deltaTime));
    }

    void Rotate()
    {
        transform.rotation = Quaternion.Euler(0f, camera.GetComponent<CinemachinePanTilt>().PanAxis.Value, 0f);
    }
}