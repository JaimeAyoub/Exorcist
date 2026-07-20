using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using UnityUtils;

public class CombatManager : Singleton<CombatManager>
{
    public PlayerInputHandler inputHandler;
    public LetterSpawner letterSpawner;
    public CanvasGroup combatgroup;
    public GameObject player;
    public GameObject enemy;
    public bool isCombat = false;
    private bool _isPlayerAlive = true;

    //Variables para la logica del tiempo
    public float currentTime;
    public float MaxTime = 20;
    public Slider _timeSlider;

    private CinemachineHardLookAt cam;

    public GameObject playerSpawner;
    public GameObject enemySpawner;
    private static Vector3 toPlayerSpawn;
    private static Vector3 toEnemySpanwe;

    public Image imageToFade;

    public Vector3 _currentPositionPlayer;
    private Quaternion _currentRotationPlayer;
    private bool isTransitioning;

    private float _currentAberration;
    public GameObject bookSprite;
    public GameObject book;
    public GameObject candle;
    public GameObject healthCandle;
    public Image DamageVignette;
    public GameObject CameraHolder;

    public PlayableDirector sequenceCombat;
    public CanvasGroup sequence;
    public SoundData TriggerSound;
    public float timeToSubstract;

    //Cosas para el nuevo combate

    private Vector2 MouseDelta;
    private float baseRotationXCamera;
    private float timeForChangeLook;
    private bool canChangeLook = true;
    public bool isLookingAtBook;
    public CinemachineCamera camera;
    public InputAction LookBooKAction;

    public SoundData BGMMusic;

    void Start()
    {
        LookBooKAction = InputSystem.actions.FindAction("LookBook");
        bookSprite.SetActive(false);
        healthCandle.SetActive(false);
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
        _timeSlider.value = currentTime;

        isLookingAtBook = LookBooKAction.IsPressed();
        if (isLookingAtBook)
        {
            LookAtBook();
            ApproachToPlayer();
        }
        else
        {
            LookAtEnemy();
        }
    }

    private enum Combatturn
    {
        PlayerTurn,
        EnemyTurn,
        None
    }


    private Combatturn _currentturn;

    public void StartCombat()
    {
        if (isCombat || isTransitioning) return;
        isTransitioning = true;

        SoundManager.Instance.CreateSound().WithSoundData(TriggerSound).Play();
        inputHandler.EnableTyping();
    }


    public void StartCombatRoutine()
    {
        enemy = player.GetComponentInChildren<PlayerCollision>().collisionEnemy;
        if (enemy == null)
        {
            Debug.LogWarning("Enemy not found, teleport skipped.");
            isTransitioning = false;
            return;
        }


        player.GetComponentInChildren<PlayerAttack>().target = enemy;


        SetUpCombat();

        isTransitioning = false;
    }


    private IEnumerator CombatLoop()
    {
        while (isCombat)
        {
            if (_currentturn == Combatturn.PlayerTurn)
            {
                inputHandler.EnableTyping();

                if (IsCombatEnd()) yield break;

                yield return new WaitUntil(() => currentTime <= 0);
                _currentturn = Combatturn.EnemyTurn;
            }
            else if (_currentturn == Combatturn.EnemyTurn)
            {
                inputHandler.DesactivateTyping();

                if (enemy != null)
                    enemy.GetComponent<EnemyAttack>().Attack(1);
                Debug.Log("Enemigo hace damage");

                if (IsCombatEnd()) yield break;

                ResetTime();
                _currentturn = Combatturn.PlayerTurn;
            }
        }
    }

    public void EndCombat()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        StartCoroutine(EndCombatRoutine());
    }

    private IEnumerator EndCombatRoutine()
    {
        if (OptionsScript.Instance.volumeProfile.TryGet(out OptionsScript.Instance._chromaticAberration))
        {
            OptionsScript.Instance._chromaticAberration.intensity.value = _currentAberration;
        }

        imageToFade.DOFade(1f, 0.5f).SetUpdate(true);
        yield return new WaitForSecondsRealtime(0.5f);

        isCombat = false;
        Destroy(enemy);
        inputHandler.SetGameplay();
        inputHandler.KeyTypedEvent -= letterSpawner.UpdateScreenText;

        TeleportPlayer(_currentPositionPlayer);
        Debug.Log("PlayerRegresado");
        player.transform.rotation = _currentRotationPlayer;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = true;

        UIManager.Instance.CheckEnd();
        _currentturn = Combatturn.None;
        Cursor.visible = false;
        healthCandle.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        bookSprite.SetActive(false);
        book.SetActive(true);
        candle.SetActive(true);
        ResetTime();
        letterSpawner.EmptyAll();

        _currentPositionPlayer = Vector3.zero;
        OptionsScript.Instance.PixelationShaderMaterial.SetFloat("_PixelSize", 4.0f);

        if (_isPlayerAlive)
        {
            imageToFade.DOFade(0f, 0.5f).SetUpdate(true);
            yield return new WaitForSecondsRealtime(0.5f);
            isTransitioning = false;
        }
        else
        {
            inputHandler.SetUI();
            ChangeScene sceneChange = FindFirstObjectByType<ChangeScene>();
            if (sceneChange)
            {
                sceneChange.SelectSceneT(2);
            }
            else
            {
                Debug.Log("No hay SceneChange en la escena weon");
            }
        }
    }

    public bool IsCombatEnd()
    {
        if (player.GetComponentInChildren<PlayerHealth>().currentHealth <= 0)
        {
            Debug.Log("Derrota");
            //AudioManager.instance.StopSFX();
            _isPlayerAlive = false;
            EndCombat();
            return true;
        }

        if (enemy.GetComponent<EnemyHealthBase>().currentHealth <= 0)
        {
            Debug.Log("Victoria");
            EndCombat();
            //AudioManager.instance.StopSFX();
            return true;
        }

        return false;
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
        timeToSubstract = 0;
    }

    public void SetTimeToSubstract(float time)
    {
        timeToSubstract = time;
    }

    private void TeleportPlayer(Vector3 playerToTeleport)
    {
        if (player == null)
        {
            Debug.LogWarning("No player found");
            return;
        }

        player.transform.rotation = Quaternion.Euler(0, 0, 0);
        player.transform.position = playerToTeleport;
    }

    private void TeleportEnemy(Vector3 enemyPosTeleport)
    {
        if (enemy != null)
        {
            enemy.transform.DOKill();
            enemy.transform.position = enemyPosTeleport;
            Debug.Log("Enemigo tepeado");
        }
        else
            Debug.LogWarning("Enemy not found");
    }

    public void SetUpCombat()
    {
        isCombat = true;
        SoundManager.Instance.CreateSound().WithSoundData(BGMMusic).Play();

        if (player == null) Debug.LogError("¡PLAYER es null!");
        if (enemy == null) Debug.LogError("¡ENEMY es null!");
        if (OptionsScript.Instance == null) Debug.LogError("¡OptionsScript.Instance es null!");
        else if (OptionsScript.Instance.PixelationShaderMaterial == null)
            Debug.LogError("¡PixelationShaderMaterial es null!");
        if (inputHandler == null) Debug.LogError("¡inputHandler es null!");
        if (CameraHolder == null) Debug.LogError("¡CameraHolder es null!");
        if (letterSpawner == null) Debug.LogError("¡letterSpawner es null!");
        if (UIManager.Instance == null) Debug.LogError("¡UIManager.Instance es null!");
        if (bookSprite == null) Debug.LogError("¡bookSprite es null!");

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;

        _currentPositionPlayer = player.transform.position;
        _currentRotationPlayer = player.transform.rotation;

        if (OptionsScript.Instance.volumeProfile.TryGet(out OptionsScript.Instance._chromaticAberration))
        {
            _currentAberration = OptionsScript.Instance._chromaticAberration.intensity.value;
            OptionsScript.Instance._chromaticAberration.intensity.value = 0;
        }

        OptionsScript.Instance.PixelationShaderMaterial.SetFloat("_PixelSize", 0.1f);
        //AudioManager.instance.PlayBGM(SoundType.COMBATE, 1f);
        //AudioManager.instance.PlaySFX(SoundType.ENEMIGO, 0.3f);
        _currentturn = Combatturn.PlayerTurn;

        TeleportEnemy(toEnemySpanwe);
        TeleportPlayer(toPlayerSpawn);
        inputHandler.SetCombat();
        CameraHolder.transform.rotation = Quaternion.Euler(0, 0, 0);
        baseRotationXCamera = CameraHolder.transform.rotation.eulerAngles.x;
        player.transform.LookAt(enemy.transform.position);
        letterSpawner.EmptyAll();
        letterSpawner.FillCharQueue();
        UIManager.Instance.ActivateCanvas(UIManager.Instance._combatCanvas);
        inputHandler.KeyTypedEvent -= letterSpawner.UpdateScreenText;
        inputHandler.KeyTypedEvent += letterSpawner.UpdateScreenText;


        bookSprite.SetActive(false);
        book.SetActive(false);
        candle.SetActive(false);
        healthCandle.SetActive(true);

        Vector3 currentPosBook = bookSprite.transform.position;
        bookSprite.transform.position = new Vector3(currentPosBook.x, currentPosBook.y - 1.5f, currentPosBook.z);
        bookSprite.transform.DOMove(currentPosBook, 0.5f).SetUpdate(true);
    }

    private void LookAtBook()
    {
        StopAllCoroutines();
        StartCoroutine(CoolDownForChangeLook());
        if (CameraHolder != null)
        {
            CameraHolder.transform.DOKill();

            SoundManager.Instance.CreateSound().WithSoundData(BGMMusic).Play();
            Vector3 currentCameraRotation = CameraHolder.transform.rotation.eulerAngles;
            Vector3 newCameraRotation =
                new Vector3(baseRotationXCamera + 45.0f, currentCameraRotation.y, currentCameraRotation.z);
            CameraHolder.transform.DORotate(newCameraRotation, 0.3f);
        }
    }

    private void LookAtEnemy()
    {
        StopAllCoroutines();
        StartCoroutine(CoolDownForChangeLook());
        if (CameraHolder != null)
        {
            CameraHolder.transform.DOKill();
            Vector3 currentCameraRotation = CameraHolder.transform.rotation.eulerAngles;
            Vector3 newCameraRotation =
                new Vector3(baseRotationXCamera, currentCameraRotation.y, currentCameraRotation.z);

            CameraHolder.transform.DORotate(newCameraRotation, 0.3f);
        }
    }

    public void ApproachToPlayer()
    {
        if (player)
        {
            Vector3 direction = (player.transform.position - enemy.transform.position).normalized;

            Vector3 newDirection = new Vector3(direction.x, 0, direction.z);
            enemy.transform.position += newDirection * (2.0f * Time.deltaTime);

            Debug.Log(Vector3.Distance(enemy.transform.position, player.transform.position));
            if (Vector3.Distance(enemy.transform.position, player.transform.position) <= 2.5f)
            {
                enemy.GetComponent<EnemyAttack>().Attack(1);
                TeleportEnemy(toEnemySpanwe);
            }
        }
    }


    private IEnumerator CoolDownForChangeLook()
    {
        canChangeLook = false;
        yield return new WaitForSeconds(0.5f);
        canChangeLook = true;
    }
}