using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    
    [SerializeField] private int size;
    [SerializeField] private Tile tile;
    [SerializeField] private Camera cam;
    
    
    void Start()
    {
        // Should always be and odd number
        if (size % 2 == 0) size++;
        
        GenerateLevel();
        SetupCamera();
    }

    void GenerateLevel()
    {
        int offset = Mathf.FloorToInt(size / 2f);
        
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                Tile newTile = Instantiate(tile, new Vector3Int(x - offset, 0, z - offset), Quaternion.identity);

                if (tile.transform.position.x - offset == 0)
                {
                    tile.SetStartingTile();
                }
            }
        }
    }

    void SetupCamera()
    {
        cam.orthographicSize = size / 2f;
        
    }
}
