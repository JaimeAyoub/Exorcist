using System;
using UnityEngine;
using DG.Tweening;

public abstract class EnemyHealthBase : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;

    private Tween damageTween;

    public SoundData damageSound;

    private Material erotionMaterial;

    private void Start()
    {
        currentHealth = maxHealth;
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            erotionMaterial = sr.material;
        else
            Debug.LogWarning($"[Enemigo] '{gameObject.name}' sin SpriteRenderer hijo: sin animación de erosión.");
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

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
        if (damageSound == null) return;
        SoundManager.Instance.CreateSound().WithSoundData(damageSound).Play();
    }

    private void Death()
    {
        Debug.Log($"[Enemigo] Muerte de '{gameObject.name}'. Terminando combate...");
        UIManager.Instance.CheckEnd();
        if (damageTween != null && damageTween.IsActive())
            damageTween.Kill();

        // Fail-safe: si no hay material de erosión, terminar el combate igual.
        if (erotionMaterial == null)
        {
            Debug.LogWarning("[Enemigo] Sin material de erosión: fin de combate directo.");
            CombatManager.Instance.EndCombat();
            return;
        }

        erotionMaterial.DOFloat(-0.2f, "_ErotionValue", 1.5f).OnComplete(() => CombatManager.Instance.EndCombat());
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