using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCollision : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject collisionEnemy;
    void Start()
    {
        AudioManager.instance.PlayBGM(SoundType.FONDO, 1f);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && !CombatManager.instance.isCombat)
        {
            AudioManager.instance.StopSFX();
            collisionEnemy =  other.gameObject;
            CombatManager.instance.StartCombat();
        }
    }
}