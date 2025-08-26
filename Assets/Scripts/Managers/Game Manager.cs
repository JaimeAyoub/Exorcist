
using System.Collections;
using UnityEngine;
using UnityUtils;

public class GameManager : Singleton<GameManager>
{
    public bool isPlayerSound;
    public SoundData walkSoundData;

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
        base.Awake();
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
        SoundManager.Instance.CreateSound().WithSoundData(walkSoundData).Play();
    }
    private IEnumerator PlayerWalkSound()
    {
        isPlayerSound = true;
        yield return new WaitForSeconds(0.4f);
        AudioManager.instance.PlaySFXRandom(SoundType.PASOS, 0.40f, 0.55f);
        isPlayerSound = false;
    }
}
