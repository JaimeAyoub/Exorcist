using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public AudioManager audioManager;
    public PlayerMovement playerMovement;
    public bool isPlayerSound;
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

        CheckComponents();
    }
    void Start()
    {
        //audioManager.PlayBGM(SoundType.FONDO, 0.5f);
        AudioManager.instance.PlayBGM(SoundType.FONDO, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void CheckPlayerMoving()
    {
        if (playerMovement.IsMove())
        {
            if (isPlayerSound == false)
            {
                StartCoroutine(PlayerWalkSound());
            }
        }
        else
        {
        
            StopAllCoroutines();
            isPlayerSound = false;
        }
    }
    private IEnumerator PlayerWalkSound()
    {
        isPlayerSound = true;
        yield return new WaitForSeconds(0.4f);
        AudioManager.instance.PlaySFXRandom(SoundType.PASOS, 0.40f, 0.55f);
        isPlayerSound = false;
    }


    void CheckComponents()
    {
        if (audioManager == null)
        {
            audioManager = FindAnyObjectByType<AudioManager>();
        }
        else
        {
            Debug.LogWarning("No se encontro el AudioManager");
        }

        if (playerMovement == null)
        {
            playerMovement = FindAnyObjectByType<PlayerMovement>();
        }
        else
        {
            Debug.LogWarning("No se encontro el PlayerMovement");
        }

        //if (enemyMovement == null)
        //{
        //    enemyMovement = FindAnyObjectByType<EnemyMovement>();
        //}
        //else
        //{
        //    Debug.LogWarning("No se encontro el EnemyMovement");
        //}
    }

}
