using UnityEngine;

public class BaseUnit : Unit
{
    [Header("Base Settings")]
    public bool isPlayerBase;

    public override void Initialize(UnitData unitData, Team unitTeam)
    {
        // Bazele au de obicei statistici fixe, nu neapărat din JSON-ul de unități
        this.team = unitTeam;
        this.hp = 2000f; // HP-ul bazei conform pitch-ului
        this.attackRange = 0f; // Baza în sine nu atacă (sau are turret separat)
    }

    protected override void Update()
    {
        if (hp <= 0) return;

        // Bazele nu se mișcă și nu caută ținte ca unitățile mobile
        // Doar verificăm dacă a murit
    }

    // Suprascriem Move ca să fim siguri că baza nu pleacă de pe loc
    void Move() { }

    public override void TakeDamage(float amount)
    {
        hp -= amount;
        Debug.Log($"{(isPlayerBase ? "Player" : "Enemy")} Base HP: {hp}");
        
        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Baza a fost distrusă!");
        // Anunțăm GameManager că s-a terminat jocul
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndGame(!isPlayerBase);
        }
        Destroy(gameObject);
    }
}
