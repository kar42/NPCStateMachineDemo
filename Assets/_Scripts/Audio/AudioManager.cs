using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[System.Serializable]
public class Sound
{
    
    public string m_name;
    public AudioClip[] m_clips;

    [Range(0f, 1f)]
    public float volume = 1.0f;
    [Range(0f, 1.5f)]
    public float pitch = 1.0f;
    public Vector2 m_randomVolumeRange = new Vector2(1.0f, 1.0f);
    public Vector2 m_randomPitchRange = new Vector2(1.0f, 1.0f);
    public bool m_loop = false;

    private AudioSource m_source;

    public void SetSource(AudioSource source)
    {
        m_source = source;
        int randomClip = Random.Range(0, m_clips.Length - 1);
        m_source.clip = m_clips[randomClip];
        //m_source.clip = 
        m_source.loop = m_loop;
    }
    
    public void SetMixerGroup(AudioMixerGroup mixerGroup)
    {
        //Debug.Log(m_source.name + " mixer source group: " + mixerGroup);
        m_source.outputAudioMixerGroup = mixerGroup;
    }

    public void Play()
    {
        if(m_clips.Length > 1)
        {
            int randomClip = Random.Range(0, m_clips.Length - 1);
            m_source.clip = m_clips[randomClip];
        }
        m_source.volume = volume;
        //m_source.pitch = pitch * Random.Range(m_randomPitchRange.x, m_randomPitchRange.y);
        m_source.Play();
    }

    public void Stop()
    {
        m_source.Stop();
    }

    public bool IsPlaying()
    {
        if (m_source.isPlaying)
            return true;
        else
            return false;
    }
}

public class AudioManager : MonoBehaviour
{
    // Make it a singleton class that can be accessible everywhere
    public static AudioManager instance;

    [SerializeField] AudioMixerGroup sfxMixerGroup;
    [SerializeField] AudioMixerGroup musicMixerGroup;
    [SerializeField] AudioMixerGroup voiceMixerGroup;
    [SerializeField] Sound[] sounds;
    

    private void Awake()
    {
        if(instance != null)
        {
            Debug.LogError("More than one AudioManger in scene");
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        for(int i = 0; i < sounds.Length; i++)
        {
            GameObject go = new GameObject("Sound_" + i + "_" + sounds[i].m_name);
            go.transform.SetParent(transform);
            sounds[i].SetSource(go.AddComponent<AudioSource>());
            if (sounds[i].m_name.Contains("_sfx"))
            {
                sounds[i].SetMixerGroup(sfxMixerGroup);
                
            }
            else if(sounds[i].m_name.Contains("_music"))
            {
                sounds[i].SetMixerGroup(musicMixerGroup);
            }
            else if(sounds[i].m_name.Contains("_voice"))
            {
                sounds[i].SetMixerGroup(voiceMixerGroup);
            }
        }
    }

    private void Update()
    {
        
    }

    public void PlaySound (string name)
    {
        for(int i = 0; i < sounds.Length; i++)
        {
            
            if (sounds[i].m_name == name)
            {
                sounds[i].Play();
                return;
            }
        }
        

        Debug.LogWarning("AudioManager: Sound name not found in list: " + name);
    }

    public void StopSound(string name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].m_name == name && sounds[i].IsPlaying())
            {
                sounds[i].Stop();
                return;
            }
        }
    }

    public bool IsPlaying(string name)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].m_name == name && sounds[i].IsPlaying())
            {
                return true;
            }
        }

        return false;
    }
}
