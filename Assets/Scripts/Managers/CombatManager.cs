using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    public static CombatManager instance;
    public AudioManager audioManager;

    public CanvasGroup combatgroup;
    public GameObject player;
    public GameObject enemy;
    public bool isplayerTurn = true;
    public bool isenemyTurn;
    public bool isCombat = false;

    //Variables para la logica del tiempo
    public float currentTime;
    public float MaxTime = 20;
    public Slider _timeSlider;

    void Start()
    {
        currentTime = MaxTime;
        _timeSlider.maxValue = MaxTime;
    }


    void Update()
    {
        if (isCombat)
        {
            currentTime -= Time.timeScale * Time.deltaTime * 2;
            // Debug.Log(combatTime);
            _timeSlider.value = currentTime;
        }
    }

    public enum Combatturn
    {
        PlayerTurn,
        EnemyTurn,
        None
    }

    private Combatturn _currentturn;

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
        UIManager.instance.ActivateCanvas(UIManager.instance._combatCanvas);
        player.GetComponent<PlayerAttack>().target = enemy;
        AudioManager.instance.PlayBGM(SoundType.COMBATE, 0.5f);
        AudioManager.instance.PlaySFX(SoundType.ENEMIGO, 0.5f);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _currentturn = Combatturn.PlayerTurn;
        isCombat = true;
        TextReader.instance.ActivarModoEscritura();
        StopAllCoroutines();
        StartCoroutine(CombatLoop());
    }

    public IEnumerator CombatLoop()
    {
        Debug.unityLogger.Log("CombatStart");
        while (isCombat)
        {
            if (_currentturn == Combatturn.PlayerTurn)
            {
                combatgroup.interactable = true;


                //Accion del jugador.
                if (IsCombatEnd()) yield break;
                yield return new WaitUntil(() => currentTime <= 0);
                isplayerTurn = false;
                _currentturn = Combatturn.EnemyTurn;
            }
            else if (_currentturn == Combatturn.EnemyTurn)
            {
                enemy.GetComponent<EnemyAttack>().Attack(1);
                if (IsCombatEnd()) yield break;


                isplayerTurn = true;
                ResetTime();
                _currentturn = Combatturn.PlayerTurn;
            }
        }

        yield return null;
    }

    private void EndCombat()
    {
        isCombat = false;
        UIManager.instance.ActivateCanvas(UIManager.instance._mainCanvas);
        _currentturn = Combatturn.None;
        TextReader.instance.DesactivarModoEscritura();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    public bool IsCombatEnd()
    {
        if (player.GetComponent<PlayerHealth>().currentHealth <= 0)
        {
            Debug.Log("Derrota");
            EndCombat();
            return true;
        }
        if (enemy.GetComponent<EnemyHealth>().currentHealth <= 0)
        {
            Debug.Log("Victoria");
            EndCombat();
            Destroy(enemy);
            return true;
        }

        return false;
    }

    public void EndPlayerTurn()
    {
        isplayerTurn = false;
    }

    public void AddTime(float time)
    {
        currentTime += time;
    }

    public void SubstracTime(float time)
    {
        currentTime -= time;
    }

    void ResetTime()
    {
        currentTime = MaxTime;
    }
}