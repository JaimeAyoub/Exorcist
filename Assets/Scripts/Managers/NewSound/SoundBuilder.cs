
using UnityEngine;

public class SoundBuilder 
{
    readonly SoundManager _soundManager;
    private SoundData _soundData;
    private Vector3 _position = Vector3.zero;
    private bool _randomPitch;

    public SoundBuilder(SoundManager soundManager)
    {
        this._soundManager = soundManager;
    }

    public SoundBuilder WithSoundData(SoundData soundData)
    {
        this._soundData = soundData;
        return this;
    }

    public SoundBuilder WithPosition(Vector3 position)
    {
        this._position = position;
        return this;
    }

    public SoundBuilder WithRandomPitch()
    {
        this._randomPitch = true;
        return this;
    }

    public void Play()
    {
        if (!_soundManager.CanPlaySound(_soundData)) return;

        var soundEmitter = _soundManager.Get();
        soundEmitter.Initialize(_soundData);
        soundEmitter.transform.position = _position;
        soundEmitter.transform.parent = SoundManager.Instance.transform;

        if(_randomPitch)
        {
            soundEmitter.WithRandomPitch();
        }

        if(_soundData.frequentSound)
        {
            _soundManager.FrequentSoundEmitters.Enqueue(soundEmitter);
        }
        soundEmitter.Play();
    }
}