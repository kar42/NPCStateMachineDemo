using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundOnStart : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;

    void Start()
    {
       // SoundManager.Instance.PlaySound(audioClip);
    }
}
