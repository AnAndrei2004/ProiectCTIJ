using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI tierText;
    public Slider playerBaseHPBar;
    public Slider enemyBaseHPBar;

    private void Start()
    {
        UpdateUI();
        GameManager.Instance.OnGoldChanged += UpdateUI;
        GameManager.Instance.OnXPChanged += UpdateUI;
        GameManager.Instance.OnTierChanged += UpdateUI;
    }

    private void UpdateUI()
    {
        if (goldText) goldText.text = $"Gold: {Mathf.FloorToInt(GameManager.Instance.currentGold)}";
        if (xpText) xpText.text = $"XP: {Mathf.FloorToInt(GameManager.Instance.currentXP)}";
        if (tierText) tierText.text = $"Tier: {GameManager.Instance.currentTier}";
    }

    public void UpdateBaseHP(float playerHP, float enemyHP)
    {
        if (playerBaseHPBar) playerBaseHPBar.value = playerHP / 2000f;
        if (enemyBaseHPBar) enemyBaseHPBar.value = enemyHP / 300f;
    }
}
