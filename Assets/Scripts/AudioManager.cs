using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource sfxSourceBackground;
    private AudioClip currentBgSection;
    private AudioClip nextBgSection;
    private int currentBgSectionIndex;
    
    public int beatCounter = 0;
    private float secPerBeat;
    private float hastenedTrackTime;
    
    private bool hastenClapPlayed;
    private bool trackOnGlitchEffect;

    private int[] beatsToAllowGlitchEffect = {0,1,5,6,9,10,13,14};

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
        
        beatCounter = 0;
    }

    private void Start()
    {
        secPerBeat = BeatManager.instance.SecondsPerBeat;
    }

    private void OnEnable()
    {
        BeatManager.instance.OnClapPlayed += PlayClap;
        //BeatManager.instance.OnEveryBeat += PlayHihat;
        BeatManager.instance.OnEveryBeat += HandleBackgroundMusic;
        BeatManager.instance.OnHasteRealised += OnHasteClap;
        GameManager.instance.OnGameFinished += FinishedGameSfx;
    }

    private void OnDisable()
    {
        BeatManager.instance.OnClapPlayed -= PlayClap;
        //BeatManager.instance.OnEveryBeat -= PlayHihat;
        BeatManager.instance.OnEveryBeat -= HandleBackgroundMusic;
        BeatManager.instance.OnHasteRealised -= OnHasteClap;
        GameManager.instance.OnGameFinished -= FinishedGameSfx;
    }

    private void HandleBackgroundMusic()
    {
        beatCounter++;
        
        
        //if (beatCounter > 7) beatCounter = 0;

        // If on effect, get track back to normal
        if (beatCounter % 2 != 0 && trackOnGlitchEffect)
        {
            Debug.LogWarning("BACK TO NORMAL ON BEAT " + beatCounter + " TO BEAT " + hastenedTrackTime / secPerBeat);
            sfxSourceBackground.time = BeatManager.instance.CurrentBeatPosition * secPerBeat;
            trackOnGlitchEffect = false;
        }
        
        //PlayBackgroundSection(beatCounter / 2);

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
            if(beatsToAllowGlitchEffect.Contains(beatCounter % 16)) PlayNextBgSection();
            hastenClapPlayed = false;
        }
    }
    
    
    public void PlayKick()
    {
        sfxSource.PlayOneShot(_samples.kick);
    }

    public void PlayHihat()
    {
        sfxSourceBackground.PlayOneShot(_samples.hiHat);
    }

    public void PlayBackgroundSection(int beatPlayed)
    {
        // Replace current section with next and play it, save next for haste glitchy effect
        if(currentBgSection != null) currentBgSection = nextBgSection;
        else currentBgSection = _samples.GetRandomBackgroundPart(beatPlayed);
        
        nextBgSection = _samples.GetRandomBackgroundPart((beatPlayed + 1) % 4);
        
        sfxSourceBackground.clip = currentBgSection;
        sfxSourceBackground.Play();
    }

    public void PlayNextBgSection()
    {
        Debug.LogWarning("SONG GLITCH ON BEAT " + beatCounter);
        trackOnGlitchEffect = true;
        hastenedTrackTime = BeatManager.instance.CurrentBeatPosition * secPerBeat + secPerBeat;
        sfxSourceBackground.time = hastenedTrackTime;
    }

    public void OnHasteClap(bool playNow)
    {
        if (playNow)
        {
            if(beatsToAllowGlitchEffect.Contains(beatCounter % 16)) PlayNextBgSection();
        }  
        else
        {
             hastenClapPlayed = true;
        }
    }
    
    
    
}
