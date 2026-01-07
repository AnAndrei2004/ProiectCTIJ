using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bară de HP care urmărește o unitate.
/// Atașează pe un Canvas World Space cu un Image (fill) pentru viață.
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("References")]
    public Image fillImage;           // Imaginea care se umple (slider)
    public Image backgroundImage;     // Fundal (opțional)
    
    [Header("Colors")]
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    
    [Header("Settings")]
    public Vector3 offset = new Vector3(0f, 1.2f, 0f);  // Offset deasupra unității
    public bool hideWhenFull = true;  // Ascunde bara când HP e plin
    
    private Unit targetUnit;
    private Transform targetTransform;
    private Camera mainCamera;
    private float maxHP;  // HP maxim salvat când se setează ținta
    private bool maxHPInitialized = false;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Încearcă să găsească unitatea părinte
        if (targetUnit == null)
        {
            targetUnit = GetComponentInParent<Unit>();
            if (targetUnit != null)
            {
                targetTransform = targetUnit.transform;
            }
        }
        
        // NU setăm maxHP aici - așteptăm primul Update când currentHP e deja setat corect
    }
    
    void LateUpdate()
    {
        if (targetUnit == null || targetTransform == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // Poziționează bara deasupra unității
        transform.position = targetTransform.position + offset;
        
        // Face bara să fie mereu orientată spre cameră (billboard)
        if (mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }
        
        UpdateBar();
    }
    
    void UpdateBar()
    {
        if (targetUnit == null || fillImage == null) return;
        
        // Inițializează maxHP o singură dată, când currentHP e deja setat (după ApplyRolePreset)
        if (!maxHPInitialized && targetUnit.currentHP > 0)
        {
            maxHP = targetUnit.currentHP;  // La start, currentHP == maxHP
            maxHPInitialized = true;
            fillImage.fillAmount = 1f;  // Forțează bara plină
            return;
        }
        
        if (maxHP <= 0) return;  // Încă nu e inițializat
        
        // Calculează procentul corect
        float currentHealth = Mathf.Max(0f, targetUnit.currentHP);
        float healthPercent = Mathf.Clamp01(currentHealth / maxHP);
        
        // Actualizează fill amount DIRECT (fără smooth pentru precizie)
        fillImage.fillAmount = healthPercent;
        
        // Schimbă culoarea în funcție de HP
        if (healthPercent > 0.6f)
        {
            fillImage.color = fullHealthColor;
        }
        else if (healthPercent > 0.3f)
        {
            fillImage.color = midHealthColor;
        }
        else
        {
            fillImage.color = lowHealthColor;
        }
        
        // Ascunde când e plin (opțional)
        if (hideWhenFull)
        {
            bool shouldShow = healthPercent < 0.99f;
            if (fillImage.gameObject.activeSelf != shouldShow)
            {
                fillImage.gameObject.SetActive(shouldShow);
                if (backgroundImage != null)
                    backgroundImage.gameObject.SetActive(shouldShow);
            }
        }
    }
    
    /// <summary>
    /// Setează unitatea țintă pentru această bară de HP.
    /// </summary>
    public void SetTarget(Unit unit)
    {
        targetUnit = unit;
        maxHPInitialized = false;  // Reset pentru a lua maxHP corect la primul Update
        if (unit != null)
        {
            targetTransform = unit.transform;
        }
    }
}
