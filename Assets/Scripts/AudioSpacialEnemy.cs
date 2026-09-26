using System;
using UnityEngine;

public class AudioSpacialEnemy : MonoBehaviour
{
 [SerializeField] AudioSource _enemyNoises;
 [SerializeField] AudioClip[] _audioClips;


 public void Start()
 {
     if (_audioClips.Length > 0)
     {
         _enemyNoises.clip = _audioClips[0];
        // _enemyNoises.Play();
     }
 }

 public void Update()
 {
     if (Input.GetKeyDown(KeyCode.L))
     {
         _enemyNoises.Play();
     }
 }
}
