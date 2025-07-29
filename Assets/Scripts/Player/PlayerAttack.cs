using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject target;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Attack(int amout)
    {
        target.GetComponent<EnemyHealth>().TakeDamage(amout);
     
    }
    
}
