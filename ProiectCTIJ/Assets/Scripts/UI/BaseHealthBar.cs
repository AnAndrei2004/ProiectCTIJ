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
    
    private float lastHP;
    
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
        }
        
        UpdateBar();
    }
    
    void FindTargetBase()
    {
        // Caută în părinte
        targetBase = GetComponentInParent<BaseUnit>();
        
        // Sau caută după tag/nume
        if (targetBase == null)
        {
            BaseUnit[] bases = FindObjectsByType<BaseUnit>(FindObjectsSortMode.None);
            foreach (var b in bases)
            {
                // Poți personaliza logica aici pentru Player vs Enemy base
                targetBase = b;
                break;
            }
        }
    }
    
    void Update()
    {
        if (targetBase == null)
        {
            gameObject.SetActive(false);
            return;
        }
        
        UpdateBar();
    }
    
    void UpdateBar()
    {
        if (targetBase == null || fillImage == null) return;
        
        float healthPercent = Mathf.Clamp01(targetBase.currentHP / targetBase.hp);
        
        // Smooth fill (opțional)
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
            hpText.text = $"{Mathf.CeilToInt(targetBase.currentHP)} / {Mathf.CeilToInt(targetBase.hp)}";
        }
        
        lastHP = targetBase.currentHP;
    }
}
