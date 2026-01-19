using UnityEngine;

public class AISpawner : MonoBehaviour
{
    public float initialBudget = 150f;
    public float budgetIncreaseRate = 0.15f;
    public float baseIncomePerSecond = 3f;
    
    private float currentBudget = 0f;
    private float timer = 0f;
    private float spawnCooldown = 2f;
    private float lastSpawnTime = -999f;
    private float currentIncomePerSecond = 0f;
    private int spawnStep = 0;

    private void Start()
    {
        currentBudget = initialBudget;
        currentIncomePerSecond = baseIncomePerSecond;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        timer += Time.deltaTime;
        currentBudget += currentIncomePerSecond * Time.deltaTime;
        
        if (timer >= 60f)
        {
            currentIncomePerSecond *= (1f + budgetIncreaseRate);
            timer = 0f;
        }

        if (Time.time >= lastSpawnTime + spawnCooldown)
        {
            TrySpawnEnemy();
        }
    }

    private void TrySpawnEnemy()
    {
        if (UnitSpawner.Instance == null || UnitSpawner.Instance.enemyPrefabs == null || UnitSpawner.Instance.enemyPrefabs.Length == 0)
            return;

        EnemyRole desiredRole = GetDesiredRole();
        int unitIndex = FindBestAffordableByRole(desiredRole);
        if (unitIndex < 0)
        {
            unitIndex = FindMostExpensiveAffordable();
        }

        if (unitIndex < 0)
            return;

        int cost = GetCostForPrefab(UnitSpawner.Instance.enemyPrefabs[unitIndex]);
        if (cost > currentBudget)
            return;

        UnitSpawner.Instance.SpawnUnit(unitIndex, Team.Enemy);
        currentBudget -= cost;
        lastSpawnTime = Time.time;
        spawnCooldown = GetCooldownForBudget();
        spawnStep++;
    }

    private EnemyRole GetDesiredRole()
    {
        switch (spawnStep % 4)
        {
            case 0:
                return EnemyRole.Heavy;
            case 1:
                return EnemyRole.Ranged;
            default:
                return EnemyRole.Light;
        }
    }

    private int FindBestAffordableByRole(EnemyRole role)
    {
        int bestIndex = -1;
        int bestCost = -1;

        for (int i = 0; i < UnitSpawner.Instance.enemyPrefabs.Length; i++)
        {
            GameObject prefab = UnitSpawner.Instance.enemyPrefabs[i];
            if (prefab == null) continue;

            if (GetRoleForPrefab(prefab) != role) continue;

            int cost = GetCostForPrefab(prefab);
            if (cost <= currentBudget && cost > bestCost)
            {
                bestCost = cost;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private int FindMostExpensiveAffordable()
    {
        int bestIndex = -1;
        int bestCost = -1;

        for (int i = 0; i < UnitSpawner.Instance.enemyPrefabs.Length; i++)
        {
            GameObject prefab = UnitSpawner.Instance.enemyPrefabs[i];
            if (prefab == null) continue;

            int cost = GetCostForPrefab(prefab);
            if (cost <= currentBudget && cost > bestCost)
            {
                bestCost = cost;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private float GetCooldownForBudget()
    {
        if (currentBudget >= 150f) return 1.5f;
        if (currentBudget >= 80f) return 2.0f;
        return 2.5f;
    }

    private EnemyRole GetRoleForPrefab(GameObject prefab)
    {
        if (prefab == null) return EnemyRole.Light;
        string name = prefab.name;

        if (name.Contains("Soldier") || name.Contains("Knight") || name.Contains("Merchant"))
            return EnemyRole.Heavy;

        if (name.Contains("Priest"))
            return EnemyRole.Ranged;

        return EnemyRole.Light;
    }

    private int GetCostForPrefab(GameObject prefab)
    {
        if (prefab == null) return 0;
        string name = prefab.name;

        if (name.Contains("Soldier") || name.Contains("Knight"))
            return 50;

        if (name.Contains("Thief") || name.Contains("Merchant"))
            return 20;

        if (name.Contains("Peasant") || name.Contains("Priest"))
            return 30;

        return 25;
    }

    private enum EnemyRole { Heavy, Light, Ranged }
}
