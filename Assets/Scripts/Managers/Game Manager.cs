using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public bool isPlayerSound;


    private void OnEnable()
    {
        PlayerInputHandler.MovementEvent += PlayWalkSound;
        PlayerInputHandler.StopMovementEvent += StopMoveSound;
    }


    private void OnDisable()
    {
        
        PlayerInputHandler.MovementEvent -= PlayWalkSound;
        PlayerInputHandler.StopMovementEvent -= StopMoveSound;
    }
    private void StopMoveSound()
    {
        StopAllCoroutines();
    }

    private void PlayWalkSound()
    {
        if(!isPlayerSound)
        {
            StartCoroutine(PlayerWalkSound());
        }
    }

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
    void Start()
    {
        AudioManager.instance.PlayBGM(SoundType.FONDO, 0.5f);
    }
    private IEnumerator PlayerWalkSound()
    {
        isPlayerSound = true;
        yield return new WaitForSeconds(0.4f);
        AudioManager.instance.PlaySFXRandom(SoundType.PASOS, 0.40f, 0.55f);
        isPlayerSound = false;
    }
}
