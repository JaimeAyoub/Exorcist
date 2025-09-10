using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    PASOS,
    TECLAS,
    FONDO,
    COMBATE,
    ENEMIGO,
    GulaDamage,
    IraDamage,
    PadreDamage,
    PlayerDamage
}

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    [Tooltip("Esta lista tiene que ser llenada en orden de acuerdo con el enum de arriba en la clase AudioManager")]
    [SerializeField]
    private List<AudioClip> soundList;

    public static AudioManager instance;
    public AudioSource audioSource;
    public AudioSource BGMSource;
    public AudioSource soundSourcePoint;

    [Header("Configuración de SFX")]
    [SerializeField] private int poolSize = 10; 
    private List<AudioSource> sfxPool = new List<AudioSource>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeSFXPool();
    }

    private void InitializeSFXPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            newSource.loop = false;
            sfxPool.Add(newSource);
        }
    }

    private AudioSource GetAvailableSFXSource()
    {
        foreach (AudioSource source in sfxPool)
        {
            if (!source.isPlaying)
                return source;
        }
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        newSource.loop = false;
        sfxPool.Add(newSource);
        return newSource;
    }

    public void PlaySFX(SoundType clip, float volume = 1f)
    {
        AudioSource source = GetAvailableSFXSource();
        source.PlayOneShot(soundList[(int)clip], volume);
    }

    public void PlayBGM(SoundType clip, float volume = 1f)
    {
        BGMSource.clip = soundList[(int)clip];
        BGMSource.volume = volume;
        BGMSource.loop = true;
        BGMSource.Play();
    }

    public void PlaySFXRandom(SoundType clip, float minValue, float maxValue, float volume = 1f)
    {
        AudioSource source = GetAvailableSFXSource();
        float randomPitch = Random.Range(minValue, maxValue);
        source.pitch = randomPitch;
        source.PlayOneShot(soundList[(int)clip], volume);
        StartCoroutine(ResetPitchAfterPlay(source, randomPitch));
    }

    private IEnumerator ResetPitchAfterPlay(AudioSource source, float originalPitch)
    {
        yield return new WaitForSeconds(0.1f);
        source.pitch = 1f;
    }

    public void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        AudioSource audioSource = Instantiate(soundSourcePoint, position, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        Destroy(audioSource.gameObject, clip.length);
    }

    public void StopSFX()
    {
        foreach (AudioSource source in sfxPool)
        {
            source.Stop();
        }
    }
}