using UnityEngine;
using TMPro;

/// <summary>
/// Afișează gold-ul jucătorului în UI.
/// Atașează pe un GameObject cu TextMeshProUGUI.
/// </summary>
public class GoldDisplay : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Textul care afișează gold-ul. Dacă nu e setat, caută automat pe acest GameObject.")]
    public TextMeshProUGUI goldText;
    
    [Header("Display Settings")]
    [Tooltip("Prefixul afișat înainte de număr (ex: 'Gold: ')")]
    public string prefix = "Gold: ";
    [Tooltip("Sufixul afișat după număr (ex: ' G')")]
    public string suffix = "";
    [Tooltip("Afișează și iconița de gold")]
    public bool showIcon = true;
    public string iconPrefix = "💰 ";  // Emoji sau lasă gol
    
    [Header("Animation")]
    [Tooltip("Animează schimbarea gold-ului")]
    public bool animateChanges = true;
    public float animationSpeed = 5f;
    public Color gainColor = Color.green;
    public Color loseColor = Color.red;
    public Color normalColor = Color.white;
    
    private int displayedGold;
    private int targetGold;
    private float colorTimer;
    
    void Start()
    {
        // Găsește textul automat dacă nu e setat
        if (goldText == null)
        {
            goldText = GetComponent<TextMeshProUGUI>();
        }
        
        // Inițializează cu gold-ul curent
        if (GameManager.Instance != null)
        {
            displayedGold = Mathf.FloorToInt(GameManager.Instance.currentGold);
            targetGold = displayedGold;
        }
        
        UpdateDisplay();
    }
    
    void Update()
    {
        if (GameManager.Instance == null || goldText == null) return;
        
        int currentGold = Mathf.FloorToInt(GameManager.Instance.currentGold);
        
        // Detectează schimbări
        if (currentGold != targetGold)
        {
            int diff = currentGold - targetGold;
            targetGold = currentGold;
            
            // Schimbă culoarea temporar
            if (animateChanges)
            {
                goldText.color = diff > 0 ? gainColor : loseColor;
                colorTimer = 0.5f;
            }
        }
        
        // Animează numărul
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
    
    void UpdateDisplay()
    {
        if (goldText == null) return;
        
        string icon = showIcon ? iconPrefix : "";
        goldText.text = $"{icon}{prefix}{displayedGold}{suffix}";
    }
}
