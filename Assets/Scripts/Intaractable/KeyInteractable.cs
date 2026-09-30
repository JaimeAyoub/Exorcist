using UnityEngine;

public class KeyInteractable : Interactable
{
    [SerializeField] private GameObject Enemy;
    private EnemyHealthBase enemyDeath;

    public void Start()
    {
        gameObject.SetActive(false);
        if (Enemy)
        {
            enemyDeath = Enemy.GetComponent<EnemyHealthBase>();
            enemyDeath.OnEnemyDeath += enableKey;
        }
    }

    public override void Interact()
    {
        GameManager.Instance.addKey();
        Debug.Log("Llave add");
        Destroy(gameObject);
    }

    private void enableKey()
    {
        gameObject.SetActive(true);
        enemyDeath.OnEnemyDeath -= enableKey;
    }
}