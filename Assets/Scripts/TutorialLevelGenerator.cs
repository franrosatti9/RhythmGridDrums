using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLevelGenerator : MonoBehaviour
{
    [SerializeField] private int stepTileOffset;
    [SerializeField] private int initialTilesAmount;
    [SerializeField] private Tile tilePrefab;
    Queue<Tile> tileQueue = new Queue<Tile>();
    private int normalTilesCount;
    private Vector3 lastTilePos;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateStarting()
    {
        for (int z = 0; z < initialTilesAmount; z++)
        {
            var newTile = Instantiate(tilePrefab, new Vector3Int(0, 0, z), Quaternion.identity);
            lastTilePos = newTile.transform.position;
            tileQueue.Enqueue(newTile);
            newTile.autoActivate = false;
            // Deactivate first tile so can't get back
            if(z == 0) newTile.SetCanMoveTo(false);
        }
    }

    public Tile GenerateNext(TutorialStep step)
    {
        Vector3 pos = lastTilePos + Vector3.forward;
        var newTile = Instantiate(tilePrefab, pos + Vector3.down * 2, Quaternion.identity);
        newTile.transform.localScale = Vector3.zero;
        AnimateSpawnedTile(newTile);
        tileQueue.Enqueue(newTile);
        newTile.autoActivate = false;
        lastTilePos = pos;
        
        if (normalTilesCount < stepTileOffset)
        {
            normalTilesCount++;
            return null;
        }
        else
        {
            // Only activate if not a deactivated tile
            if(step.tileData.activatedColor != null) newTile.ActivateTileWithData(step.tileData);
            normalTilesCount = 0;
            return newTile;
        }
    }

    void AnimateSpawnedTile(Tile tile)
    {
        LeanTween.moveY(tile.gameObject, 0, .4f).setEaseInCubic();
        LeanTween.scale(tile.gameObject, Vector3.one, .3f).setEaseInCubic();
    }
}
