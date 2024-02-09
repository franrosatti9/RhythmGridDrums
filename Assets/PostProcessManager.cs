using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Serialization;

public class PostProcessManager : MonoBehaviour
{
    [SerializeField] PostProcessVolume volume;
    ChromaticAberration chromaticAberration;
    Vignette vignette;


    [SerializeField] AnimationCurve chromaticAberrationCurve;
    [SerializeField] AnimationCurve vignetteColorCurve;
    [SerializeField] AnimationCurve vignetteIntensityCurve;
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
        volume.profile.TryGetSettings<ChromaticAberration>(out chromaticAberration);
        volume.profile.TryGetSettings<Vignette>(out vignette);
        defaultVignetteIntensity = vignette.intensity.value;
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

        Color initialColor = vignette.color.value;
        Color newColor = vignetteColor;

        while(elapsed < successEffectDuration)
        {
            vignette.color.Interp(initialColor, newColor,vignetteColorCurve.Evaluate(elapsed / successEffectDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        vignette.color.value = defaultVignetteColor;
        //vignette.intensity.value = defaultVignetteIntensity;
    }

    IEnumerator AnimateLostComboEffect()
    {
        Debug.LogWarning("ANIMATE LOST");
        float elapsed = 0f;

        while(elapsed < lostComboEffectDuration)
        {
            vignette.intensity.value = vignetteIntensityCurve.Evaluate(elapsed / lostComboEffectDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        vignette.intensity.value = defaultVignetteIntensity;
        //vignette.intensity.value = vignetteCurve.keys[vignetteCurve.length - 1].value;
    }
}

