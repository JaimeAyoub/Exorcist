using System;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class CameraRaycast : UnityUtils.Singleton<CameraRaycast>
{
    public CinemachineVirtualCameraBase virtualCamera;
    LayerMask layerMask;

    public bool canOpen = false;

    void Awake()
    {
        layerMask = LayerMask.GetMask("Door", "Player");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
           TryInteract();
        }
    }
    

    private void TryInteract()
    {
        RaycastHit hit;
        if (Physics.Raycast(virtualCamera.transform.position, virtualCamera.transform.forward, out hit, 2.5f,
                layerMask))
        {
            DoorScript door = hit.collider.GetComponent<DoorScript>();
            if (door != null)
            {
                door.ToggleDoor();
            }
        }
    }
}