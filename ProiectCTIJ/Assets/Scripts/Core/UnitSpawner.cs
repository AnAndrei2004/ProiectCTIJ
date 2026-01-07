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
    public float enemySpawnInterval = 6f;
    private float nextEnemySpawnTime = 2f;

    private int enemySpawnStep = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // AI simplu pentru spawn inamici
        if (Time.time >= nextEnemySpawnTime && GameManager.Instance != null && !GameManager.Instance.isGameOver)
        {
            SpawnStrategicEnemyUnit();

            // Slow, slightly randomized pacing (more room for strategy)
            float interval = Mathf.Max(enemySpawnInterval, 5f);
            nextEnemySpawnTime = Time.time + Random.Range(interval * 0.9f, interval * 1.25f);
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
        int cost = GetCostForPrefab(prefab);

        if (GameManager.Instance.currentGold >= cost)
        {
            GameManager.Instance.UpdateGold(-cost);
            CreateUnitInstance(prefab, Team.Player);
            lastSpawnTime = Time.time;
            Debug.Log($"Spawned {prefab.name} (Cost: {cost})");
        }
        else
        {
            Debug.Log($"Nu ai suficient aur! Ai nevoie de {cost}, ai {GameManager.Instance.currentGold}");
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

    private void SpawnStrategicEnemyUnit()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // Simple wave pattern: Heavy -> Ranged -> Light -> Light (repeat)
        EnemyRole desiredRole;
        switch (enemySpawnStep % 4)
        {
            case 0:
                desiredRole = EnemyRole.Heavy;
                break;
            case 1:
                desiredRole = EnemyRole.Ranged;
                break;
            default:
                desiredRole = EnemyRole.Light;
                break;
        }

        int index = FindEnemyPrefabIndex(desiredRole);
        if (index < 0)
        {
            // Fallback: random if role not present in enemyPrefabs array.
            index = Random.Range(0, enemyPrefabs.Length);
        }

        SpawnEnemyUnit(index);
        enemySpawnStep++;
    }

    private enum EnemyRole { Heavy, Light, Ranged }

    private int FindEnemyPrefabIndex(EnemyRole role)
    {
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            GameObject prefab = enemyPrefabs[i];
            if (prefab == null) continue;

            string name = prefab.name;
            if (role == EnemyRole.Heavy)
            {
                if (name.Contains("Soldier") || name.Contains("Knight") || name.Contains("Merchant"))
                    return i;
            }
            else if (role == EnemyRole.Light)
            {
                if (name.Contains("Thief") || name.Contains("Peasant"))
                    return i;
            }
            else if (role == EnemyRole.Ranged)
            {
                if (name.Contains("Priest"))
                    return i;
            }
        }

        return -1;
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

            // Preserve prefab identity for presets/UI/logs.
            unit.unitName = prefab.name;
            go.name = $"{prefab.name} ({team})";
            
            // Creează bara de HP pentru această unitate
            if (HealthBarManager.Instance != null)
            {
                HealthBarManager.Instance.CreateHealthBar(unit);
            }
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
            return GetCostForPrefab(playerPrefabs[index]);
        }
        return 0;
    }

    public string GetPlayerUnitName(int index)
    {
        if (index >= 0 && index < playerPrefabs.Length)
        {
            return playerPrefabs[index] != null ? playerPrefabs[index].name : "Unknown";
        }
        return "Unknown";
    }

    private int GetCostForPrefab(GameObject prefab)
    {
        if (prefab == null) return 0;
        string name = prefab.name;

        // Heavy
        if (name.Contains("Soldier") || name.Contains("Knight") || name.Contains("Merchant"))
            return 35;

        // Light
        if (name.Contains("Thief") || name.Contains("Peasant"))
            return 20;

        // Ranged
        if (name.Contains("Priest"))
            return 30;

        // Fallback
        return 25;
    }
}
