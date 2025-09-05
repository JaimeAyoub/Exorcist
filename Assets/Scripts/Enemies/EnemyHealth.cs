using DG.Tweening;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;

    private Tween damageTween;

    void Start()
    {
        currentHealth = maxHealth;
    }

  
    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        Debug.Log("Vida del enemigo: " + currentHealth);

        DamageFlash();

        if (currentHealth <= 0)
            Death();
        else
            CombatManager.instance.IsCombatEnd();
    }

    private void Death()
    {
        if (damageTween != null && damageTween.IsActive())
            damageTween.Kill();

        transform.DOScale(Vector3.zero, 0.5f)
            .OnComplete(() => CombatManager.instance.EndCombat());
        ;
    }

    void DamageFlash()
    {
        SpriteRenderer enemysp = GetComponentInChildren<SpriteRenderer>();
        if (enemysp == null) return;

        if (damageTween != null && damageTween.IsActive())
            damageTween.Kill();

        damageTween = enemysp.DOColor(Color.red, 0.125f)
            .SetLoops(4, LoopType.Yoyo)
            .OnKill(() =>
            {
                if (enemysp != null) enemysp.color = Color.white;
            });
    }
}