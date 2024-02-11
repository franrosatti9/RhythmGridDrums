using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "ScriptableObjects/New Tile Data", fileName = "NewTileData")]
public class TileDataSO : ScriptableObject
{
   public string tileName;
   public Sprite sprite;
   public float[] requiredKickBeats;
   public TileEffect effect;

   public Material activatedColor;
   public Material completedFlashColor;

   public Material deactivatedColor;
   //public Sprite doubleSkipBeatSprite;

}

public enum TileEffect
{
   None,
   HastenClap,
   SkipClap
   
}
