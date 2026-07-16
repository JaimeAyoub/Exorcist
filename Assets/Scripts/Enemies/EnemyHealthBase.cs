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
        erotionMaterial = GetComponentInChildren<SpriteRenderer>().material;
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
        SoundManager.Instance.CreateSound().WithSoundData(damageSound).Play();
    }

    private void Death()
    {
        UIManager.Instance.CheckEnd();
        if (damageTween != null && damageTween.IsActive())
            damageTween.Kill();

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