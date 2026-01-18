using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ataseaza acest script pe un GameObject gol in scena.
/// Va crea automat bare de HP pentru toate unitatile.
/// </summary>
public class HealthBarManager : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Prefab pentru bara de HP a unitatilor (World Space Canvas)")]
    public GameObject unitHealthBarPrefab;
    
    [Header("Settings")]
    public Vector3 unitBarOffset = new Vector3(0f, 0.8f, 0f);
    public Vector3 unitBarScale = new Vector3(0.01f, 0.01f, 1f);
    
    [Header("Base Health Bar References")]
    [Tooltip("Referinta la bara de HP a bazei jucatorului (in UI)")]
    public BaseHealthBar playerBaseHealthBar;
    [Tooltip("Referinta la bara de HP a bazei inamice (in UI)")]
    public BaseHealthBar enemyBaseHealthBar;
    
    public static HealthBarManager Instance { get; private set; }

    // Initializeaza singletonul managerului.
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

    // Initializeaza referintele pentru barele bazelor.
    void Start()
    {
        // Gaseste bazele si le atribuie barele de HP din UI
        AssignBaseBars();
    }

    // Asociaza barele de HP cu bazele din scena.
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
    /// Creeaza o bara de HP pentru o unitate.
    /// Apeleaza asta cand spawnezi o unitate noua.
    /// </summary>
    public void CreateHealthBar(Unit unit)
    {
        if (unit == null || unitHealthBarPrefab == null) return;
        
        // Nu crea bare pentru baze (au bara lor separata in UI)
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
