using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAnimation : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _visual;
    [SerializeField] private MeshRenderer _meshRenderer;

    [SerializeField] private float updateTransitionTime;
    [SerializeField] private float deactivateTransitionTime;
    
    
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartActivateAnim(TileDataSO data)
    {
        StartCoroutine(ActivateAnimation(data));
    }

    public void StartCompletedAnim(TileDataSO data, Action resetCallback)
    {
        StartCoroutine(CompletedAnimation(data, resetCallback));
    }

    public void StartDeactivatedAnim(TileDataSO data, Action resetCallback)
    {
        StopAllCoroutines();
        StartCoroutine(DeactivateAnimation(data, resetCallback));
    }
    
    public void StartUpdateTileAnim(TileDataSO tileData)
    {
        StopAllCoroutines();
        StartCoroutine(ActivateAnimation(tileData));
    }

    IEnumerator ActivateAnimation(TileDataSO data)
    {
        _visual.sprite = data.sprite;
        float elapsed = 0;

        var initialMat = _meshRenderer.material;
        var finalMat = data.activatedColor;
        var initialSpriteColor = new Color(1, 1, 1, 0);
            
        

        while (elapsed < updateTransitionTime)
        {
            _meshRenderer.material.Lerp(initialMat, finalMat, elapsed);
            _visual.color = Color.Lerp(initialSpriteColor, Color.white, elapsed);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Complete Lerps
        _meshRenderer.material.Lerp(initialMat, finalMat, 1);
        _visual.color = Color.Lerp(Color.clear, Color.white, 1);
        
    }
    
    IEnumerator UpdateAnimation(TileDataSO data)
    {
        _visual.sprite = data.sprite;
        float elapsed = 0;

        var initialMat = _meshRenderer.material;
        var finalMat = data.activatedColor;
        var initialSpriteColor = new Color(1, 1, 1, 0);
            
        

        while (elapsed < updateTransitionTime)
        {
            _meshRenderer.material.Lerp(initialMat, finalMat, elapsed);
            _visual.color = Color.Lerp(initialSpriteColor, Color.white, elapsed);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Complete Lerps
        _meshRenderer.material.Lerp(initialMat, finalMat, 1);
        _visual.color = Color.Lerp(Color.clear, Color.white, 1);
        
    }
    
    IEnumerator CompletedAnimation(TileDataSO data, Action resetCallback)
    {
        // Flash completed color
        _meshRenderer.material = data.completedFlashColor;
        yield return new WaitForSeconds(0.25f);
        _meshRenderer.material = data.activatedColor;
        yield return new WaitForSeconds(0.25f);
        _meshRenderer.material = data.completedFlashColor;
        yield return new WaitForSeconds(0.25f);
        _meshRenderer.material = data.activatedColor;

        StartCoroutine(DeactivateAnimation(data, resetCallback));



    }

    IEnumerator DeactivateAnimation(TileDataSO data, Action resetCallback)
    {
        // Deactivate 
        float elapsed = 0;
        
        var initialMat = data.activatedColor;
        var finalMat = data.deactivatedColor;
        
        var finalSpriteColor = new Color(1, 1, 1, 0);
        
        while (elapsed < deactivateTransitionTime)
        {
            _meshRenderer.material.Lerp(initialMat, finalMat, elapsed);
            _visual.color = Color.Lerp(Color.white, finalSpriteColor, elapsed);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Reset color and symbol sprite
        _meshRenderer.material = finalMat;
        _visual.color = Color.Lerp(Color.white, finalSpriteColor, 1);
        _visual.sprite = null;
        resetCallback.Invoke();
    }

    
}
