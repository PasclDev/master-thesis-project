using System;
using Classes;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public AudioSource aus;

    public static AudioManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            aus = gameObject.AddComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Play(string soundName)
    {
        Sound s = Array.Find(sounds, sound => sound.name == soundName);
        aus.clip = s.clip;
        aus.volume = s.volume;
        aus.pitch = s.pitch;
        aus.Play();
    }

    public float GetSoundLength(string soundName)
    {
        return Array.Find(sounds, sound => sound.name == soundName).clip.length;
    }
}
