using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Atașează acest script pe un GameObject gol în scenă.
/// Va crea automat bare de HP pentru toate unitățile.
/// </summary>
public class HealthBarManager : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Prefab pentru bara de HP a unităților (World Space Canvas)")]
    public GameObject unitHealthBarPrefab;
    
    [Header("Settings")]
    public Vector3 unitBarOffset = new Vector3(0f, 0.8f, 0f);
    public Vector3 unitBarScale = new Vector3(0.01f, 0.01f, 1f);
    
    [Header("Base Health Bar References")]
    [Tooltip("Referință la bara de HP a bazei jucătorului (în UI)")]
    public BaseHealthBar playerBaseHealthBar;
    [Tooltip("Referință la bara de HP a bazei inamice (în UI)")]
    public BaseHealthBar enemyBaseHealthBar;
    
    public static HealthBarManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Găsește bazele și le atribuie barele de HP din UI
        AssignBaseBars();
    }
    
    void AssignBaseBars()
    {
        BaseUnit[] bases = FindObjectsByType<BaseUnit>(FindObjectsSortMode.None);
        foreach (var baseUnit in bases)
        {
            if (baseUnit.isPlayerBase && playerBaseHealthBar != null)
            {
                playerBaseHealthBar.targetBase = baseUnit;
            }
            else if (!baseUnit.isPlayerBase && enemyBaseHealthBar != null)
            {
                enemyBaseHealthBar.targetBase = baseUnit;
            }
        }
    }
    
    /// <summary>
    /// Creează o bară de HP pentru o unitate.
    /// Apelează asta când spawn-ezi o unitate nouă.
    /// </summary>
    public void CreateHealthBar(Unit unit)
    {
        if (unit == null || unitHealthBarPrefab == null) return;
        
        // Nu crea bare pentru baze (au bara lor separată în UI)
        if (unit is BaseUnit) return;
        
        GameObject barObj = Instantiate(unitHealthBarPrefab, unit.transform);
        barObj.transform.localPosition = unitBarOffset;
        barObj.transform.localScale = unitBarScale;
        
        HealthBar healthBar = barObj.GetComponent<HealthBar>();
        if (healthBar != null)
        {
            healthBar.SetTarget(unit);
            healthBar.offset = unitBarOffset;
        }
    }
}
