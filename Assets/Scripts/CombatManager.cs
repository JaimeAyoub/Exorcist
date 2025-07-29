using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class CombatManager : MonoBehaviour
{
    public static CombatManager instance;

    public CanvasGroup combatgroup;
    public GameObject player;
    public GameObject enemy;
    public bool isplayerTurn = true;
    public bool isenemyTurn;
    public bool isCombat = false;

    void Start()
    {
    }


    void Update()
    {
    }

    public enum combatturn
    {
        playerTurn,
        enemyTurn,
        none
    }

    private combatturn currentturn;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartCombat()
    {
        if (isCombat)
            return;
        combatgroup.alpha = 1;
        player.GetComponent<PlayerAttack>().target = enemy;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        currentturn = combatturn.playerTurn;
        isCombat = true;
        StopAllCoroutines();
        StartCoroutine(CombatLoop());
    }

    public IEnumerator CombatLoop()
    {
        Debug.unityLogger.Log("CombatStart");
        while (isCombat)
        {
            if (currentturn == combatturn.playerTurn)
            {
                combatgroup.interactable = true;

                //Accion del jugador.
                yield return new WaitUntil(() => isplayerTurn ==  false);
                currentturn = combatturn.enemyTurn;
            }
            else if (currentturn == combatturn.enemyTurn)
            {
                enemy.GetComponent<EnemyAttack>().Attack(1);

                if (isCombatEnd()) yield break;
                
                isplayerTurn = true;
                currentturn = combatturn.playerTurn;
            }
        }

        yield return null;
    }

    private void endCombat()
    {
        isCombat = false;
        combatgroup.alpha = 0;
        combatgroup.interactable = false;

        currentturn = combatturn.none;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    private bool isCombatEnd()
    {
        if (player.GetComponent<PlayerHealth>().currentHealth <= 0 ||
            enemy.GetComponent<EnemyHealth>().currentHealth <= 0)
        {
            endCombat();
            return true;
        }

        return false;
    }

    public void EndPlayerTurn()
    {
        isplayerTurn = false;
    }
}