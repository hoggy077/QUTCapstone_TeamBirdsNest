using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

///*
/// Specific sound files can be played in other scripts using the following line of code: (also required "using UnityEngine.Audio;" in headspace)
///     AudioManager.instance.PlaySound("SoundNameHere");
///

public class AudioManager : MonoBehaviour
{
    //Variables
    public SoundClass[] sounds;
    public static AudioManager instance;
    [SerializeField] public Slider volumeSfxSlider;
    [SerializeField] public Slider volumeBgmSlider;
    public TextMeshProUGUI sfxText;
    public TextMeshProUGUI bgmText;

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

    //Updates the sound to match the Options SFX Slider (& Resets BGM to not change)
    public void ChangeSFXVolume()
    {
        float bgmTemp = sounds[5].source.volume;

        foreach (SoundClass s in sounds)
        {
            s.source.volume = volumeSfxSlider.value;
        }

        sounds[5].source.volume = bgmTemp;

        sfxText.text = (Math.Round(volumeSfxSlider.value * 100) + "%").ToString();

    }

    public void ChangeBGMVolume()
    {
        sounds[5].source.volume = volumeBgmSlider.value;
        bgmText.text = (Math.Round(volumeBgmSlider.value * 100) + "%").ToString();
    }

    //Function which updates the UI slider on load
    public void UpdateSliders()
    {
        volumeBgmSlider.value = sounds[5].source.volume;
        volumeSfxSlider.value = sounds[0].source.volume;
    }
}
