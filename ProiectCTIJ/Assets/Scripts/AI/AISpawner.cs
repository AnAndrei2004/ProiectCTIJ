using UnityEngine;

public class AISpawner : MonoBehaviour
{
    public float initialBudget = 150f;
    public float budgetIncreaseRate = 0.15f; // 15% per minute
    
    private float currentBudget = 0f;
    private float timer = 0f;
    private float spawnCooldown = 1.5f;
    private float lastSpawnTime = 0f;

    private void Start()
    {
        currentBudget = initialBudget;
    }

    private void Update()
    {
        if (GameManager.Instance.isGameOver) return;

        timer += Time.deltaTime;
        
        // Increase budget over time
        if (timer >= 60f)
        {
            currentBudget *= (1f + budgetIncreaseRate);
            timer = 0f;
        }

        // AI Spawning logic
        if (Time.time >= lastSpawnTime + spawnCooldown)
        {
            TrySpawnEnemy();
        }
    }

    private void TrySpawnEnemy()
    {
        // Simple AI: 70% Soldier, 20% Archer, 10% Tank
        float rand = Random.value;
        int unitIndex = 0;

        if (rand < 0.7f) unitIndex = 0; // Soldier
        else if (rand < 0.9f) unitIndex = 2; // Archer
        else unitIndex = 1; // Tank

        // In a real game, we'd check if AI has "gold" or just use a timer-based budget
        // For MVP, we'll just spawn based on the cooldown
        UnitSpawner.Instance.SpawnUnit(unitIndex, Team.Enemy);
        lastSpawnTime = Time.time;
        
        // Randomize next spawn time slightly
        spawnCooldown = Random.Range(1.0f, 3.0f);
    }
}
