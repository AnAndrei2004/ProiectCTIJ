using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    public static UnitSpawner Instance { get; private set; }

    [Header("Player Prefabs")]
    public GameObject[] playerPrefabs; // Trage aici cele 3 prefab-uri pentru Player

    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs;  // Trage aici cele 3 prefab-uri pentru Enemy

    [Header("Spawn Points")]
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;

    [Header("Spawn Settings")]
    public float spawnCooldown = 0.5f;
    private float lastSpawnTime = -999f;

    [Header("Enemy AI Settings")]
    public float enemySpawnInterval = 3f;
    private float nextEnemySpawnTime = 2f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // AI simplu pentru spawn inamici
        if (Time.time >= nextEnemySpawnTime && GameManager.Instance != null && !GameManager.Instance.isGameOver)
        {
            SpawnRandomEnemyUnit();
            nextEnemySpawnTime = Time.time + enemySpawnInterval;
        }
    }

    /// <summary>
    /// Spawn o unitate player (0, 1 sau 2)
    /// </summary>
    public void SpawnPlayerUnit(int unitIndex)
    {
        if (unitIndex < 0 || unitIndex >= playerPrefabs.Length)
        {
            Debug.LogWarning($"Index invalid pentru player unit: {unitIndex}");
            return;
        }

        // Verifică cooldown
        if (Time.time - lastSpawnTime < spawnCooldown)
        {
            Debug.Log("Spawn în cooldown!");
            return;
        }

        GameObject prefab = playerPrefabs[unitIndex];
        Unit unitScript = prefab.GetComponent<Unit>();

        if (unitScript != null)
        {
            int cost = unitScript.cost;
            
            if (GameManager.Instance.currentGold >= cost)
            {
                GameManager.Instance.UpdateGold(-cost);
                CreateUnitInstance(prefab, Team.Player);
                lastSpawnTime = Time.time;
                Debug.Log($"Spawned {unitScript.unitName} (Cost: {cost})");
            }
            else
            {
                Debug.Log($"Nu ai suficient aur! Ai nevoie de {cost}, ai {GameManager.Instance.currentGold}");
            }
        }
        else
        {
            Debug.LogError($"Prefab-ul {prefab.name} nu are scriptul Unit atașat!");
        }
    }

    /// <summary>
    /// Spawn o unitate inamică (0, 1 sau 2)
    /// </summary>
    public void SpawnEnemyUnit(int unitIndex)
    {
        if (unitIndex < 0 || unitIndex >= enemyPrefabs.Length) return;
        
        CreateUnitInstance(enemyPrefabs[unitIndex], Team.Enemy);
    }

    /// <summary>
    /// Spawn o unitate inamică aleatorie (pentru AI)
    /// </summary>
    public void SpawnRandomEnemyUnit()
    {
        if (enemyPrefabs.Length == 0) return;
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        SpawnEnemyUnit(randomIndex);
    }

    private void CreateUnitInstance(GameObject prefab, Team team)
    {
        // Determină poziția de spawn în funcție de echipă
        Transform spawnPoint = (team == Team.Player) ? playerSpawnPoint : enemySpawnPoint;
        Vector3 spawnPos = new Vector3(spawnPoint.position.x, -4f, spawnPoint.position.z);
        
        GameObject go = Instantiate(prefab, spawnPos, Quaternion.identity);
        Unit unit = go.GetComponent<Unit>();
        
        if (unit != null)
        {
            unit.SetTeam(team); // Setează echipa și orientarea
            go.name = $"{unit.unitName} ({team})";
        }
    }

    /// <summary>
    /// Metodă de compatibilitate pentru scripturile vechi
    /// </summary>
    public void SpawnUnit(int unitIndex, Team team)
    {
        if (team == Team.Player)
            SpawnPlayerUnit(unitIndex);
        else
            SpawnEnemyUnit(unitIndex);
    }

    // Metode utile pentru UI
    public int GetPlayerUnitCost(int index)
    {
        if (index >= 0 && index < playerPrefabs.Length)
        {
            Unit u = playerPrefabs[index].GetComponent<Unit>();
            return (u != null) ? u.cost : 0;
        }
        return 0;
    }

    public string GetPlayerUnitName(int index)
    {
        if (index >= 0 && index < playerPrefabs.Length)
        {
            Unit u = playerPrefabs[index].GetComponent<Unit>();
            return (u != null) ? u.unitName : "Unknown";
        }
        return "Unknown";
    }
}
