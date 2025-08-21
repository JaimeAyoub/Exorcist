using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityUtils;
public class SoundManager : PersistentSingleton<SoundManager>
{
    IObjectPool<SoundEmitter> soundEmitterPool;
    readonly List<SoundEmitter> activeSoundEmitters = new();
    public readonly Queue<SoundEmitter> frequentSoundEmitters = new();

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
        if (!sData.frequentSound) return true;

        if (frequentSoundEmitters.Count >= maxSoundInstances && frequentSoundEmitters.TryDequeue(out var soundEmitter))
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
        emitter.gameObject.SetActive(false);
        activeSoundEmitters.Remove(emitter);
    }

    private void OnDestroyObjectPool(SoundEmitter emitter)
    {
        Destroy(emitter.gameObject);
    }
}
