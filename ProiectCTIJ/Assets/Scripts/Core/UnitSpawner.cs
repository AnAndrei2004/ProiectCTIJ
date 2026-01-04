using UnityEngine;
using System.Collections.Generic;

public class UnitSpawner : MonoBehaviour
{
    public static UnitSpawner Instance { get; private set; }

    public GameObject unitPrefab; // Generic prefab that gets initialized with UnitData
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;

    [Header("Spawn Settings")]
    public float spawnCooldown = 0.5f; // Cooldown între spawnuri
    private float lastSpawnTime = -999f;

    private List<UnitData> unitDataList = new List<UnitData>();

    private void Awake()
    {
        Instance = this;
        LoadUnitData();
    }

    private void LoadUnitData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("units");
        if (jsonFile != null)
        {
            // Note: Simple JSON parsing might need a wrapper or a library like Newtonsoft
            // For MVP, we'll assume we can manually populate or use a simple wrapper
            // UnitDataList data = JsonUtility.FromJson<UnitDataList>(jsonFile.text);
            // unitDataList.AddRange(data.units);
        }
        else
        {
            // Fallback hardcoded for MVP if JSON fails
            // Speed: unități Unity pe secundă (2-3 e normal pentru un joc 2D)
            // Range: distanța de atac în unități Unity (1-3 pentru melee, 5+ pentru ranged)
            unitDataList.Add(new UnitData { id = "soldier", cost = 25, hp = 90, damage = 10, attackRate = 1f, speed = 2f, range = 1.5f });
            unitDataList.Add(new UnitData { id = "tank", cost = 60, hp = 240, damage = 16, attackRate = 1.2f, speed = 1.5f, range = 1.5f });
            unitDataList.Add(new UnitData { id = "archer", cost = 45, hp = 70, damage = 8, attackRate = 0.8f, speed = 2f, range = 5f });
        }
    }

    public void SpawnUnit(int unitIndex, Team team)
    {
        if (unitIndex < 0 || unitIndex >= unitDataList.Count) return;

        // Verifică cooldown pentru player
        if (team == Team.Player && Time.time - lastSpawnTime < spawnCooldown)
        {
            Debug.Log("Spawn în cooldown!");
            return;
        }

        UnitData data = unitDataList[unitIndex];

        if (team == Team.Player)
        {
            if (GameManager.Instance.currentGold >= data.cost)
            {
                GameManager.Instance.UpdateGold(-data.cost);
                CreateUnitInstance(data, team, playerSpawnPoint.position);
                lastSpawnTime = Time.time;
            }
        }
        else
        {
            CreateUnitInstance(data, team, enemySpawnPoint.position);
        }
    }

    private void CreateUnitInstance(UnitData data, Team team, Vector3 position)
    {
        Vector3 spawnPos = new Vector3(position.x, -2.44f, position.z);
        GameObject go = Instantiate(unitPrefab, spawnPos, Quaternion.identity);
        Unit unit = go.GetComponent<Unit>();
        unit.Initialize(data, team);
    }
}
