using UnityEngine;

public class BaseUnit : Unit
{
    [Header("Base Settings")]
    public bool isPlayerBase;
    
    [Header("Visual Effects")]
    [Tooltip("Prefab pentru efect de foc când baza e low HP")]
    public GameObject fireEffectPrefab;
    [Tooltip("Prefab pentru explozie când baza e distrusă")]
    public GameObject explosionEffectPrefab;
    [Tooltip("Procentul de HP sub care apare focul (0.3 = 30%)")]
    [Range(0f, 1f)] public float fireThreshold = 0.3f;
    
    private GameObject activeFireEffect;
    private bool fireStarted = false;

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

        // Verifică dacă trebuie să pornească efectul de foc
        CheckFireEffect();
    }
    
    void CheckFireEffect()
    {
        if (fireEffectPrefab == null) return;
        
        float healthPercent = currentHP / hp;
        
        // Pornește focul când HP scade sub threshold
        if (!fireStarted && healthPercent <= fireThreshold && healthPercent > 0)
        {
            fireStarted = true;
            activeFireEffect = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity, transform);
            Debug.Log($"{(isPlayerBase ? "Player" : "Enemy")} Base is on fire! HP: {healthPercent * 100:F0}%");
        }
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
        
        // Oprește efectul de foc
        if (activeFireEffect != null)
        {
            Destroy(activeFireEffect);
        }
        
        // Spawn efect de explozie
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            // Distruge explozia după 3 secunde (sau lasă-l să se auto-distrugă)
            Destroy(explosion, 3f);
        }
        
        // Anunțăm GameManager că s-a terminat jocul
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndGame(!isPlayerBase);
        }
        
        Destroy(gameObject);
    }
}
