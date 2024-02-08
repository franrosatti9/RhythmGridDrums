using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource sfxSourceBackground;
    private AudioClip currentBackgroundPart;
    public int beatCounter = 0;
    private bool hastenClapPlayed;
    [SerializeField] private SamplesSO _samples;
    public SamplesSO Samples => _samples;
    public static AudioManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        beatCounter = -1;
    }

    private void OnEnable()
    {
        BeatManager.instance.OnClapPlayed += PlayClap;
        BeatManager.instance.OnEveryBeat += PlayHihat;
        BeatManager.instance.OnEveryBeat += HandleBackgroundMusic;
        BeatManager.instance.OnHasteRealised += QueueHasteClap;
        GameManager.instance.OnGameFinished += FinishedGameSfx;
    }

    private void HandleBackgroundMusic()
    {
        beatCounter++;
        if (beatCounter > 7) beatCounter = 0;
        
        
        
        
    }

    private void FinishedGameSfx()
    {
        // TODO: Play Crash and piano/instrument stop
        sfxSource.PlayOneShot(_samples.finishImpact);
    }

    public void PlayClap(float beatPlayed)
    {
        // TODO: See where to handle backing track sections
        sfxSource.PlayOneShot(_samples.clap);
        
        // Repeat music to make glitchy effect when hastened clap
        if (hastenClapPlayed)
        {
            beatCounter--;
            sfxSourceBackground.Play();
            hastenClapPlayed = false;
            return;
        }
        
        if(beatCounter % 2 != 0) return;
        
        
        
        PlayRandomBackgroundPart(beatCounter / 2);
        

    }
    
    
    public void PlayKick()
    {
        sfxSource.PlayOneShot(_samples.kick);
    }

    public void PlayHihat()
    {
        sfxSourceBackground.PlayOneShot(_samples.hiHat);
    }

    public void PlayRandomBackgroundPart(int beatPlayed)
    {
        currentBackgroundPart = _samples.GetRandomBackgroundPart(beatPlayed);
        sfxSourceBackground.clip = currentBackgroundPart;
        sfxSourceBackground.Play();
    }

    public void QueueHasteClap()
    {
        hastenClapPlayed = true;
    }
    
}
