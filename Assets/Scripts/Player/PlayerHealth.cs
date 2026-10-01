using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;
    public VolumeProfile _volumeProfile;
    public Vignette vignette;
    public float vignetteIntensity;
    public float tweenDuration;

    public float intensityCameraShake;
    public float durationCameraShake;

    public Sprite[] candleHealthSprites;
    public SpriteRenderer candleHealthSpriteRenderer;

    [Header("Sonidos")] public SoundData DamageSound;
    private readonly float _defaultVignetteIntensity = 0.25f;
    private Color _defaultVignetteColor;

    private void Start()
    {
        // Valor de prueba para testear el audio de combate (latidos/lowpass).
        maxHealth = 3;
        currentHealth = maxHealth;
        if (_volumeProfile.TryGet(out vignette))
        {
            vignette.intensity.value = _defaultVignetteIntensity;
            _defaultVignetteColor = vignette.color.value;
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public event Action OnPlayerDeath;

    /// <summary>
    ///     Se invoca cada vez que la vida cambia: (vidaActual, vidaMaxima).
    ///     Lo usa CombatAudioController para cambiar latidos y lowpass.
    /// </summary>
    public event Action<int, int> OnHealthChanged;

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        CameraShake.Instance.CmrShake(intensityCameraShake, durationCameraShake);
        SoundManager.Instance.CreateSound().WithSoundData(DamageSound).Play();
        //  AudioManager.instance.PlaySFX(SoundType.PlayerDamage, 0.5f);
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

        ChangeSprite();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0)
            Death();
    }

    private void Death()
    {
        Debug.Log("Muerte plyaer");
        OnPlayerDeath?.Invoke();
    }

    private void ChangeSprite()
    {
        candleHealthSpriteRenderer.sprite =
            currentHealth >= 0 ? candleHealthSprites[currentHealth] : candleHealthSprites[0];
    }
}