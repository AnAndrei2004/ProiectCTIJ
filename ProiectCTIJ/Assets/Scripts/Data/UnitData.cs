using System;

[Serializable]
public class UnitData
{
    public string id;
    public string name;
    public int cost;
    public float hp;
    public float damage;
    public float attackRate;
    public float speed;
    public float range;
    public string description;
}

[Serializable]
public class UnitDataList
{
    public UnitData[] units;
}
