using UnityEngine;

public class AISpawner : MonoBehaviour
{
    public float initialBudget = 150f;
    public float budgetIncreaseRate = 0.15f; // 15% pe minut
    
    private float currentBudget = 0f;
    private float timer = 0f;
    private float spawnCooldown = 1.5f;
    private float lastSpawnTime = 0f;

    // Initializeaza bugetul AI.
    private void Start()
    {
        currentBudget = initialBudget;
    }

    // Update pentru buget si spawn AI.
    private void Update()
    {
        if (GameManager.Instance.isGameOver) return;

        timer += Time.deltaTime;
        
        // Creste bugetul in timp
        if (timer >= 60f)
        {
            currentBudget *= (1f + budgetIncreaseRate);
            timer = 0f;
        }

        // Logica de spawn AI
        if (Time.time >= lastSpawnTime + spawnCooldown)
        {
            TrySpawnEnemy();
        }
    }

    // Spawneaza un inamic pe baza unei distributii simple.
    private void TrySpawnEnemy()
    {
        // AI simplu: 70% Soldier, 20% Archer, 10% Tank
        float rand = Random.value;
        int unitIndex = 0;

        if (rand < 0.7f) unitIndex = 0; // Soldier
        else if (rand < 0.9f) unitIndex = 2; // Archer
        else unitIndex = 1; // Tank

        // In joc real am verifica gold-ul AI; aici folosim doar cooldown
        UnitSpawner.Instance.SpawnUnit(unitIndex, Team.Enemy);
        lastSpawnTime = Time.time;
        
        // Randomizeaza timpul pentru urmatorul spawn
        spawnCooldown = Random.Range(1.0f, 3.0f);
    }
}
