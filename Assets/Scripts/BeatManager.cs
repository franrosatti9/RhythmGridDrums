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
    private int nextBeat;
    private int expectedBeatToMove;
    
    [SerializeField] private float bpm;
    [SerializeField] private int gameLengthInBeats;


    [SerializeField] private float beat;

    [SerializeField] private AudioSource bgMusicSource;
    
    private LinkedList<int> clapsList = new();

    public float CurrentBeatPosition => songPosInBeats;

    public float SecondsPerBeat => secPerBeat;

    public event Action<float> OnClapPlayed;
    public event Action<bool> OnHasteRealised;
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
        for (int i = 1; i < gameLengthInBeats; i += 2)
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
        nextClapBeat = 1;
        nextBeat = 0;
        expectedBeatToMove = nextClapBeat;
    }
    
    void Update()
    {
        if (!GameManager.instance.IsStarted) return;

        if (Input.GetKey(KeyCode.A))
        {
            bgMusicSource.time = bgMusicSource.time - 0.1f;
        }
        //songPosition += Time.deltaTime;
        songPosition = (float)(AudioSettings.dspTime - dspTimeSong); // position in seconds

        songPosInBeats = songPosition / secPerBeat; // position in beats

        if (songPosInBeats >= nextBeat)
        {
            OnEveryBeat?.Invoke();
            nextBeat++;
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
        bgMusicSource.Play();
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
        // Check if moved before current clap
        if (nextClapBeat < expectedBeatToMove)
        {
            clapsList.AddAfter(clapsList.First, nextClapBeat + 1);
            expectedBeatToMove = nextClapBeat + 1;
            // TODO: Rework so instead of an event with a bool, it tells AudioManager correctly independently of before/after
            OnHasteRealised?.Invoke(false);
        }
        else
        {
            clapsList.AddFirst(clapsList.First.Value - 1);
            
            nextClapBeat = clapsList.First.Value;
            expectedBeatToMove = nextClapBeat;
            OnHasteRealised?.Invoke(true);
        }
        
        //bool exceptFirst = nextClapBeat < expectedBeatToMove;
        //AnticipateAllClaps(exceptFirst);
        Debug.Log("HASTEN");
        
        

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
