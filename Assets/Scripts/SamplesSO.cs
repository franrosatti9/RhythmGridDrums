using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/New Sample Set", fileName = "NewSampleSet")]
public class SamplesSO : ScriptableObject
{
    public AudioClip kick;
    public AudioClip clap;
    // Maybe implement hiHats integrating the song when on combo
    public AudioClip hiHat;
    public AudioClip[] backgroundTrackParts;
    public AudioClip[] backgroundSection1;
    public AudioClip[] backgroundSection2;
    public AudioClip[] backgroundSection3;
    public AudioClip[] backgroundSection4;
    public AudioClip finishImpact;
    

    public AudioClip GetRandomBackgroundPart()
    {
        return backgroundTrackParts[Random.Range(0, backgroundTrackParts.Length)];
    }
    
    public AudioClip GetRandomBackgroundPart(int beat)
    {
        switch (beat)
        {
            case 0:
                return backgroundSection1[Random.Range(0, backgroundSection1.Length)];
                break;
            case 1:
                return backgroundSection2[Random.Range(0, backgroundSection2.Length)];
                break;
            case 2:
                return backgroundSection3[Random.Range(0, backgroundSection3.Length)];
                break;
            case 3:
                return backgroundSection4[Random.Range(0, backgroundSection4.Length)];
                break;
            default:
                return null;
        }
    }
}
