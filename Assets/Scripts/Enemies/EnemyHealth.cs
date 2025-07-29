using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;  
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth > 0)
        {
            currentHealth -= amount;
            Debug.Log("Vida del enemigo: " + currentHealth);
        }
        // else if (currentHealth <= 0)
             //Death();
    }

    private void Death()
    {
        throw new System.NotImplementedException();
    }
}
