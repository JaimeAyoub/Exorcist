using System;
using UnityEngine;
using DG.Tweening;

public abstract class EnemyHealthBase : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;

    private Tween damageTween;

    public SoundData damageSound;

    [Header("Erosion Effect (al morir)")]
    [Tooltip("Nombre de la propiedad de erosión en el shader")]
    public string erosionProperty = "_ErosionAmount";
    public float erosionDuration = 1.5f;

    private Material erosionMaterial;
    private bool isDead = false; // Evita que Death() se ejecute más de una vez

    private void Start()
    {
        currentHealth = maxHealth;

        SpriteRenderer sp = GetComponentInChildren<SpriteRenderer>();
        if (sp != null)
        {
            // .material (no .sharedMaterial) instancia el material,
            // así el efecto de erosión no afecta a otros enemigos que compartan el mismo asset.
            erosionMaterial = sp.material;
            erosionMaterial.SetFloat(erosionProperty, 0f);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0 || isDead) return;

        currentHealth -= damageAmount;
        Debug.Log("Vida del enemigo: " + currentHealth);

        DamageFlash();
        PlayDamageSound();
        if (currentHealth <= 0)
            Death();
        else
            CombatManager.Instance.IsCombatEnd();
    }

    private void PlayDamageSound()
    {
        SoundManager.Instance.CreateSound().WithSoundData(damageSound).Play();
    }

    private void Death()
    {
        if (isDead) return; // Protección extra: no dispares Death() dos veces
        isDead = true;

        UIManager.Instance.CheckEnd();

        // Matar cualquier tween de daño en curso (flash rojo / shake) para que no interfiera
        if (damageTween != null && damageTween.IsActive())
            damageTween.Kill();

        if (erosionMaterial == null)
        {
            // Fallback por si el SpriteRenderer no se encontró en Start()
            CombatManager.Instance.EndCombat();
            return;
        }

        erosionMaterial.DOFloat(1f, erosionProperty, erosionDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => CombatManager.Instance.EndCombat());
    }

    private void DamageFlash()
    {
        SpriteRenderer enemysp = GetComponentInChildren<SpriteRenderer>();
        if (enemysp == null) return;

        if (damageTween != null && damageTween.IsActive())
            damageTween.Kill();
        damageTween = DOTween.Sequence()
            .Join(enemysp.DOColor(Color.red, 0.125f)
                .SetLoops(4, LoopType.Yoyo))
            .Join(transform.DOShakePosition(0.5f, 0.5f))
            .OnKill(() =>
            {
                if (enemysp != null)
                    enemysp.color = Color.white;
            });
    }
}