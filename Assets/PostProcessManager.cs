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


    [SerializeField] AnimationCurve vignetteColorCurve;
    [SerializeField] AnimationCurve vignetteIntensityCurve;
    [SerializeField] AnimationCurve lensDistortionCurve;
    [SerializeField] Color vignetteColor;
    [SerializeField] ColorParameter defaultVignetteColor;
    float defaultVignetteIntensity;

    [SerializeField] float lostComboEffectDuration;
    [SerializeField] float successEffectDuration;
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
        if (mult != 1 || progress != 0) return;
        StopAllCoroutines();
        StartCoroutine(AnimateLostComboEffect());
    }

    public void SuccessEffect(int beat)
    {
        //float diff = 100f - healthLeft;

        //float intensity = 1; //Unity.Mathematics.math.remap(0f, 100f, 0.25f, 0.35f, diff);


        StartCoroutine(AnimateSuccessEffect());
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
        _lensDistortion.intensity.value = 1f;
        //vignette.intensity.value = defaultVignetteIntensity;
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
        _vignette.intensity.value = defaultVignetteIntensity;
        //vignette.intensity.value = vignetteCurve.keys[vignetteCurve.length - 1].value;
    }
}

