using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : UnityUtils.Singleton<CameraShake>
{
    private CinemachineCamera _cinemachineCamera;
    private CinemachineBasicMultiChannelPerlin _noise;
    private float shakeTime;

    private void Awake()
    {
        _cinemachineCamera = this.gameObject.GetComponent<CinemachineCamera>();
        if (_cinemachineCamera != null)
            _noise = _cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        else
        {
            Debug.LogError("Cinemachine Camera not found");
        }
    }

    void Update()
    {
        if (shakeTime > 0)
        {
            shakeTime -= Time.deltaTime;
            if (shakeTime <= 0 && _noise != null)
                _noise.AmplitudeGain = 0f;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            CameraShakeTest();
        }
    }

    public void CmrShake(float intensity, float time)
    {
        if (_noise == null) return;

        _noise.AmplitudeGain = intensity;
        shakeTime = time;
    }

    public void CameraShakeTest()
    {
        CmrShake(2.5f,0.1f);
    }



}
