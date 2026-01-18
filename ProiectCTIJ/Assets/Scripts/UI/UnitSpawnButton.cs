using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Componenta pentru un buton de spawn de unitate cu cap/icona si pret.
/// Se ataseaza pe un GameObject cu Button, Image si TextMeshProUGUI.
/// </summary>
public class UnitSpawnButton : MonoBehaviour
{
    [Header("References")]
    public Image headImage;           // Imaginea capului/iconitei unitatii
    public TextMeshProUGUI priceText; // Textul cu pretul
    public TextMeshProUGUI nameText;  // Textul cu numele unitatii
    public Button spawnButton;        // Butonul de spawn
    
    [Header("Visual Settings")]
    public Color affordableColor = Color.white;
    public Color notAffordableColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
    
    private int unitIndex = -1;
    private int unitCost = 0;
    private string unitName = "";

    private void Start()
    {
        if (spawnButton == null)
            spawnButton = GetComponent<Button>();
            
        if (spawnButton != null)
        {
            spawnButton.onClick.AddListener(OnSpawnClicked);
        }
        
        // Monitorizeaza schimbarile de aur pentru a actualiza culoarea
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged += UpdateAffordability;
        }
    }

    /// <summary>
    /// Configureaza acest buton cu informatiile unitatii.
    /// </summary>
    public void SetupUnit(int index, string name, int cost, Sprite headSprite)
    {
        unitIndex = index;
        unitName = name;
        unitCost = cost;
        
        if (nameText != null)
            nameText.text = name;
            
        if (priceText != null)
            priceText.text = $"${cost}";
            
        if (headImage != null)
            headImage.sprite = headSprite;
            
        UpdateAffordability();
    }

    /// <summary>
    /// Actualizeaza starea butonului in functie de aur disponibil.
    /// </summary>
    private void UpdateAffordability()
    {
        if (GameManager.Instance != null && headImage != null)
        {
            bool canAfford = GameManager.Instance.currentGold >= unitCost;
            headImage.color = canAfford ? affordableColor : notAffordableColor;
            
            if (spawnButton != null)
                spawnButton.interactable = canAfford;
        }
    }

    /// <summary>
    /// Se apeleaza cand butonul este apasat.
    /// </summary>
    private void OnSpawnClicked()
    {
        if (UnitSpawner.Instance != null && unitIndex >= 0)
        {
            UnitSpawner.Instance.SpawnPlayerUnit(unitIndex);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged -= UpdateAffordability;
        }
    }
}
