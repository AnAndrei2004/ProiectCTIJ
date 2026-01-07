using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bară de HP specială pentru baze - afișată fix pe ecran (Screen Space).
/// </summary>
public class BaseHealthBar : MonoBehaviour
{
    [Header("References")]
    public BaseUnit targetBase;
    public Image fillImage;
    public Image backgroundImage;
    public TMPro.TextMeshProUGUI hpText;  // Opțional: afișează "1500 / 2000"
    
    [Header("Colors")]
    public Color fullHealthColor = new Color(0.2f, 0.8f, 0.2f);
    public Color midHealthColor = new Color(0.9f, 0.7f, 0.1f);
    public Color lowHealthColor = new Color(0.9f, 0.2f, 0.2f);
    public Color criticalHealthColor = new Color(0.5f, 0f, 0f);
    
    [Header("Thresholds")]
    [Range(0f, 1f)] public float lowHealthThreshold = 0.3f;
    [Range(0f, 1f)] public float criticalHealthThreshold = 0.15f;
    
    [Header("Which Base")]
    [Tooltip("True = bara pentru baza Player, False = bara pentru baza Enemy")]
    public bool isForPlayerBase = true;
    
    private float lastHP;
    private float maxHP;
    
    void Start()
    {
        // Găsește baza automat dacă nu e setată
        if (targetBase == null)
        {
            FindTargetBase();
        }
        
        if (targetBase != null)
        {
            lastHP = targetBase.currentHP;
            maxHP = targetBase.hp;
            if (maxHP <= 0) maxHP = 2000f;  // Default pentru baze
        }
        
        UpdateBar();
    }
    
    void FindTargetBase()
    {
        BaseUnit[] bases = FindObjectsByType<BaseUnit>(FindObjectsSortMode.None);
        
        foreach (var b in bases)
        {
            // Găsește baza corectă în funcție de isForPlayerBase
            if (b.isPlayerBase == isForPlayerBase)
            {
                targetBase = b;
                maxHP = b.hp;
                if (maxHP <= 0) maxHP = 2000f;
                Debug.Log($"[BaseHealthBar] {gameObject.name} -> {b.gameObject.name} (HP: {b.currentHP}/{maxHP})");
                break;
            }
        }
    }
    
    void Update()
    {
        // Re-try finding base if not set
        if (targetBase == null)
        {
            FindTargetBase();
            if (targetBase == null) return;
        }
        
        UpdateBar();
    }
    
    void UpdateBar()
    {
        if (targetBase == null || fillImage == null) return;
        
        // Asigură-te că maxHP e setat corect
        if (maxHP <= 0)
        {
            maxHP = targetBase.hp;
            if (maxHP <= 0) maxHP = 2000f;
        }
        
        float currentHealth = Mathf.Max(0f, targetBase.currentHP);
        float healthPercent = Mathf.Clamp01(currentHealth / maxHP);
        
        // Smooth fill
        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, healthPercent, Time.deltaTime * 8f);
        
        // Culoare în funcție de HP
        if (healthPercent > 0.6f)
        {
            fillImage.color = fullHealthColor;
        }
        else if (healthPercent > lowHealthThreshold)
        {
            fillImage.color = midHealthColor;
        }
        else if (healthPercent > criticalHealthThreshold)
        {
            fillImage.color = lowHealthColor;
        }
        else
        {
            fillImage.color = criticalHealthColor;
        }
        
        // Actualizează textul HP
        if (hpText != null)
        {
            hpText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHP)}";
        }
        
        lastHP = targetBase.currentHP;
    }
}
