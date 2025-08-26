using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityUtils;
public class SoundManager : PersistentSingleton<SoundManager>
{
    IObjectPool<SoundEmitter> _soundEmitterPool;
    private readonly List<SoundEmitter> _activeSoundEmitters = new();
    public readonly Queue<SoundEmitter> FrequentSoundEmitters = new();

    [SerializeField] SoundEmitter soundEmitterPrefab;
    [SerializeField] private bool collectionCheck = true;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxPoolSize = 100;
    [SerializeField] private int maxSoundInstances = 30;

    private void Start()
    {
        InitializePool();
    }

    public SoundBuilder CreateSound() => new SoundBuilder(this);
    
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
}
