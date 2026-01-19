using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelRoot;
    public TextMeshProUGUI titleText;
    public Button playAgainButton;
    public Button menuButton;

    [Header("Texts")]
    public string winTitle = "You have Won";
    public string loseTitle = "GAME OVER";

    [Header("Scenes")]
    [Tooltip("Daca e gol, se reincarca scena curenta.")]
    public string playAgainSceneName = "";
    [Tooltip("Numele scenei de meniu. Daca e gol, se foloseste menuSceneIndex.")]
    public string menuSceneName = "";
    public int menuSceneIndex = 0;

    private bool isShown = false;
    private bool isSubscribed = false;

    private void Awake()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveListener(HandlePlayAgain);
            playAgainButton.onClick.AddListener(HandlePlayAgain);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveListener(HandleMenu);
            menuButton.onClick.AddListener(HandleMenu);
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Start()
    {
        if (!TrySubscribe())
        {
            StartCoroutine(WaitForGameManager());
        }
    }

    private bool TrySubscribe()
    {
        if (isSubscribed) return true;
        if (GameManager.Instance == null) return false;

        GameManager.Instance.OnGameOver += HandleGameOver;
        isSubscribed = true;
        return true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed) return;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= HandleGameOver;
        }
        isSubscribed = false;
    }

    private System.Collections.IEnumerator WaitForGameManager()
    {
        while (GameManager.Instance == null)
        {
            yield return null;
        }

        TrySubscribe();
    }

    private void HandleGameOver(bool playerWon)
    {
        Show(playerWon);
    }

    public void Show(bool playerWon)
    {
        if (isShown) return;
        isShown = true;

        EnsureEventSystem();

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = playerWon ? winTitle : loseTitle;
        }

        // Re-register button listeners when panel becomes visible
        RegisterButtonListeners();
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif
    }

    private void RegisterButtonListeners()
    {
        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
            playAgainButton.onClick.AddListener(HandlePlayAgain);
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(HandleMenu);
        }
    }

    public void HandlePlayAgain()
    {
        Debug.Log("Play Again clicked!");
        
        // Destroy GameManager to reset state on new game
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }

        Time.timeScale = 1f;

        if (!string.IsNullOrWhiteSpace(playAgainSceneName))
        {
            SceneManager.LoadScene(playAgainSceneName);
            return;
        }

        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void HandleMenu()
    {
        Debug.Log("Menu clicked!");
        
        // Destroy GameManager to reset state when returning to menu
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }

        Time.timeScale = 1f;

        if (!string.IsNullOrWhiteSpace(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
            return;
        }

        SceneManager.LoadScene(menuSceneIndex);
    }
}
