using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Economy")]
    public float currentGold = 100f;
    public float goldPerSecond = 3f;

    [Header("Game State")]
    public bool isGameOver = false;
    public bool isPaused = false;

    public event Action OnGoldChanged;
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

    /// <summary>
    /// Adaugă recompensă pentru uciderea unui inamic
    /// </summary>
    public void AddKillReward(int reward)
    {
        currentGold += reward;
        Debug.Log($"+{reward} aur pentru uciderea inamicului! Total: {currentGold}");
        OnGoldChanged?.Invoke();
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
