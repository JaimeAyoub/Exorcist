using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
   public GameObject player;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   public void Attack(int amount)
    {
        player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlayerHealth>().TakeDamage(amount);
        
    }
}
