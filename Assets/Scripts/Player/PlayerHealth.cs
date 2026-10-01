using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;

    [Header("Viñeta de daño (Image)")] public Image damageVignette;

    [Tooltip("Alpha con vida completa.")] public float minVignetteAlpha;

    [Tooltip("Alpha al borde de la muerte (1 de vida).")]
    public float maxVignetteAlpha = 0.6f;

    [Tooltip("Cuánto sube momentáneamente al recibir el golpe, por encima del nuevo nivel base.")]
    public float hitFlashBoost = 0.25f;

    public float tweenDuration;
    public Color vignetteColor = Color.red;

    public float intensityCameraShake;
    public float durationCameraShake;

    public Sprite[] candleHealthSprites;
    public SpriteRenderer candleHealthSpriteRenderer;

    [Header("Sonidos")] public SoundData DamageSound;

    private void Start()
    {
        maxHealth = 3;
        currentHealth = maxHealth;

        if (damageVignette != null)
        {
            var c = vignetteColor;
            c.a = 0f; // arranca invisible, fuera de combate
            damageVignette.color = c;
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public event Action OnPlayerDeath;
    public event Action<int, int> OnHealthChanged;

    private float GetBaseAlphaForHealth()
    {
        if (maxHealth <= 0) return minVignetteAlpha;
        var missingHealthRatio = 1f - Mathf.Clamp01((float)currentHealth / maxHealth);
        return Mathf.Lerp(minVignetteAlpha, maxVignetteAlpha, missingHealthRatio);
    }

    /// <summary>Llamar al ENTRAR en combate: deja la viñeta visible según la vida actual.</summary>
    public void EnterCombatVignette()
    {
        if (damageVignette == null) return;
        DOTween.Kill("VignetteTween");
        damageVignette.DOFade(GetBaseAlphaForHealth(), tweenDuration).SetId("VignetteTween");
    }

    /// <summary>Llamar al SALIR de combate: la viñeta se apaga del todo.</summary>
    public void ExitCombatVignette()
    {
        if (damageVignette == null) return;
        DOTween.Kill("VignetteTween");
        damageVignette.DOFade(0f, tweenDuration).SetId("VignetteTween");
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        CameraShake.Instance.CmrShake(intensityCameraShake, durationCameraShake);
        SoundManager.Instance.CreateSound().WithSoundData(DamageSound).Play();

        if (damageVignette != null)
        {
            DOTween.Kill("VignetteTween");

            var newBaseAlpha = GetBaseAlphaForHealth();
            var flashAlpha = Mathf.Min(newBaseAlpha + hitFlashBoost, 1f);

            damageVignette.DOFade(flashAlpha, tweenDuration)
                .SetId("VignetteTween")
                .OnComplete(() => { damageVignette.DOFade(newBaseAlpha, tweenDuration).SetId("VignetteTween"); });
        }

        ChangeSprite();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0)
            Death();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;

        if (damageVignette != null)
        {
            DOTween.Kill("VignetteTween");
            damageVignette.DOFade(0f, tweenDuration).SetId("VignetteTween");
        }

        ChangeSprite();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
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