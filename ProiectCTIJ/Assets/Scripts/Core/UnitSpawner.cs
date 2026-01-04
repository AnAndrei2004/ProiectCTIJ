using UnityEngine;
using System.Collections.Generic;

public class UnitSpawner : MonoBehaviour
{
    public static UnitSpawner Instance { get; private set; }

    public GameObject unitPrefab; // Generic prefab that gets initialized with UnitData
    public Transform playerSpawnPoint;
    public Transform enemySpawnPoint;

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
            unitDataList.Add(new UnitData { id = "soldier", cost = 25, hp = 90, damage = 10, attackRate = 1f, speed = 60, range = 40 });
            unitDataList.Add(new UnitData { id = "tank", cost = 60, hp = 240, damage = 16, attackRate = 1.2f, speed = 45, range = 40 });
            unitDataList.Add(new UnitData { id = "archer", cost = 45, hp = 70, damage = 8, attackRate = 0.8f, speed = 60, range = 200 });
        }
    }

    public void SpawnUnit(int unitIndex, Team team)
    {
        if (unitIndex < 0 || unitIndex >= unitDataList.Count) return;

        UnitData data = unitDataList[unitIndex];

        if (team == Team.Player)
        {
            if (GameManager.Instance.currentGold >= data.cost)
            {
                GameManager.Instance.UpdateGold(-data.cost);
                CreateUnitInstance(data, team, playerSpawnPoint.position);
            }
        }
        else
        {
            CreateUnitInstance(data, team, enemySpawnPoint.position);
        }
    }

    private void CreateUnitInstance(UnitData data, Team team, Vector3 position)
    {
        GameObject go = Instantiate(unitPrefab, position, Quaternion.identity);
        Unit unit = go.GetComponent<Unit>();
        unit.Initialize(data, team);
    }
}
