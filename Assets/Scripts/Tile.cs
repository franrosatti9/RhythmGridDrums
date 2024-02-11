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
    [SerializeField] private PossibleTilesListSO possibleTileDatas;
    private TileDataSO previousTileData;
    private float setActivatedTime;
    private bool inHasteMode;
    public TileDataSO Data => tileData;

    private void Awake()
    {
        _animation = GetComponent<TileAnimation>();
        BeatManager.instance.OnClapPlayed += HandleClapPlayed;
        Player.instance.OnPlayerMoved += HandlePlayerMoved;
        GameManager.instance.OnMiss += HandlePlayerMissed;
        
        // To set tile to Activated when half the transition is made
        setActivatedTime = _animation.UpdateTransitionTime;
        tileData = possibleTileDatas.GetDeactivatedData();
    }

    private void OnDisable()
    {
        BeatManager.instance.OnClapPlayed -= HandleClapPlayed;
        Player.instance.OnPlayerMoved -= HandlePlayerMoved;
        GameManager.instance.OnMiss -= HandlePlayerMissed;
    }

    private void HandlePlayerMoved(Tile newTile)
    {
        if (newTile == this) return;
        
        if(inHasteMode) DeactivateTemporaryHaste();
    }

    private void HandlePlayerMissed()
    {
        if(inHasteMode) DeactivateTemporaryHaste();
    }

    private void HandleClapPlayed(float obj)
    {
        //if(inHasteMode) DeactivateTemporaryHaste();
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

        // TODO: Make this random initialization better

        tileData = possibleTileDatas.GetRandomData();
        
        Invoke(nameof(SetActivatedTrue), setActivatedTime);
        Activated = true;
        
        _animation.StartActivateAnim(tileData);

    }
    
    public void UpdateTile(TileDataSO newData)
    {
        if (!inHasteMode) return;

        if (newData == possibleTileDatas.GetDeactivatedData())
        {
            _animation.StartDeactivatedAnim(tileData);
            ResetTile();
        }
        else
        {
            tileData = newData;
            Activated = true;
            _animation.StartUpdateTileAnim(tileData);
            
        }
        

    }

    public void CompletedTile()
    {
        if (!Activated) return;
        _animation.StartCompletedAnim(tileData);
        ResetTile();
        GameManager.instance.CompletedTile();
        
        //Debug.Log("TILE SUCCESS");
    }

    public void TileFailed()
    {
        if (!Activated) return;
        _animation.StartDeactivatedAnim(tileData);
        ResetTile();
        GameManager.instance.MissedTile();
    }

    public void ResetTile()
    {
        tileData = possibleTileDatas.GetDeactivatedData();
        Activated = false;
        Invoke(nameof(ActivateTile), Random.Range(2f, 10f));
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
    
    public void TransformSurroundingTiles()
    {
        GetNeighbourTile(Vector3.forward);
        GetNeighbourTile(Vector3.left);
        GetNeighbourTile(Vector3.right);
        GetNeighbourTile(Vector3.back);

        inHasteMode = false;
    }
    
    void GetNeighbourTile(Vector3 dir)
    {
        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, 1f))
        {
            var tile = hit.collider.GetComponentInParent<Tile>();
            if (tile)
            {
                //Debug.Log("hit " + tile.name + " in " + tile.transform.position);
                tile.ActivateTemporaryHaste();
            }
        }
    }

    public void ActivateTemporaryHaste()
    {
        if (inHasteMode) return;
        previousTileData = tileData;
        inHasteMode = true;
        UpdateTile(possibleTileDatas.GetHasteTileData());
    }

    public void DeactivateTemporaryHaste()
    {
        //Debug.Log("Deactivate Haste!");
        UpdateTile(previousTileData);
        inHasteMode = false;
    }

    void SetActivatedTrue()
    {
        Activated = true;
    }
}

public enum TileType
{
    None,
    Simple,
    Double,
    DoubleSkipBeat
}
