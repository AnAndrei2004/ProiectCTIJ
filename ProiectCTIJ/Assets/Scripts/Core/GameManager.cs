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
        if (isGameOver) return;
        isGameOver = true;
        OnGameOver?.Invoke(playerWon);
        GameOverUI gameOverUI = FindObjectOfType<GameOverUI>(true);
        if (gameOverUI != null)
        {
            gameOverUI.Show(playerWon);
        }
        Time.timeScale = 0f;
    }

    // Termina jocul dupa un delay (foloseste timp real, independent de Time.timeScale).
    public void EndGameAfterDelay(float delaySeconds, bool playerWon)
    {
        if (isGameOver) return;
        StartCoroutine(EndGameAfterDelayRoutine(delaySeconds, playerWon));
    }

    private System.Collections.IEnumerator EndGameAfterDelayRoutine(float delaySeconds, bool playerWon)
    {
        yield return new WaitForSecondsRealtime(delaySeconds);
        EndGame(playerWon);
    }

    // Comuta pauza jocului.
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }
}
