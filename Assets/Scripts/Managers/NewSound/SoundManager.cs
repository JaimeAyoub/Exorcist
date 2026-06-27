using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityUtils;
using UnityEngine.Audio;

public class SoundManager : Singleton<SoundManager>
{
    IObjectPool<SoundEmitter> _soundEmitterPool;
    private readonly List<SoundEmitter> _activeSoundEmitters = new();
    public readonly Queue<SoundEmitter> FrequentSoundEmitters = new();

    [SerializeField] SoundEmitter soundEmitterPrefab;
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxPoolSize = 100;
    [SerializeField] private int maxSoundInstances = 30;

    [Header("Music Settings")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    private AudioSource _musicSource;
    public AudioClip musicClip;

    private void Start()
    {
        InitializePool();
        InitializeMusicSource();
    }

    private void Update()
    {
        PlayMusic();
    }

    public SoundBuilder CreateSound() => new SoundBuilder(this);
    

    private void InitializeMusicSource()
    {
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;
        _musicSource.outputAudioMixerGroup = musicMixerGroup;
    }

# region Music
    public void PlayMusic()
    {
        if (musicClip == null) return;

        if (_musicSource.clip == musicClip && _musicSource.isPlaying) return;

        _musicSource.clip = musicClip;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        _musicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        _musicSource.volume = volume;
    }
# endregion

#region SFX
    public bool CanPlaySound(SoundData sData)
    {
        if (!sData.frequentSound) return true;

        if (FrequentSoundEmitters.Count >= maxSoundInstances && FrequentSoundEmitters.TryDequeue(out var soundEmitter))
        {
            try
            {
                soundEmitter.Stop();
                return true;
            }
            catch
            {
                Debug.Log("SoundEmitter is already released");
            }
            return false;
        }
        return true;
    }

    public SoundEmitter Get()
    {
        return _soundEmitterPool.Get();
    }

    public void ReturnToPool(SoundEmitter soundEmitter)
    {
        _soundEmitterPool.Release(soundEmitter);
    }

    private void InitializePool()
    {
        _soundEmitterPool = new ObjectPool<SoundEmitter>(
            CreateSoundEmitter,
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyObjectPool,
            collectionCheck,
            defaultCapacity,
            maxPoolSize);
    }

    SoundEmitter CreateSoundEmitter()
    {
        SoundEmitter soundEmitter = Instantiate(soundEmitterPrefab);
        soundEmitter.gameObject.SetActive(false);
        return soundEmitter;
    }

    private void OnTakeFromPool(SoundEmitter emitter)
    {
        emitter.gameObject.SetActive(true);
        _activeSoundEmitters.Add(emitter);
    }
    private void OnReturnedToPool(SoundEmitter emitter)
    {
        emitter.gameObject.SetActive(false);
        _activeSoundEmitters.Remove(emitter);
    }

    private void OnDestroyObjectPool(SoundEmitter emitter)
    {
        Destroy(emitter.gameObject);
    }
    #endregion
}