using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;
using UnityUtils;

public class SoundEmitter : MonoBehaviour
{
    public SoundData Data {  get; private set; }
    AudioSource audioSource;
    Coroutine playingCoroutine;

    private void Awake()
    {
        audioSource = gameObject.GetOrAdd<AudioSource>();
    }

    public void Play()
    {
        if(playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
        }

        audioSource.Play();
        playingCoroutine = StartCoroutine(WaitForSoundToEnd());
    }

    IEnumerator WaitForSoundToEnd()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        SoundManager.Instance.ReturnToPool(this);
    }

    public void Stop()
    {
        if(playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
            playingCoroutine = null;
        }

        audioSource.Stop();
        SoundManager.Instance.ReturnToPool(this);
    }


    public void Initialize(SoundData sData)
    {
        Data = sData;
        audioSource.clip = sData.clip;
        audioSource.outputAudioMixerGroup = sData.mixerGroup;
        audioSource.loop = sData.loop;
        audioSource.playOnAwake = sData.playOnAwake;
    }

    public void WithRandomPitch(float min = -0.05f, float max = 0.05f)
    {
        audioSource.pitch += Random.Range(min, max);
    }
}
