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
        
        UpdateBar();
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
        
        float healthPercent = Mathf.Clamp01(targetUnit.currentHP / targetUnit.hp);
        
        // Actualizează fill amount
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
        if (unit != null)
        {
            targetTransform = unit.transform;
        }
    }
}
