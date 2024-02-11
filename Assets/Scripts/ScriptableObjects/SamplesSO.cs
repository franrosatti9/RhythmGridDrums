using System.Collections;
using System.Collections.Generic;
using UnityEditor.TerrainTools;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/New Sample Set", fileName = "NewSampleSet")]
public class SamplesSO : ScriptableObject
{
    public float bpm;
    public int lengthInBeats;
    public AudioClip music;
    public AudioClip kick;
    public AudioClip clap;
    // Maybe implement hiHats integrating the song when on combo
    public AudioClip hiHat;
    
    public AudioClip finishImpact;
    public AudioClip finishMusic;
}
