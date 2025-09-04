using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using UnityUtils;

public class CombatManager : MonoBehaviour
{
    public static CombatManager instance;
    public AudioManager audioManager;

    public PlayerInputHandler inputHandler;
    public LetterSpawner letterSpawner;
    public CanvasGroup combatgroup;
    public GameObject player;
    public GameObject enemy;
    private bool isplayerTurn = true;
    private bool isenemyTurn;
    public bool isCombat = false;

    //Variables para la logica del tiempo
    public float currentTime;
    public float MaxTime = 20;
    public Slider _timeSlider;


    public GameObject playerSpawner;
    public GameObject enemySpawner;
    private static Vector3 toPlayerSpawn;
    private static Vector3 toEnemySpanwe;

    public Image imageToFade;

    public GameObject _currentPositionPlayer;
    private bool isTransitioning;

    private float _currentAberration;
    public GameObject bookSprite;
    public GameObject book;
    public GameObject candle;
    public Image DamageVignette;


    void Start()
    {
        bookSprite.SetActive(false);
        currentTime = MaxTime;
        _timeSlider.maxValue = MaxTime;
        Color c = DamageVignette.color;
        c.a = 0f;
        DamageVignette.color = c;
       toPlayerSpawn = playerSpawner.transform.position;
       toEnemySpanwe = enemySpawner.transform.position;
    }


    void Update()
    {
        if (!isCombat) return;
        currentTime -= Time.timeScale * Time.deltaTime * 2;
        // Debug.Log(combatTime);
        _timeSlider.value = currentTime;
    }

    private enum Combatturn
    {
        PlayerTurn,
        EnemyTurn,
        None
    }

    private Combatturn _currentturn;

    private void Awake()
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
        if (isCombat || isTransitioning) return;
        isTransitioning = true;


        if (OptionsScript.Instance.volumeProfile.TryGet(out OptionsScript.Instance._chromaticAberration))
        {
            _currentAberration = OptionsScript.Instance._chromaticAberration.intensity.value;
            OptionsScript.Instance._chromaticAberration.intensity.value = 0;
        }

        Sequence seq = DOTween.Sequence().SetUpdate(true);

        _currentPositionPlayer.transform.position = player.transform.position;

        enemy = player.GetComponentInChildren<PlayerCollision>().collisionEnemy;
        enemy.transform.position = toEnemySpanwe;
        seq.Join(imageToFade.DOFade(1f, 0.5f));

        player.GetComponentInChildren<PlayerAttack>().target = enemy;
        seq.AppendCallback(() =>
        {
            player.transform.position = toPlayerSpawn;
            player.transform.rotation = playerSpawner.transform.rotation;

            enemy.transform.rotation = enemySpawner.transform.rotation;

            AudioManager.instance.PlayBGM(SoundType.COMBATE, 0.5f);
            AudioManager.instance.PlaySFX(SoundType.ENEMIGO, 0.5f);
            _currentturn = Combatturn.PlayerTurn;
            isCombat = true;

            letterSpawner.EmptyAll();
            letterSpawner.FillCharQueue();
            UIManager.Instance.ActivateCanvas(UIManager.Instance._combatCanvas);
            inputHandler.SetCombat();
            inputHandler.KeyTypedEvent -= letterSpawner.UpdateScreenText;
            inputHandler.KeyTypedEvent += letterSpawner.UpdateScreenText;
            StopAllCoroutines();
            bookSprite.SetActive(true);
            book.SetActive(false);
            candle.SetActive(false);

            StartCoroutine(CombatLoop());
        });

        seq.Append(imageToFade.DOFade(0f, 0.5f));

        seq.OnComplete(() => { isTransitioning = false; });
    }

    private IEnumerator CombatLoop()
    {
        Debug.unityLogger.Log("CombatStart");
        Debug.Log(_currentturn);
        Debug.Log(isCombat);
        while (isCombat)
        {
            if (_currentturn == Combatturn.PlayerTurn)
            {
                inputHandler.EnableTyping();
                Debug.Log("Turno player");
                if (IsCombatEnd()) yield break;


                yield return new WaitUntil(() => currentTime <= 0);
                isplayerTurn = false;
                _currentturn = Combatturn.EnemyTurn;
            }
            else if (_currentturn == Combatturn.EnemyTurn)
            {
                inputHandler.DesactivateTyping();
                DamageVignette.DOFade(1, 0.125f)
                    .SetLoops(2, LoopType.Yoyo);
                if (enemy != null)
                    enemy.GetComponent<EnemyAttack>().Attack(1);
                Debug.Log("Enemigo hace damage");
                if (IsCombatEnd()) yield break;


                isplayerTurn = true;
                ResetTime();
                _currentturn = Combatturn.PlayerTurn;
            }
        }

        yield return null;
    }

    public void EndCombat()
    {
        player.transform.position = _currentPositionPlayer.transform.position;
        Sequence seq = DOTween.Sequence().SetUpdate(true);
        if (OptionsScript.Instance.volumeProfile.TryGet(out OptionsScript.Instance._chromaticAberration))
        {
            OptionsScript.Instance._chromaticAberration.intensity.value = _currentAberration;
        }

        seq.Join(imageToFade.DOFade(1f, 0.5f));
        seq.AppendCallback(() =>
        {
            isCombat = false;
            Destroy(enemy);
            inputHandler.SetGameplay();
            inputHandler.KeyTypedEvent -= letterSpawner.UpdateScreenText;
            UIManager.Instance.ActivateCanvas(UIManager.Instance._mainCanvas);
            _currentturn = Combatturn.None;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            bookSprite.SetActive(false);
            book.SetActive(true);
            candle.SetActive(true);
            ResetTime();
            letterSpawner.EmptyAll();
        });
        seq.Append(imageToFade.DOFade(0f, 0.5f));

        seq.OnComplete(() => { isTransitioning = false; });
    }

    public bool IsCombatEnd()
    {
        if (player.GetComponentInChildren<PlayerHealth>().currentHealth <= 0)
        {
            Debug.Log("Derrota");
            EndCombat();
            return true;
        }

        if (enemy.GetComponent<EnemyHealth>().currentHealth <= 0)
        {
            Debug.Log("Victoria");
            EndCombat();
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
        if (currentTime <= MaxTime)
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