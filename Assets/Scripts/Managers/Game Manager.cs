
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
        SoundManager.Instance.CreateSound().WithSoundData(walkSoundData).WithRandomPitch().StepSound().Play();
    }

    void Awake()
    {
        base.Awake();
    }
    void Start()
    {
        AudioManager.instance.PlayBGM(SoundType.FONDO, 0.5f);
    }
    
    
    
}
