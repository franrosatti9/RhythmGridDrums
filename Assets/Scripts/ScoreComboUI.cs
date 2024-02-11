using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreComboUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Color[] multiplierColors;
    [SerializeField] private Image progressBar;
    [SerializeField] private ScoreController scoreController;

    private void OnEnable()
    {
        GameManager.instance.OnGameStarted += EnableCombo;
        
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void EnableCombo()
    {
        gameObject.SetActive(true);
        scoreController.OnScoreUpdated += UpdateScore;
        scoreController.OnComboUpdated += UpdateCombo;
    }

    void Hide()
    {
        gameObject.SetActive(false);
        scoreController.OnScoreUpdated -= UpdateScore;
        scoreController.OnComboUpdated -= UpdateCombo;
    }

    void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }

    void UpdateCombo(int currentCombo, float progress)
    {
        UpdateColors(currentCombo);
        
        multiplierText.text = "x" + currentCombo;
        if (currentCombo == 5) progress = 1f;
        
        // TODO: Add Lerp to progress fill
        progressBar.fillAmount = progress;

    }

    void UpdateColors(int combo)
    {
        // Change color of everything (add more stuff like a bar) to the current current combo color
        multiplierText.color = multiplierColors[combo - 1];
        progressBar.color = multiplierColors[combo - 1];
    }

    // TODO: Animate bar 
    IEnumerator AnimateProgressBar()
    {
        yield return null;
    }
    
}
