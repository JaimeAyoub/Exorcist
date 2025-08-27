using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.Serialization;


[Serializable]
public class SoundData
{
    public AudioClip clip;
    public AudioMixerGroup mixerGroup;
    public bool loop;
    public bool playOnAwake;
    public bool frequentSound;
    public List<AudioClip> walkClips;
    [SerializeField] public FloorType floorType;
}
