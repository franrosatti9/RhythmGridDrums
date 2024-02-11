using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;

public class PostProcessManager : MonoBehaviour
{
    [SerializeField] private PostProcessVolume volume;
    private ChromaticAberration _chromaticAberration;
    private Vignette _vignette;
    private LensDistortion _lensDistortion;
    private ColorGrading _colorGrading;


    [SerializeField] AnimationCurve vignetteColorCurve;
    [SerializeField] AnimationCurve vignetteIntensityCurve;
    [SerializeField] AnimationCurve lensDistortionCurve;
    [SerializeField] AnimationCurve colorGradingCurve;
    [SerializeField] Color vignetteColor;
    [SerializeField] ColorParameter defaultVignetteColor;
    float defaultVignetteIntensity;

    [SerializeField] float lostComboEffectDuration;
    [SerializeField] float successEffectDuration;
    [SerializeField] float hasteEffectDuration;
    [SerializeField] private ScoreController scoreController;
    public static PostProcessManager instance;

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
    }

    void Start()
    {
        volume.profile.TryGetSettings(out _chromaticAberration);
        volume.profile.TryGetSettings(out _vignette);
        volume.profile.TryGetSettings(out _lensDistortion);
        volume.profile.TryGetSettings(out _colorGrading);
        
        defaultVignetteIntensity = _vignette.intensity.value;
        //defaultVignetteColor = (ColorParameter)vignette.color;
    }

    private void OnEnable()
    {
        GameManager.instance.OnTileSuccess += SuccessEffect;
        scoreController.OnComboUpdated += LostComboEffect;
    }

    private void OnDisable()
    {
        GameManager.instance.OnTileSuccess -= SuccessEffect;
    }

    public void LostComboEffect(int mult, float progress)
    {
        // TODO: IMPROVE TO ONLY LiSTEN TO EVENT WHEN LOST COMBO COMPLETELY
        if (mult != 1 || progress != 0) return;
        StopAllCoroutines();
        StartCoroutine(AnimateLostComboEffect());
    }

    public void SuccessEffect(int beat)
    {
        StartCoroutine(AnimateSuccessEffect());
    }

    public void HasteEffect()
    {
        StartCoroutine(AnimateHasteEffect());
    }

    IEnumerator AnimateSuccessEffect()
    {
        float elapsed = 0f;

        Color initialColor = _vignette.color.value;
        Color newColor = vignetteColor;
        float defaultLensIntensity = 1;

        while(elapsed < successEffectDuration)
        {
            _vignette.color.Interp(initialColor, newColor,vignetteColorCurve.Evaluate(elapsed / successEffectDuration));
            _lensDistortion.intensity.value = lensDistortionCurve.Evaluate(elapsed / successEffectDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _vignette.color.value = defaultVignetteColor;
        _lensDistortion.intensity.value = 0f;
    }

    IEnumerator AnimateLostComboEffect()
    {
        float elapsed = 0f;

        while(elapsed < lostComboEffectDuration)
        {
            _vignette.intensity.value = vignetteIntensityCurve.Evaluate(elapsed / lostComboEffectDuration);
            _lensDistortion.intensity.value = -lensDistortionCurve.Evaluate(elapsed / successEffectDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // In case another coroutine was interrupted
        SetEverythingToDefault();
    }

    IEnumerator AnimateHasteEffect()
    {
        float elapsed = 0f;

        _colorGrading.enabled.value = true;
        
        while (elapsed < hasteEffectDuration)
        {
            _colorGrading.saturation.value = colorGradingCurve.Evaluate(elapsed / hasteEffectDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _colorGrading.enabled.value = false;
    }

    public void SetEverythingToDefault()
    {
        _colorGrading.saturation.value = 0;
        _colorGrading.enabled.value = false;
        
        _vignette.color.value = defaultVignetteColor;
        _vignette.intensity.value = defaultVignetteIntensity;
        
        _lensDistortion.intensity.value = 0f;
    }
}

