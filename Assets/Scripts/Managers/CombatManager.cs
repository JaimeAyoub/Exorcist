using System.Collections;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UI;
using UnityUtils;

public class CombatManager : Singleton<CombatManager>
{
    public PlayerInputHandler inputHandler;
    public LetterSpawner letterSpawner;
    public GameObject player;
    public GameObject enemy;
    public bool isCombat;


    public GameObject playerSpawner;
    public GameObject enemySpawner;

    public Image imageToFade;

    public Vector3 _currentPositionPlayer;
    public GameObject book;
    public GameObject candle;
    public Image DamageVignette;
    public GameObject CameraHolder;

    public PlayableDirector sequenceCombat;
    public CanvasGroup sequence;
    public SoundData TriggerSound;
    public bool isLookingAtBook;
    public CinemachineCamera camera;
    public InputAction LookBooKAction;

    // Legacy: la música de combate ahora la lleva CombatAudioController
    // (música + latidos + lowpass). Se mantiene el campo por compatibilidad.
    public SoundData BGMMusic;

    public PlayerHealth playerHealth;
    private bool _isPlayerAlive;


    public bool _isTakingDamage;

    [SerializeField] private float enemySpeedToApproach;

    private float _currentAberration;

    // Referencia cacheada al componente de salud del enemigo actual,
    // para poder desuscribirnos de su evento al terminar el combate.
    private EnemyHealthBase _currentEnemyHealth;
    private Quaternion _currentRotationPlayer;
    private bool _isPlayerAlive;

    //Cosas para el nuevo combate

    private float baseRotationXCamera;
    private bool canChangeLook = true;
    private bool isTransitioning;
    private float timeForChangeLook;

    private void Start()
    {
        LookBooKAction = InputSystem.actions.FindAction("LookBook");

        var c = DamageVignette.color;
        c.a = 0f;
        DamageVignette.color = c;
    }

    private void Update()
    {
        if (!isCombat) return;

        isLookingAtBook = LookBooKAction.IsPressed();
        if (isLookingAtBook && !_isTakingDamage)
        {
            LookAtBook();
            ApproachToPlayer();
        }
        else
        {
            LookAtEnemy();
        }
    }


    public void StartCombat()
    {
        if (isCombat || isTransitioning) return;
        isTransitioning = true;

        SoundManager.Instance.CreateSound().WithSoundData(TriggerSound).Play();
        // Empieza la transición: el ambiente se apaga con fade durante la animación.
        var combatAudio = CombatAudioController.TryGetInstance();
        if (combatAudio != null)
            combatAudio.BeginCombatTransition();
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

        // Nos suscribimos al evento de muerte del enemigo en vez de que
        // el propio enemigo llame directamente a CombatManager.
        _currentEnemyHealth = enemy.GetComponentInChildren<EnemyHealthBase>();
        if (_currentEnemyHealth != null)
            _currentEnemyHealth.OnEnemyDeath += EnemyDeath;
        else
            Debug.LogWarning("El enemigo no tiene un EnemyHealthBase; EnemyDeath no se disparará.");

        SetUpCombat();
        isTransitioning = false;
    }


    public void EndCombat()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        StartCoroutine(EndCombatRoutine());
    }

    private IEnumerator EndCombatRoutine()
    {
        CameraHolder.transform.DOKill();
        if (OptionsScript.Instance.volumeProfile.TryGet(out OptionsScript.Instance._chromaticAberration))
            OptionsScript.Instance._chromaticAberration.intensity.value = _currentAberration;

        imageToFade.DOFade(1f, 0.5f).SetUpdate(true);
        yield return new WaitForSecondsRealtime(0.5f);

        isCombat = false;

        // Corta música de combate y latidos.
        var combatAudio = CombatAudioController.TryGetInstance();
        if (combatAudio != null)
            combatAudio.StopCombatAudio();

        // Desuscribirse antes de destruir el enemigo (buena práctica, evita
        // que el evento quede "colgando" si algo más lo referenciara).
        if (_currentEnemyHealth != null)
        {
            _currentEnemyHealth.OnEnemyDeath -= EnemyDeath;
            _currentEnemyHealth = null;
        }

        Destroy(enemy);
        inputHandler.SetGameplay();
        inputHandler.ExactCharEvent -= letterSpawner.HandleTypedChar;
        inputHandler.SubmitEvent -= letterSpawner.HandleSubmit;

        TeleportPlayer(_currentPositionPlayer);
        Debug.Log("PlayerRegresado");
        //player.transform.rotation = _currentRotationPlayer;

        var cc = player.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = true;

        UIManager.Instance.CheckEnd();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        book.SetActive(true);
        candle.SetActive(true);
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
            var sceneChange = FindFirstObjectByType<ChangeScene>();
            if (sceneChange)
                sceneChange.SelectSceneT(2);
            else
                Debug.Log("No hay SceneChange en la escena weon");
        }
    }


    private void EnemyDeath()
    {
        Debug.Log("Victoriaa");
        _isPlayerAlive = true;

        EndCombat();
    }

    private void PlayerDeath()
    {
        Debug.Log("Derrota");
        //AudioManager.instance.StopSFX();
        _isPlayerAlive = false;
        EndCombat();
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
        {
            Debug.LogWarning("Enemy not found");
        }
    }

    public void SetUpCombat()
    {
        isCombat = true;

        // Audio de combate: música siempre + latidos según vida (vía mixers).
        if (playerHealth == null && player != null)
            playerHealth = player.GetComponent<PlayerHealth>();
        var combatAudio = CombatAudioController.TryGetInstance();
        if (combatAudio != null && playerHealth != null)
            combatAudio.StartCombatAudio(playerHealth);
        else
            Debug.LogWarning("CombatAudioController o PlayerHealth no asignados: combate sin música/latidos. Arrastra las referencias en el inspector.");
        //SoundManager.Instance.CreateSound().WithSoundData(BGMMusic).Play();

        if (player == null) Debug.LogError("¡PLAYER es null!");
        if (enemy == null) Debug.LogError("¡ENEMY es null!");
        if (OptionsScript.Instance == null) Debug.LogError("¡OptionsScript.Instance es null!");
        else if (OptionsScript.Instance.PixelationShaderMaterial == null)
            Debug.LogError("¡PixelationShaderMaterial es null!");
        if (inputHandler == null) Debug.LogError("¡inputHandler es null!");
        if (CameraHolder == null) Debug.LogError("¡CameraHolder es null!");
        if (letterSpawner == null) Debug.LogError("¡letterSpawner es null!");
        if (UIManager.Instance == null) Debug.LogError("¡UIManager.Instance es null!");

        var cc = player.GetComponent<CharacterController>();
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

        TeleportEnemy(enemySpawner.transform.position);
        TeleportPlayer(playerSpawner.transform.position);
        inputHandler.SetCombat();
        CameraHolder.transform.rotation = Quaternion.Euler(0, 0, 0);
        baseRotationXCamera = CameraHolder.transform.rotation.eulerAngles.x;
        player.transform.LookAt(enemy.transform.position);
        letterSpawner.EmptyAll();
        letterSpawner.FillCharQueue();
        UIManager.Instance.ActivateCanvas(UIManager.Instance._combatCanvas);
        inputHandler.ExactCharEvent -= letterSpawner.HandleTypedChar;
        inputHandler.SubmitEvent -= letterSpawner.HandleSubmit;
        inputHandler.ExactCharEvent += letterSpawner.HandleTypedChar;
        inputHandler.SubmitEvent += letterSpawner.HandleSubmit;
    }

    private void LookAtBook()
    {
        inputHandler.DesactivateTyping();
        if (CameraHolder != null)
        {
            CameraHolder.transform.DOKill();

            SoundManager.Instance.CreateSound().WithSoundData(BGMMusic).Play();
            var currentCameraRotation = CameraHolder.transform.rotation.eulerAngles;
            var newCameraRotation =

                new Vector3(baseRotationXCamera + 45.0f, currentCameraRotation.y, currentCameraRotation.z);
            CameraHolder.transform.DORotate(newCameraRotation, 0.3f);
            letterSpawner.gameObject.SetActive(true);
        }
    }

    private void LookAtEnemy()
    {
        // letterSpawner.gameObject.SetActive(false);
        inputHandler.EnableTyping();
        if (CameraHolder != null)
        {
            CameraHolder.transform.DOKill();
            var currentCameraRotation = CameraHolder.transform.rotation.eulerAngles;
            var newCameraRotation =
                new Vector3(baseRotationXCamera, currentCameraRotation.y, currentCameraRotation.z);

            CameraHolder.transform.DORotate(newCameraRotation, 0.3f);
        }
    }

    public void ApproachToPlayer()
    {
        if (player)
        {
            var direction = (player.transform.position - enemy.transform.position).normalized;

            var newDirection = new Vector3(direction.x, 0, direction.z);
            enemy.transform.position += newDirection * (enemySpeedToApproach * Time.deltaTime);

            Debug.Log(Vector3.Distance(enemy.transform.position, player.transform.position));
            if (Vector3.Distance(enemy.transform.position, player.transform.position) <= 2.0f)
            {
                _isTakingDamage = true;
                LookAtEnemy();
                TimelinesManager.instance.PlayTimeLine(TimelinesManager.instance.TakeDamageTimeline);
            }
        }
    }

    public void ResetEnemyPosition()
    {
        // El enemigo golpea al jugador (1 de daño) antes de volver a su sitio.
        // Sin esto el jugador nunca pierde vida ni muere.
        if (enemy != null)
        {
            EnemyAttack enemyAttack = enemy.GetComponent<EnemyAttack>();
            if (enemyAttack == null)
                enemyAttack = enemy.GetComponentInChildren<EnemyAttack>();
            if (enemyAttack != null)
                enemyAttack.Attack(1);
            else
                Debug.LogWarning("El enemigo no tiene EnemyAttack; el jugador no recibe daño.");
        }
        _isTakingDamage = false;
        TeleportEnemy(enemySpawner.transform.position);
    }
}