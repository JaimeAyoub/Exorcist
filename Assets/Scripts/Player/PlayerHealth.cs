using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
   public int currentHealth;
    public int maxHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
    }

   public void TakeDamage(int damage)
    {
        if (currentHealth > 0)
        {
            currentHealth -= damage;
            Debug.Log("Vida del jugador: " + currentHealth);
        }
      
        else if (currentHealth <= 0)
            Death();
    }

    private void Death()
    {
        throw new System.NotImplementedException();
    }
}