using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using UnityUtils;

public class SoundEmitter : MonoBehaviour
{
    public SoundData Data {  get; private set; }
    private AudioSource _audioSource;
    private Coroutine _playingCoroutine;

    private void Awake()
    {
        _audioSource = gameObject.GetOrAdd<AudioSource>();
    }

    public void Play()
    {
        if(_playingCoroutine != null)
        {
            StopCoroutine(_playingCoroutine);
        }

        _audioSource.Play();
        _playingCoroutine = StartCoroutine(WaitForSoundToEnd());
    }

    private IEnumerator WaitForSoundToEnd()
    {
        yield return new WaitWhile(() => _audioSource.isPlaying);
        SoundManager.Instance.ReturnToPool(this);
    }

    public void Stop()
    {
        if(_playingCoroutine != null)
        {
            StopCoroutine(_playingCoroutine);
            _playingCoroutine = null;
        }

        _audioSource.Stop();
        SoundManager.Instance.ReturnToPool(this);
    }


    public void Initialize(SoundData sData)
    {
        Data = sData;
        _audioSource.clip = sData.clip;
        _audioSource.outputAudioMixerGroup = sData.mixerGroup;
        _audioSource.loop = sData.loop;
        _audioSource.playOnAwake = sData.playOnAwake;
    }

    public void WithRandomPitch(float min = -0.05f, float max = 0.05f)
    {
        _audioSource.pitch += Random.Range(min, max);
    }
}
