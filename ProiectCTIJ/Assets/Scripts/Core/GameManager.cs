using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Economy")]
    public float currentGold = 200f;
    public float goldPerSecond = 3f;

    [Header("Game State")]
    public bool isGameOver = false;
    public bool isPaused = false;

    public event Action OnGoldChanged;
    public event Action<bool> OnGameOver;

    // Initializeaza singletonul GameManager.
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

    // Update pentru economie si stare joc.
    private void Update()
    {
        if (isGameOver || isPaused) return;

        UpdateGold(goldPerSecond * Time.deltaTime);
    }

    // Modifica gold-ul curent si notifica UI.
    public void UpdateGold(float amount)
    {
        currentGold += amount;
        OnGoldChanged?.Invoke();
    }

    /// <summary>
    /// Adauga recompensa pentru uciderea unui inamic
    /// </summary>
    public void AddKillReward(int reward)
    {
        currentGold += reward;
        Debug.Log($"+{reward} aur pentru uciderea inamicului! Total: {currentGold}");
        OnGoldChanged?.Invoke();
    }

    // Marcheaza sfarsitul jocului si opreste timpul.
    public void EndGame(bool playerWon)
    {
        isGameOver = true;
        OnGameOver?.Invoke(playerWon);
        Time.timeScale = 0f;
    }

    // Comuta pauza jocului.
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }
}
