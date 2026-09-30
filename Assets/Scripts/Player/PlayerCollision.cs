using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCollision : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject collisionEnemy;
    void Start()
    {
       // AudioManager.instance.PlayBGM(SoundType.FONDO, 1f);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsEnemy(other) && !CombatManager.Instance.isCombat)
        {
            AudioManager.instance.StopSFX();
            TimelinesManager.Instance.PlayTimeLine(TimelinesManager.Instance.StartCombatTimeline);
            collisionEnemy =  other.gameObject;
            collisionEnemy.GetComponent<EnemyAttack>().isInCombat = true;
            CombatManager.Instance.StartCombat();
        }
    }

    /// <summary>Enemigo normal (tag Enemy) o Gula (tag Gula / componente GulaHealth).</summary>
    private static bool IsEnemy(Collider other)
    {
        if (other == null) return false;
        if (other.CompareTag("Enemy")) return true;
        // Tag directo sin CompareTag para no exigir que exista en TagManager.
        if (other.gameObject.tag == "Gula") return true;
        if (other.GetComponent<GulaHealth>() != null) return true;
        if (other.GetComponentInChildren<GulaHealth>() != null) return true;
        if (other.gameObject.name.Contains("Gula")) return true;
        return false;
    }
}