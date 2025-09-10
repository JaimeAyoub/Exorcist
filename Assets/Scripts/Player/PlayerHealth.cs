using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;
    public VolumeProfile _volumeProfile;
    public UnityEngine.Rendering.Universal.Vignette vignette;
    public float vignetteIntensity;
    public float tweenDuration;
    private float _defaultVignetteIntensity = 0.25f;
    private Color _defaultVignetteColor;

    public float intensityCameraShake;
    public float durationCameraShake;


    void Start()
    {
        currentHealth = maxHealth;
        if (_volumeProfile.TryGet(out vignette))
        {
            vignette.intensity.value = _defaultVignetteIntensity;
            _defaultVignetteColor = vignette.color.value;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        CameraShake.Instance.CmrShake(intensityCameraShake, durationCameraShake);
        AudioManager.instance.PlaySFX(SoundType.PlayerDamage, 0.5f);
        DOTween.Kill("VignetteTween");
        DOTween.Kill("VignetteColorTween");


        DOTween.To(() => vignette.intensity.value,
                x => vignette.intensity.value = x,
                vignetteIntensity,
                tweenDuration)
            .SetId("VignetteTween")
            .OnComplete(() =>
            {
                DOTween.To(() => vignette.intensity.value,
                        x => vignette.intensity.value = x,
                        _defaultVignetteIntensity,
                        tweenDuration)
                    .SetId("VignetteTween");
            });

        DOTween.To(() => vignette.color.value,
                x => vignette.color.value = x,
                Color.red,
                tweenDuration)
            .SetId("VignetteColorTween")
            .OnComplete(() =>
            {
                DOTween.To(() => vignette.color.value,
                        x => vignette.color.value = x,
                        _defaultVignetteColor,
                        tweenDuration)
                    .SetId("VignetteColorTween");
            });

        if (currentHealth <= 0)
            Death();
    }

    private void Death()
    {
        throw new System.NotImplementedException();
    }
}