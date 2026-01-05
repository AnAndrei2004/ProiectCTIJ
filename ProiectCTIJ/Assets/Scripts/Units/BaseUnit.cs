using UnityEngine;

public class BaseUnit : Unit
{
    [Header("Base Settings")]
    public bool isPlayerBase;

    protected override void Start()
    {
        // Bazele au HP fix, nu folosim valorile din Unit
        hp = 2000f;
        attackRange = 0f;
        damage = 0f;
        speed = 0f;
        
        // Setăm echipa în funcție de tipul bazei
        team = isPlayerBase ? Team.Player : Team.Enemy;
    }

    protected override void Update()
    {
        if (hp <= 0) return;

        // Bazele nu se mișcă și nu caută ținte ca unitățile mobile
        // Doar verificăm dacă a murit
    }

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
