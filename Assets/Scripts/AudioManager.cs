using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource sfxSourceBackground;
    [SerializeField] private AudioMixer musicMixer;
    private AudioClip currentBgSection;
    private AudioClip nextBgSection;
    private int currentBgSectionIndex;
    
    public int beatCounter = 0;
    private float secPerBeat;
    private float hastenedTrackTime;
    
    private bool hastenClapPlayed;
    private bool trackOnGlitchEffect;

    private int[] beatsToAllowGlitchEffect = {2,6,10,13,14};

    [SerializeField] private SamplesSO _samples;
    [SerializeField] private ScoreController scoreController;
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
        musicMixer = sfxSourceBackground.outputAudioMixerGroup.audioMixer;
        secPerBeat = BeatManager.instance.SecondsPerBeat;
    }

    private void OnEnable()
    {
        BeatManager.instance.OnClapPlayed += PlayClap;
        scoreController.OnComboUpdated += PlayLostComboEffect;
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
        sfxSource.PlayOneShot(_samples.finishMusic);
        sfxSourceBackground.Stop();
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

    public void PlayNextBgSection()
    {
        PostProcessManager.instance.HasteEffect();
        Debug.LogWarning("SONG GLITCH ON BEAT " + beatCounter + "(" + beatCounter % 16 + ")");
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

    public void PlayLostComboEffect(int mult, float progress)
    {
        if (mult != 1 || progress != 0) return;
        StopAllCoroutines();
        StartCoroutine(LostComboEffect());
    }

    IEnumerator LostComboEffect()
    {
        float elapsed = 0f;
        float duration = 0.3f;
        float currValue;
        float pitchDecreaseAmount = 0.01f;
        float initialPitch = 1;

        while (elapsed < duration)
        {
            musicMixer.GetFloat("Pitch", out currValue);
            musicMixer.SetFloat("Pitch", currValue - pitchDecreaseAmount);  
            elapsed += Time.deltaTime;
            yield return null;
        }

        musicMixer.SetFloat("Pitch", initialPitch);
    }
    
    
    
}
