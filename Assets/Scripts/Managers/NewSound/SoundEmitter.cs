using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using UnityUtils;

public class SoundEmitter : MonoBehaviour
{
    public SoundData Data {  get; private set; }
    private AudioSource _audioSource;
    private Coroutine _playingCoroutine;

    /// <summary>Acceso al AudioSource por si hay que aplicarle efectos en runtime (p.ej. lowpass).</summary>
    public AudioSource AudioSource => _audioSource;

    private void Awake()
    {
        _audioSource = gameObject.GetOrAdd<AudioSource>();
    }

    public void Play()
    {
        if(_playingCoroutine != null)
        {
            StopCoroutine(_playingCoroutine);
            _playingCoroutine = null;
        }

        _audioSource.Play();

        // Los loops son persistentes (música/latidos): no vuelven solos al pool,
        // hay que llamar a Stop() explícitamente.
        if (!_audioSource.loop)
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
        if (SoundManager.Instance != null)
            SoundManager.Instance.ReturnToPool(this);
        else
            gameObject.SetActive(false);
    }

    public void Initialize(SoundData sData)
    {
        Data = sData;
        _audioSource.clip = sData.clip;
        _audioSource.outputAudioMixerGroup = sData.mixerGroup;
        _audioSource.loop = sData.loop;
        _audioSource.playOnAwake = sData.playOnAwake;
        _audioSource.pitch = 1f;
        // El pool reutiliza sources: si alguien les hizo fade, restaura el volumen.
        _audioSource.volume = 1f;
    }

    public void WithRandomPitch(float min = -0.05f, float max = 0.15f)
    {
        _audioSource.pitch = 1;
        _audioSource.pitch += Random.Range(min, max);
    }

    public void WalkSound()
    {
        var index = Random.Range(0, Data.walkClips.Count);
        _audioSource.clip = Data.walkClips[index];
    }
    
}
