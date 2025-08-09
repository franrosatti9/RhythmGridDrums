using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class TutorialHandler : MonoBehaviour
{
    [SerializeField] private TutorialLevelGenerator generator;
    [SerializeField] private TutorialStep[] tutorialSteps;
    [SerializeField] private Player player;
    [SerializeField] ScoreController scoreController;
    [SerializeField] private TextMeshProUGUI descriptionText;
    private CanvasGroup descriptionCanvasGroup;
    private int consecutiveTiles;
    private int stepIndex = 0;
    private bool tutorialCompleted;
    public TutorialStep CurrentStep => tutorialSteps[stepIndex];
    public Tile CurrentStepTile { get; private set; }

    private void Awake()
    {
        descriptionCanvasGroup = descriptionText.GetComponent<CanvasGroup>();
    }

    void Start()
    {
        Player.instance.OnPlayerMoved += OnPlayerMovedHandler;
        scoreController.OnComboUpdated += OnComboUpdateHandler;
        generator.GenerateStarting();
        
        descriptionText.text = CurrentStep.description;
    }
    
    private void OnDestroy()
    {
        Player.instance.OnPlayerMoved -= OnPlayerMovedHandler;
    }

    private void OnPlayerMovedHandler(Tile newTile)
    {
        // Avoid player moving back to tile
        newTile.SetCanMoveTo(false);
        var tile = generator.GenerateNext(CurrentStep);
        
        // If a tile expected to be completed is generated, listen to its completed event
        if (tile != null && tile.Data.effect != TileEffect.HastenClap)
        {
            // Unsuscribe from previous step tile
            if (CurrentStepTile != null)
            {
                CurrentStepTile.OnTileCompleted -= OnStepTileCompletedHandler;
                Debug.LogWarning("Completed tile " + CurrentStepTile.gameObject.name);
            }
            
            CurrentStepTile = tile;
            CurrentStepTile.OnTileCompleted += OnStepTileCompletedHandler;
        }
    }
    
    private void OnHasteSuccessHandler()
    {
        NextStep();
        GameManager.instance.OnHasteCompleted -= OnHasteSuccessHandler;
    }
    
    private void OnComboUpdateHandler(int combo, float normalized)
    {
        Debug.LogWarning("Combo: " + combo);
        if (combo > 1)
        {
            NextStep();
            scoreController.OnComboUpdated -= OnComboUpdateHandler;
        }
    }

    private void OnStepTileCompletedHandler()
    { 
        // Completed normal or skip tile
        NextStep();
    }

    public void StartFreeMovementTutorial()
    {
        
    }

    public void CompleteTutorial()
    {
        tutorialCompleted = true;
    }

    public void NextStep()
    {
        CurrentStep.OnStepExit.Invoke();
        
        // Finished all steps
        if (stepIndex + 1 >= tutorialSteps.Length)
        {
            CompleteTutorial();
            Debug.LogWarning("COMPLETED TUTORIAL");
            return;
        }
        
        stepIndex++;
        
        if(CurrentStep.tileData.effect == TileEffect.HastenClap) GameManager.instance.OnHasteCompleted += OnHasteSuccessHandler;
        
        AnimateDescriptionChange(CurrentStep.description);
        CurrentStep.OnStepReached.Invoke();
    }

    public void AnimateDescriptionChange(string text)
    {
        LeanTween.alphaCanvas(descriptionCanvasGroup, 0, .5f).setEaseOutCubic()
            .setOnComplete(() =>
            {
                descriptionText.text = text;
                LeanTween.alphaCanvas(descriptionCanvasGroup, 1, .5f).setEaseInCubic();
            });
    }
}

[System.Serializable]
public class TutorialStep
{
    public string description;
    public TileDataSO tileData;
    public UnityEvent OnStepReached = new UnityEvent();
    public UnityEvent OnStepExit = new UnityEvent(); 
}
