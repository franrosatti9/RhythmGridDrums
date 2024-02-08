using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public float beatErrorMargin = .1f;
    private Tile _centerTile;
    public bool IsStarted { get; private set; }
    public event Action OnGameStarted;
    public event Action OnGameFinished;
    public event Action OnMiss;
    public event Action<int> OnTileSuccess;
    public static GameManager instance;
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
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace) && !IsStarted)
        {
            IsStarted = true;
            OnGameStarted?.Invoke();
        }
        
    }

    public void InitPlayer(Tile centerTile)
    {
        _centerTile = centerTile;
        
    }

    public void PlayerMoved(float beatDelta)
    {
        OnTileSuccess?.Invoke(100);
        Debug.Log("BIEN CAPO");
        
    }

    public void MissedMovement()
    {
        OnMiss?.Invoke();
        Debug.Log("Missed");
    }

    public void CompletedTile()
    {
        OnTileSuccess?.Invoke(500);
    }
    
    public void MissedTile()
    {
        OnMiss?.Invoke();
    }

    public void FinishGame()
    {
        IsStarted = false;
        OnGameFinished?.Invoke();
        
    }

    public void TransformToHasteTiles(List<Tile> tiles)
    {
        
    }
}
