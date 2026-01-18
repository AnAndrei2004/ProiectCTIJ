using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bara de HP care urmareste o unitate.
/// Ataseaza pe un Canvas World Space cu un Image (fill) pentru viata.
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("References")]
    public Image fillImage;           // Imaginea care se umple (slider)
    public Image backgroundImage;     // Fundal (optional)
    
    [Header("Colors")]
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    
    [Header("Settings")]
    public Vector3 offset = new Vector3(0f, 1.2f, 0f);  // Offset deasupra unitatii
    public bool hideWhenFull = true;  // Ascunde bara cand HP e plin
    
    private Unit targetUnit;
    private Transform targetTransform;
    private Camera mainCamera;
    private float maxHP;  // HP maxim salvat cand se seteaza tinta
    private bool maxHPInitialized = false;

    // Initializeaza referintele pentru bara.
    void Start()
    {
        mainCamera = Camera.main;
        
        // Incearca sa gaseasca unitatea parinte
        if (targetUnit == null)
        {
            targetUnit = GetComponentInParent<Unit>();
            if (targetUnit != null)
            {
                targetTransform = targetUnit.transform;
            }
        }
        
        // NU setam maxHP aici - asteptam primul Update cand currentHP e deja setat corect
    }

    // Actualizeaza pozitia si afisajul barei.
    void LateUpdate()
    {
        if (targetUnit == null || targetTransform == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // Pozitioneaza bara deasupra unitatii
        transform.position = targetTransform.position + offset;
        
        // Face bara sa fie orientata spre camera (billboard)
        if (mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }
        
        UpdateBar();
    }

    // Actualizeaza umplerea si culorile barei.
    void UpdateBar()
    {
        if (targetUnit == null || fillImage == null) return;
        
        // Initializeaza maxHP o singura data, cand currentHP e deja setat (dupa ApplyRolePreset)
        if (!maxHPInitialized && targetUnit.currentHP > 0)
        {
            maxHP = targetUnit.currentHP;  // La start, currentHP == maxHP
            maxHPInitialized = true;
            fillImage.fillAmount = 1f;  // Forteaza bara plina
            return;
        }
        
        if (maxHP <= 0) return;  // Inca nu e initializat
        
        // Calculeaza procentul corect
        float currentHealth = Mathf.Max(0f, targetUnit.currentHP);
        float healthPercent = Mathf.Clamp01(currentHealth / maxHP);
        
        // Actualizeaza fill amount DIRECT (fara smooth pentru precizie)
        fillImage.fillAmount = healthPercent;
        
        // Schimba culoarea in functie de HP
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
        
        // Ascunde cand e plin (optional)
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
    /// Seteaza unitatea tinta pentru aceasta bara de HP.
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
