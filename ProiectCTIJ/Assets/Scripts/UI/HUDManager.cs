using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public Slider playerBaseHPBar;
    public Slider enemyBaseHPBar;

    // Initializeaza HUD si se aboneaza la schimbari de gold.
    private void Start()
    {
        UpdateUI();
        GameManager.Instance.OnGoldChanged += UpdateUI;
    }

    // Actualizeaza elementele HUD.
    private void UpdateUI()
    {
        if (goldText) goldText.text = $"Gold: {Mathf.FloorToInt(GameManager.Instance.currentGold)}";
    }
}
