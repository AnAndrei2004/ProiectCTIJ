using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitSpawnerUI : MonoBehaviour
{
    [Header("Unit Buttons")]
    public Button[] unitButtons = new Button[3];
    public Image[] unitIcons = new Image[3];
    public TextMeshProUGUI[] unitCostTexts = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] unitNameTexts = new TextMeshProUGUI[3];
    
    [Header("Cooldown Display")]
    public TextMeshProUGUI spawnCooldownText;
    
    private float lastSpawnTime = -999f;

    // Initializeaza butoanele si textele UI pentru unitati.
    private void Start()
    {
        // Conecteaza butoanele cu functii de spawn
        for (int i = 0; i < unitButtons.Length; i++)
        {
            int index = i; // Variabila pentru closure
            if (unitButtons[i] != null)
            {
                unitButtons[i].onClick.AddListener(() => OnUnitButtonClicked(index));
            }
            
            // Actualizeaza textele cu informatii despre unitate
            if (unitNameTexts[i] != null && UnitSpawner.Instance != null)
            {
                unitNameTexts[i].text = UnitSpawner.Instance.GetPlayerUnitName(i);
            }
            if (unitCostTexts[i] != null && UnitSpawner.Instance != null)
            {
                unitCostTexts[i].text = $"${UnitSpawner.Instance.GetPlayerUnitCost(i)}";
            }
        }
    }

    // Afiseaza cooldown si seteaza interactiunea butoanelor.
    private void Update()
    {
        // Afiseaza cooldown-ul
        if (spawnCooldownText != null && UnitSpawner.Instance != null)
        {
            float timeSinceLastSpawn = Time.time - lastSpawnTime;
            if (timeSinceLastSpawn < UnitSpawner.Instance.spawnCooldown)
            {
                float remainingTime = UnitSpawner.Instance.spawnCooldown - timeSinceLastSpawn;
                spawnCooldownText.text = $"Cooldown: {remainingTime:F1}s";
                spawnCooldownText.gameObject.SetActive(true);
                
                // Dezactiveaza butoanele in cooldown
                foreach (Button btn in unitButtons)
                {
                    if (btn != null) btn.interactable = false;
                }
            }
            else
            {
                spawnCooldownText.gameObject.SetActive(false);
                
                // Reactiveaza butoanele
                foreach (Button btn in unitButtons)
                {
                    if (btn != null) btn.interactable = true;
                }
            }
        }
    }

    // Handler pentru click pe butonul de unitate.
    private void OnUnitButtonClicked(int unitIndex)
    {
        if (UnitSpawner.Instance != null)
        {
            UnitSpawner.Instance.SpawnPlayerUnit(unitIndex);
            lastSpawnTime = Time.time;
        }
    }

    // Seteaza iconita pentru o unitate in UI.
    public void SetUnitIcon(int index, Sprite sprite)
    {
        if (index >= 0 && index < unitIcons.Length && unitIcons[index] != null)
        {
            unitIcons[index].sprite = sprite;
        }
    }
}
