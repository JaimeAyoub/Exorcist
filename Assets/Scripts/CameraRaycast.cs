using Unity.Cinemachine;
using UnityEngine;

public class CameraRaycast : MonoBehaviour
{
    public CinemachineVirtualCameraBase virtualCamera; 
    void Start()
    {
        
    }

    
    void Update()
    {
        DrawRayCast();
    }

    void DrawRayCast()
    {
        RaycastHit hit;
        Debug.DrawRay(virtualCamera.transform.position, virtualCamera.transform.forward , Color.red);
    }
}
