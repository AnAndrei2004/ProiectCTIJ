using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UnitSpawnerUI : MonoBehaviour
{
    [Header("Unit Buttons")]
    public Button[] unitButtons = new Button[3];
    public Image[] unitIcons = new Image[3];
    public TextMeshProUGUI[] unitCostTexts = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] unitNameTexts = new TextMeshProUGUI[3];
    
    [Header("Cooldown Display")]
    public TextMeshProUGUI spawnCooldownText;
    
    [Header("Unit Data")]
    public Sprite[] unitHeadSprites = new Sprite[3]; // Iconite pentru unitati
    
    private float lastSpawnTime = -999f;
    private List<UnitData> unitsData = new List<UnitData>();

    // Structura pentru informatiile unitatii
    private class UnitData
    {
        public string name;
        public int cost;
        public string id;
    }

    // Initializeaza butoanele si textele UI pentru unitati.
    private void Start()
    {
        // Incarca datele unitatilor din JSON
        LoadUnitData();
        
        // Conecteaza butoanele cu functii de spawn
        for (int i = 0; i < unitButtons.Length; i++)
        {
            int index = i; // Variabila pentru closure
            if (unitButtons[i] != null)
            {
                unitButtons[i].onClick.AddListener(() => OnUnitButtonClicked(index));
            }
            
            // Actualizeaza textele cu informatii despre unitate
            if (i < unitsData.Count)
            {
                if (unitNameTexts[i] != null)
                {
                    unitNameTexts[i].text = unitsData[i].name;
                }
                if (unitCostTexts[i] != null)
                {
                    unitCostTexts[i].text = $"${unitsData[i].cost}";
                }
                if (unitIcons[i] != null && unitHeadSprites[i] != null)
                {
                    unitIcons[i].sprite = unitHeadSprites[i];
                }
            }
        }
    }

    // Incarca datele unitatilor din fisierul JSON
    private void LoadUnitData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("units");
        if (jsonFile == null)
        {
            jsonFile = Resources.Load<TextAsset>("Assets/units");
        }
        
        if (jsonFile != null)
        {
            UnitArray unitArray = JsonUtility.FromJson<UnitArray>("{\"units\":" + jsonFile.text + "}");
            foreach (var unit in unitArray.units)
            {
                unitsData.Add(new UnitData
                {
                    name = unit.name,
                    cost = unit.cost,
                    id = unit.id
                });
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
        if (index >= 0 && index < unitHeadSprites.Length)
        {
            unitHeadSprites[index] = sprite;
        }
    }

    // Clasa auxiliara pentru deserializarea JSON
    [System.Serializable]
    private class Unit
    {
        public string id;
        public string name;
        public int cost;
        public int hp;
        public int damage;
        public float attackRate;
        public int speed;
        public int range;
        public string description;
    }

    [System.Serializable]
    private class UnitArray
    {
        public Unit[] units;
    }
}
