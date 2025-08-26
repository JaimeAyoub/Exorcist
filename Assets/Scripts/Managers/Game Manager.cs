
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
    }
    void Start()
    {
        AudioManager.instance.PlayBGM(SoundType.FONDO, 0.5f);
    }
    
    private IEnumerator PlayerWalkSound()
    {
        isPlayerSound = true;
        SoundManager.Instance.CreateSound().WithSoundData(walkSoundData).WithRandomPitch().Play();
        yield return new WaitForSeconds(walkSoundData.clip.length);
        isPlayerSound = false;
    }
}
