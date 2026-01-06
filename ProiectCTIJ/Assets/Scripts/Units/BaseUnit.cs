using UnityEngine;

public class BaseUnit : Unit
{
    [Header("Base Settings")]
    public bool isPlayerBase;

    protected override void Start()
    {
        // Setăm echipa în funcție de tipul bazei
        team = isPlayerBase ? Team.Player : Team.Enemy;
        
        // Bazele au HP fix
        hp = 2000f;
        attackRange = 0f;
        damage = 0f;
        speed = 0f;
        
        // IMPORTANT: Apelăm base.Start() pentru a initializa currentHP
        base.Start();

        // Bazele NU trebuie să fie împinse de unități prin fizică.
        // Unit.Start() setează implicit Rigidbody2D pe Dynamic; pentru baze îl forțăm pe Static.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Static;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    protected override void Update()
    {
        if (currentHP <= 0) return;

        // Bazele nu se mișcă și nu caută ținte ca unitățile mobile
    }

    public override void TakeDamage(float amount)
    {
        currentHP -= amount;
        Debug.Log($"{(isPlayerBase ? "Player" : "Enemy")} Base HP: {currentHP}/{hp}");
        
        if (currentHP <= 0)
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
