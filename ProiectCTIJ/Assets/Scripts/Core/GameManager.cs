using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Economy")]
    public float currentGold = 100f;
    public float currentXP = 0f;
    public int currentTier = 1;
    public float goldPerSecond = 3f;

    [Header("Game State")]
    public bool isGameOver = false;
    public bool isPaused = false;

    public event Action OnGoldChanged;
    public event Action OnXPChanged;
    public event Action OnTierChanged;
    public event Action<bool> OnGameOver;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isGameOver || isPaused) return;

        UpdateGold(goldPerSecond * Time.deltaTime);
    }

    public void UpdateGold(float amount)
    {
        currentGold += amount;
        OnGoldChanged?.Invoke();
    }

    public void UpdateXP(float amount)
    {
        currentXP += amount;
        CheckTierUpgrade();
        OnXPChanged?.Invoke();
    }

    private void CheckTierUpgrade()
    {
        // Simple tier logic based on the pitch
        if (currentTier == 1 && currentXP >= 50) UpgradeTier();
        else if (currentTier == 2 && currentXP >= 125) UpgradeTier();
        else if (currentTier == 3 && currentXP >= 250) UpgradeTier();
    }

    private void UpgradeTier()
    {
        currentTier++;
        goldPerSecond = 3f + 0.25f * (currentTier - 1);
        OnTierChanged?.Invoke();
        Debug.Log($"Reached Tech Tier {currentTier}!");
    }

    public void EndGame(bool playerWon)
    {
        isGameOver = true;
        OnGameOver?.Invoke(playerWon);
        Time.timeScale = 0f;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }
}
