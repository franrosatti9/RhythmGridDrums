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
        Player.instance.OnPlayerMoved += HandlePlayerMoved;
        GameManager.instance.OnMiss += HandlePlayerMissed;
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
        // TODO: Rework this, maybe handle everything on GameManager and deactivate from there
        if(Player.instance.currentTile == this) TileFailed();
        
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
        
        float rdm = Random.value;
        if (rdm < 0.45f)
        {
            tileData = tileDatas[0];
        }
        else if (rdm < 0.9f)
        {
            tileData = tileDatas[1];
        }
        else tileData = tileDatas[2];
        
        //tileData = tileDatas[Random.Range(0, tileDatas.Length)];
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
        if (!Activated) return;
        ResetTile();
        _animation.StartCompletedAnim(tileData, ResetTile);
        GameManager.instance.CompletedTile();
        
        //Debug.Log("TILE SUCCESS");
    }

    public void TileFailed()
    {
        if (!Activated) return;
        ResetTile();
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
    
    public void TransformSurroundingTiles()
    {
        GetNeighbourTile(Vector3.forward);
        GetNeighbourTile(Vector3.left);
        GetNeighbourTile(Vector3.right);
        GetNeighbourTile(Vector3.back);

        inHasteMode = false;
    }
    
    Tile GetNeighbourTile(Vector3 dir)
    {
        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, 1f))
        {
            var tile = hit.collider.GetComponentInParent<Tile>();
            if (tile)
            {
                Debug.Log("hit " + tile.name + " in " + tile.transform.position);
                tile.ActivateTemporaryHaste();
                return tile;
            }
        }

        return null;
    }

    public void ActivateTemporaryHaste()
    {
        if (inHasteMode) return;
        previousTileData = tileData;
        inHasteMode = true;
        UpdateTile(tileDatas[2]);
    }

    public void DeactivateTemporaryHaste()
    {
        //Debug.Log("Deactivate Haste!");
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
