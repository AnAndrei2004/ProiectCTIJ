using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public Slider playerBaseHPBar;
    public Slider enemyBaseHPBar;

    private void Start()
    {
        UpdateUI();
        GameManager.Instance.OnGoldChanged += UpdateUI;
    }

    private void UpdateUI()
    {
        if (goldText) goldText.text = $"Gold: {Mathf.FloorToInt(GameManager.Instance.currentGold)}";
    }

    public void UpdateBaseHP(float playerHP, float enemyHP)
    {
        if (playerBaseHPBar) playerBaseHPBar.value = playerHP / 2000f;
        if (enemyBaseHPBar) enemyBaseHPBar.value = enemyHP / 300f;
    }
}
