using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    private int comboMultiplier = 1;
    private int score;
    private float consecutiveScore = 0;

    [SerializeField] private int requiredConsecutive;
    public event Action<int> OnScoreUpdated; 
    
    // Event with current multiplier and normalized progress
    public event Action<int, float> OnComboUpdated; 
    private void OnEnable()
    {
        GameManager.instance.OnMiss += LoseCombo;
        GameManager.instance.OnTileSuccess += AddScore;
    }

    void Start()
    {
        UpdateCombo();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddScore(int amount)
    {
        score += amount * comboMultiplier;
        OnScoreUpdated?.Invoke(score);
        
        consecutiveScore++;
        UpdateCombo();
    }

    public void LoseCombo()
    {
        if(consecutiveScore == 0) return;
        // Reset Combo and Consecutive score count
        comboMultiplier = 1;
        consecutiveScore = 0;
        
        // Reset UI progress
        OnComboUpdated?.Invoke(comboMultiplier, 0);
    }

    void UpdateCombo()
    {
        // Every 5 consecutive correct tiles, increase multiplier by 1, then limit number
        float combo = Mathf.Clamp((consecutiveScore / requiredConsecutive) + 1, 1, 5);
        comboMultiplier = Mathf.FloorToInt(combo);

        // Send current multiplier and normalized combo progress
        OnComboUpdated?.Invoke(comboMultiplier, combo - comboMultiplier);
    }
    
    private void OnDisable()
    {
        GameManager.instance.OnMiss -= LoseCombo;
        GameManager.instance.OnTileSuccess -= AddScore;
    }
}
