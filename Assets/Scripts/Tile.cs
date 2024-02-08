using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Tile : MonoBehaviour
{
    private TileAnimation _animation;
    public bool Activated { get; private set; }
    public TileType CurrentType { get; private set; }
    [SerializeField] private TileDataSO tileData;
    private TileDataSO previousTileData;
    [SerializeField] private TileDataSO[] tileDatas;
    private bool inHasteMode;
    public TileDataSO Data => tileData;

    private void Awake()
    {
        _animation = GetComponent<TileAnimation>();
        BeatManager.instance.OnClapPlayed += HandleClapPlayed;
    }

    private void HandleClapPlayed(float obj)
    {
        if(inHasteMode) DeactivateTemporaryHaste();
    }

    void Start()
    {
        Invoke("ActivateTile", Random.Range(1f, 10f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetStartingTile()
    {
        
    }

    public void ActivateTile()
    {
        if (Activated) return;
        tileData = tileDatas[Random.Range(0, tileDatas.Length)];
        Activated = true;
        
        _animation.StartActivateAnim(tileData);

    }
    
    public void UpdateTile(TileDataSO newData)
    {
        if (!inHasteMode) return;
        tileData = newData;
        Activated = true;
        
        _animation.StartUpdateTileAnim(tileData);

    }

    public void CompletedTile()
    {
        GameManager.instance.CompletedTile();
        _animation.StartCompletedAnim(tileData, ResetTile);
        //Debug.Log("TILE SUCCESS");
    }

    public void TileFailed()
    {
        // Handle 
        _animation.StartDeactivatedAnim(tileData, ResetTile);
        GameManager.instance.MissedTile();
    }

    public void ResetTile()
    {
        Activated = false;
        Invoke("ActivateTile", Random.Range(1f, 10f));
        //Debug.Log("RESET!");
    }

    public void ApplyEffect()
    {
        switch (Data.effect)
        {
            case TileEffect.None:
                break;
            case TileEffect.HastenClap:
                BeatManager.instance.HastenNextClap();
                break;
            case TileEffect.SkipClap:
                BeatManager.instance.SkipNextClap();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void ActivateTemporaryHaste()
    {
        previousTileData = tileData;
        inHasteMode = true;
        UpdateTile(tileDatas[2]);
    }

    public void DeactivateTemporaryHaste()
    {
        UpdateTile(previousTileData);
        inHasteMode = false;
    }
}

public enum TileType
{
    None,
    Simple,
    Double,
    DoubleSkipBeat
}
