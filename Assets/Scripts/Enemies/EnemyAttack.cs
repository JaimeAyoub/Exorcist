using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyAttack : MonoBehaviour
{
    public GameObject player;
    public bool isInCombat;



    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    public void Attack(int amount)
    {
        if (player)
            player.GetComponent<PlayerHealth>().TakeDamage(amount);
        else
        {
            Debug.LogWarning("No player");
        }
    }


    
}