using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityUtils;
public class SoundManager : PersistentSingleton<SoundManager>
{
    IObjectPool<SoundEmitter> soundEmitterPool;
    readonly List<SoundEmitter> activeSoundEmitters = new();
    public readonly Dictionary<SoundData, int> Counts = new();

    [SerializeField] SoundEmitter soundEmitterPrefab;
    [SerializeField] bool collectionCheck = true;
    [SerializeField] int defaultCapacity = 10;
    [SerializeField] int maxPoolSize = 100;
    [SerializeField] int maxSoundInstances = 30;

    private void Start()
    {
        InitializePool();
    }

    public SoundBuilder CreateSound() => new SoundBuilder(this);
    
    public bool CanPlaySound(SoundData sData)
    {
        return !Counts.TryGetValue(sData, out var count) || count < maxSoundInstances;
    }

    public SoundEmitter Get()
    {
        return soundEmitterPool.Get();
    }

    public void ReturnToPool(SoundEmitter soundEmitter)
    {
        soundEmitterPool.Release(soundEmitter);
    }

    private void InitializePool()
    {
        soundEmitterPool = new ObjectPool<SoundEmitter>(
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
        activeSoundEmitters.Add(emitter);
    }
    private void OnReturnedToPool(SoundEmitter emitter)
    {
        if(Counts.TryGetValue(emitter.Data, out var count))
        {
            Counts[emitter.Data] -= count > 0 ? 1 : 0;
        }
        emitter.gameObject.SetActive(false);
        activeSoundEmitters.Remove(emitter);
    }

    private void OnDestroyObjectPool(SoundEmitter emitter)
    {
        Destroy(emitter.gameObject);
    }
}
