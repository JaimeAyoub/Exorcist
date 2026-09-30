using System;
using DG.Tweening;
using UnityEngine;

public abstract class EnemyHealthBase : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;

    [SerializeField] private bool shouldDrop;

    [Tooltip("Sonido al perder vida (p.ej. BaseEnemy.mp3 en el enemigo base). Suena en cada golpe.")]
    public SoundData damageSound;

    [Header("Erosion Effect (al morir)")] [Tooltip("Nombre de la propiedad de erosión en el shader")]
    public string erosionProperty = "_ErosionAmount";

    public float erosionDuration = 1.5f;

    private Tween damageTween;

    private Material erosionMaterial;
    private bool isDead; // Evita que Death() se ejecute más de una vez

    private void Start()
    {
        currentHealth = maxHealth;

        var sp = GetComponentInChildren<SpriteRenderer>();
        if (sp != null)
        {
            // .material (no .sharedMaterial) instancia el material,
            // así el efecto de erosión no afecta a otros enemigos que compartan el mismo asset.
            erosionMaterial = sp.material;
            erosionMaterial.SetFloat(erosionProperty, 0f);
        }
    }

    /// <summary>
    ///     Se dispara cuando el enemigo muere, justo al terminar el efecto de erosión.
    ///     Quien quiera reaccionar a la muerte (CombatManager, un sistema de loot, etc.)
    ///     se suscribe a este evento en vez de que este script conozca esas dependencias.
    /// </summary>
    public event Action OnEnemyDeath;

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0 || isDead) return;

        currentHealth -= damageAmount;
        Debug.Log("Vida del enemigo: " + currentHealth);

        DamageFlash();
        PlayDamageSound();
        if (currentHealth <= 0)
            Death();
    }

    private void PlayDamageSound()
    {
        // Cada vez que pierde vida suena (Enemigo Base, GulaHit, etc. según prefab).
        if (damageSound == null || damageSound.clip == null) return;
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.CreateSound().WithSoundData(damageSound).WithRandomPitch().Play();
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
            OnEnemyDeath?.Invoke();
            return;
        }

        erosionMaterial.DOFloat(1f, erosionProperty, erosionDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => OnEnemyDeath?.Invoke());
    }

    private void DamageFlash()
    {
        var enemysp = GetComponentInChildren<SpriteRenderer>();
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