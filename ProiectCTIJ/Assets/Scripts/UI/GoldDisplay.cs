using UnityEngine;
using TMPro;

/// <summary>
/// Afiseaza gold-ul jucatorului in UI.
/// Ataseaza pe un GameObject cu TextMeshProUGUI.
/// </summary>
public class GoldDisplay : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Textul care afiseaza gold-ul. Daca nu e setat, cauta automat pe acest GameObject.")]
    public TextMeshProUGUI goldText;
    
    [Header("Display Settings")]
    [Tooltip("Prefixul afisat inainte de numar (ex: 'Gold: ')")]
    public string prefix = "Gold: ";
    [Tooltip("Sufixul afisat dupa numar (ex: ' G')")]
    public string suffix = "";
    [Tooltip("Afiseaza si iconita de gold")]
    public bool showIcon = true;
    public string iconPrefix = "💰 ";  // Emoji sau lasa gol
    
    [Header("Animation")]
    [Tooltip("Animeaza schimbarea gold-ului")]
    public bool animateChanges = true;
    public float animationSpeed = 5f;
    public Color gainColor = Color.green;
    public Color loseColor = Color.red;
    public Color normalColor = Color.white;
    
    private int displayedGold;
    private int targetGold;
    private float colorTimer;
    
    // Initializeaza referinta la text si valorile initiale.
    void Start()
    {
        // Gaseste textul automat daca nu e setat
        if (goldText == null)
        {
            goldText = GetComponent<TextMeshProUGUI>();
        }
        
        // Initializeaza cu gold-ul curent
        if (GameManager.Instance != null)
        {
            displayedGold = Mathf.FloorToInt(GameManager.Instance.currentGold);
            targetGold = displayedGold;
        }
        
        UpdateDisplay();
    }

    // Actualizeaza animatia si textul de gold.
    void Update()
    {
        if (GameManager.Instance == null || goldText == null) return;
        
        int currentGold = Mathf.FloorToInt(GameManager.Instance.currentGold);
        
        // Detecteaza schimbari
        if (currentGold != targetGold)
        {
            int diff = currentGold - targetGold;
            targetGold = currentGold;
            
            // Schimba culoarea temporar
            if (animateChanges)
            {
                goldText.color = diff > 0 ? gainColor : loseColor;
                colorTimer = 0.5f;
            }
        }
        
        // Animeaza numarul
        if (animateChanges && displayedGold != targetGold)
        {
            float step = animationSpeed * Mathf.Abs(targetGold - displayedGold) * Time.deltaTime;
            if (displayedGold < targetGold)
            {
                displayedGold = Mathf.Min(targetGold, displayedGold + Mathf.CeilToInt(step));
            }
            else
            {
                displayedGold = Mathf.Max(targetGold, displayedGold - Mathf.CeilToInt(step));
            }
        }
        else
        {
            displayedGold = targetGold;
        }
        
        // Reset culoare
        if (colorTimer > 0)
        {
            colorTimer -= Time.deltaTime;
            if (colorTimer <= 0)
            {
                goldText.color = normalColor;
            }
        }
        
        UpdateDisplay();
    }

    // Compune textul final pentru afisare.
    void UpdateDisplay()
    {
        if (goldText == null) return;
        
        string icon = showIcon ? iconPrefix : "";
        goldText.text = $"{icon}{prefix}{displayedGold}{suffix}";
    }
}
