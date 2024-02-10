using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    private float xInput;
    private float yInput;

    private bool canMove = true;
    private float lastClapBeat = 0;
    [SerializeField] private float movementRate = 1f;
    [SerializeField] private float moveTime = 0.5f;
    [SerializeField] float rayOffsetDistance = 2f;

    private bool inHasteMode = false;

    private List<float> currentKickInput = new List<float>();

    public Queue<float> kicksToPlay = new();

    public Tile currentTile;

    public event Action<Tile> OnPlayerMoved;

    public static Player instance;
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

    private void OnEnable()
    {
        BeatManager.instance.OnClapPlayed += HandleClapPlayed;
        GameManager.instance.OnMiss += HandlePlayerMissed;
    }
    
    private void OnDisable()
    {
        BeatManager.instance.OnClapPlayed -= HandleClapPlayed;
        GameManager.instance.OnMiss -= HandlePlayerMissed;
    }

    private void HandleClapPlayed(float beat)
    {
        // Save when the last clap played, to compare input with current tile required inputs
        lastClapBeat = beat;
    }

    // Update is called once per frame
    void Update()
    {
        if(!GameManager.instance.IsStarted) return;
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AudioManager.instance.PlayKick();
            CheckTileMistake();
        }
        
        if(!canMove) return;
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        
        if (xInput != 0)
        {
            Physics.Raycast(transform.position + new Vector3(xInput * rayOffsetDistance, 1, 0),
                Vector3.down, out RaycastHit hit, 2f);
            
            
            if (hit.collider != null && hit.collider.transform.root.TryGetComponent(out Tile newTile))
            {
                MovePlayer(newTile);
            }
        }
        else if (yInput != 0)
        {
            Physics.Raycast(transform.position + new Vector3(0, 1, yInput * rayOffsetDistance),
                Vector3.down, out RaycastHit hit, 2f);

            if (hit.collider != null && hit.collider.transform.root.TryGetComponent(out Tile newTile))
            {
                MovePlayer(newTile);
            }
        }
    }

    public void CheckTileMistake()
    {
        if(currentTile == null || !currentTile.Activated) return;
        // Calculate when kick was played between claps (from 0 to 2)
        float kickBeatPlayed = BeatManager.instance.CurrentBeatPosition - lastClapBeat;
        currentKickInput.Add(kickBeatPlayed);
        
        // Check if last played kick is correct compared to required kicks on this tile
        float beatToCheck = currentTile.Data.requiredKickBeats[currentKickInput.Count - 1];
        
        if (Mathf.Abs(beatToCheck - kickBeatPlayed) > 0.2f)
        {
            // Failed to complete tile
            FailAndClearCurrentTile();
        }
        else
        {
        }
    }

    public void CheckTileCompletion()
    {
        // If input amount is different than required, failed
        if (currentTile.Data.requiredKickBeats.Length != currentKickInput.Count)
        {
            FailAndClearCurrentTile();
            return;
        }
        
        // Check every input was correct, return if not
        for (int i = 0; i < currentKickInput.Count; i++)
        {
            if (Mathf.Abs(currentKickInput[i] - currentTile.Data.requiredKickBeats[i]) > 0.2f)
            {
                FailAndClearCurrentTile();
                return;
            }
        }
        
        // Tile was completed if all inputs were correct

        currentTile.CompletedTile();
        return;
    }

    public void ChangeCurrentTile(Tile newTile, bool correctMovement)
    {
        // Reset current input 
        currentKickInput.Clear();
        if (newTile.Activated) currentTile = newTile;
        else currentTile = null;
        
        //TODO: Fix bug when moving too fast between tiles, try to ignore next
        
        // Deactivate both if moved incorrectly to new tile
        // Apply tile effect if moved correctly (like skip or hasten next clap)
        if (currentTile != null)
        {
            if (correctMovement)
            {
                if (!inHasteMode) currentTile.ApplyEffect();

                if (currentTile.Data.effect == TileEffect.HastenClap && !inHasteMode)
                {
                    inHasteMode = true;
                    currentTile.TransformSurroundingTiles();
                    return;
                }
            }
            else FailAndClearCurrentTile();
        }

        if (inHasteMode) inHasteMode = false;
    }
    public void MovePlayer(Tile newTile)
    {
        // Check if player moved in the right moment, then check if the tile was completed and replace with new tile
        bool correctMovement = BeatManager.instance.CheckPlayerBeatMovement();
        
        
        if(currentTile != null && currentTile.Activated) CheckTileCompletion();
        OnPlayerMoved?.Invoke(newTile);
        ChangeCurrentTile(newTile, correctMovement);
        StartCoroutine(MoveToNextTile(newTile.transform.position));
    }
    IEnumerator MoveToNextTile(Vector3 target)
    {
        canMove = false;

        Vector3 initialPos = transform.position;

        float elapsedTime = 0;
        
        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(initialPos, target, elapsedTime / moveTime);
            yield return null;
        }

        // Finish lerp
        transform.position = target;
        //if(inHasteMode) TransformSurroundingTiles();

        yield return new WaitForSeconds(movementRate - moveTime);

        canMove = true;
        
    }


    private void HandlePlayerMissed()
    {
        // TODO: Rework this, maybe handle everything on GameManager and deactivate from there
        if (IsOnTile()) FailAndClearCurrentTile();
    }

    private void FailAndClearCurrentTile()
    {
        currentTile.TileFailed();
        currentTile = null;
    }

    public bool IsOnTile() => currentTile != null;
    
    
}
