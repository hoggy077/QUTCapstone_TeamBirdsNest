using System;
using UnityEngine;
using UnityEngine.Audio;

///*
/// Specific sound files can be played in other scripts using the following line of code: (also required "using UnityEngine.Audio;" in headspace)
///     FindObjectOfType<AudioManager>().PlaySound("SoundNameHere");
///

public class AudioManager : MonoBehaviour
{
    //Variables
    public SoundClass[] sounds;
    public static AudioManager instance;

    void Awake()
    {
        //Prevents destroying or duplicating audiomanager during scene change/loading
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        //Assigns the values for each listed sound file on startup
        foreach(SoundClass s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    //Public float to call sounds when needed from other scripts
    public void PlaySound(string name)
    {
        //finds the referenced sound file and plays it
        SoundClass s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("Sound was not found.");
            return;
        }
        s.source.Play();
        //Debug.Log("SoundFound");
    }
}
