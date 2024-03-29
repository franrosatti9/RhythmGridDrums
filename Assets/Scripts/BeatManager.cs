using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Linq;
using System.Xml;

public class BeatManager : MonoBehaviour
{
    float songPosition = 0;
    float songPosInBeats;
    float secPerBeat;
    float dspTimeSong;
    private int nextClapBeat;
    private int nextHihatBeat;
    private int expectedBeatToMove;
    
    [SerializeField] private float bpm;
    [SerializeField] private int gameLengthInBeats;


    [SerializeField] private float beat;

    private LinkedList<int> clapsList = new();

    public float CurrentBeatPosition => songPosInBeats;

    public event Action<float> OnClapPlayed;
    public event Action OnHasteRealised;
    public event Action OnEveryBeat;

    public static BeatManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        // Initialize all Claps list, starting from beat 2
        for (int i = 2; i < gameLengthInBeats; i += 2)
        {
            clapsList.AddLast(i);
        }
    }

    private void OnEnable()
    {
        GameManager.instance.OnGameStarted += GameStart;
    }

    private void Start()
    {
        secPerBeat = 60f / bpm;
        nextClapBeat = 2;
        nextHihatBeat = 0;
        expectedBeatToMove = nextClapBeat;
    }
    
    void Update()
    {
        if (!GameManager.instance.IsStarted) return;

        songPosition += Time.deltaTime;
        //songPosition = (float)(AudioSettings.dspTime - dspTimeSong); // position in seconds

        songPosInBeats = songPosition / secPerBeat; // position in beats

        if (songPosInBeats >= nextHihatBeat)
        {
            OnEveryBeat?.Invoke();
            nextHihatBeat++;
        }

        if (songPosInBeats >= nextClapBeat)
        {
            Debug.Log("Played Clap: " + songPosInBeats);
            // Event with beat played information
            OnClapPlayed?.Invoke(nextClapBeat);
            
            // "Dequeue" clap and now check for next one

            if (clapsList.Count > 0)
            {
                clapsList.RemoveFirst();
                if (clapsList.First == null)
                {
                    GameManager.instance.FinishGame();
                    return;
                }

                nextClapBeat = clapsList.First.Value;
            }

        }

        // Player didn't move after the clap, missed
        if (songPosInBeats > expectedBeatToMove + GameManager.instance.beatErrorMargin)
        {
            Debug.Log("Passed");
            GameManager.instance.MissedMovement();
            expectedBeatToMove = nextClapBeat;
        }


    }

    public void GameStart()
    {
        //GetComponent<AudioSource>()?.Play();
        dspTimeSong = (float)AudioSettings.dspTime;
    }

    public bool CheckPlayerBeatMovement()
    {
        // No claps to check after next one
        if (GetClapAfterNext() < 0) return false;

        // If the difference between the beat in which the player moved and the clap is
        // less than the margin of error, the movement was correct
        float beatDelta = Mathf.Abs(songPosInBeats - expectedBeatToMove);
        Debug.Log("Player Moved: " + (songPosInBeats - expectedBeatToMove));
        //Debug.Log("Expected to Move: " + expectedBeatToMove);

        if (beatDelta <= GameManager.instance.beatErrorMargin)
        {
            GameManager.instance.PlayerMoved(beatDelta);

            // If between margin of error, check if moved before or after required clap to update accordingly
            if (songPosInBeats > expectedBeatToMove)
            {
                expectedBeatToMove = nextClapBeat;

            }
            else
            {
                expectedBeatToMove = GetClapAfterNext();


            }

            return true;
        }

        // Player moved too early
        if (songPosInBeats < expectedBeatToMove && expectedBeatToMove - songPosInBeats < 1.5f)
        {
            Debug.Log("Skipped Clap");
            GameManager.instance.MissedMovement();
            expectedBeatToMove = GetClapAfterNext();

            return false;
        }

        return false;


    }

    public int GetClapAfterNext()
    {
        if (clapsList.First.Next != null)
        {
            return clapsList.First.Next.Value;
        }

        return -1;
    }

    public void HastenNextClap()
    {
        // Check if moved before or after current clap
        bool exceptFirst = nextClapBeat < expectedBeatToMove;
        AnticipateAllClaps(exceptFirst);
        Debug.Log("HASTEN");
        
        OnHasteRealised?.Invoke();
        
        nextClapBeat = clapsList.First.Value;
        expectedBeatToMove = nextClapBeat;
        
        // TODO: Fix bug where Missed raises on Update when moving before current clap
    }
    

    public void SkipNextClap()
    {
        if(clapsList.Contains(expectedBeatToMove)) clapsList.Remove(expectedBeatToMove);
        nextClapBeat = clapsList.First.Value;
        
        expectedBeatToMove = nextClapBeat;
    }

    // TODO: Figure out a simpler way to do this
    public void AnticipateAllClaps(bool exceptFirst)
    {
        LinkedList<int> newList = new();
        foreach (int clap in clapsList)
        {
            if (exceptFirst && newList.Count == 0)
            {
                newList.AddLast(clap);
                continue;
            }
            
            newList.AddLast(clap - 1);
        }

        clapsList = newList;
    }
}
