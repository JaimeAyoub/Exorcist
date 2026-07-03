using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCollision : MonoBehaviour
{
    public GameObject collisionEnemy;


    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && !CombatManager.Instance.isCombat)
        {
            
            collisionEnemy =  other.gameObject;
            CombatManager.Instance.StartCombat();
        }
    }
}