using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "ScriptableObjects/New Tile List", fileName = "NewTileList")]
public class PossibleTilesListSO : ScriptableObject
{
    [SerializeField] private List<DataWithChance> datas;
    [SerializeField] private TileDataSO hasteData;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public TileDataSO GetRandomData()
    {
        TileDataSO result = datas[0].data;
        var totalWeight = 0;

        foreach (var data in datas)
        {
            totalWeight += data.weight;
        }

        var rdm = Random.Range(0, totalWeight);

        foreach (var data in datas)
        {
            var weight = data.weight;

            if (rdm >= weight)
            {
                rdm -= weight;
            }
            else
            {
                result = data.data;
                break;
            }
        }

        return result;
    }

    public TileDataSO GetHasteTileData()
    {
        return hasteData;
    }
}

[Serializable]
public class DataWithChance
{
    public TileDataSO data;
    public int weight;
}
