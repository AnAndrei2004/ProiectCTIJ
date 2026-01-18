using UnityEngine;

public class BaseUnit : Unit
{
    [Header("Base Settings")]
    public bool isPlayerBase;
    
    [Header("Visual Effects")]
    [Tooltip("Prefab pentru efect de foc cand baza e low HP")]
    public GameObject fireEffectPrefab;
    [Tooltip("Prefab pentru explozie cand baza e distrusa")]
    public GameObject explosionEffectPrefab;
    [Tooltip("Procentul de HP sub care apare focul (0.3 = 30%)")]
    [Range(0f, 1f)] public float fireThreshold = 0.3f;
    
    private GameObject activeFireEffect;
    private bool fireStarted = false;

    // Initializeaza baza si seteaza atributele fixe.
    protected override void Start()
    {
        // Setam echipa in functie de tipul bazei
        team = isPlayerBase ? Team.Player : Team.Enemy;
        
        // Bazele au HP fix
        hp = isPlayerBase ? 300f : 300f;
        attackRange = 0f;
        damage = 0f;
        speed = 0f;
        
        // IMPORTANT: Apelam base.Start() pentru a initializa currentHP
        base.Start();

        // Bazele NU trebuie sa fie impinse de unitati prin fizica.
        // Unit.Start() seteaza implicit Rigidbody2D pe Dynamic; pentru baze il fortam pe Static.
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

    // Update pentru efectul de foc al bazei.
    protected override void Update()
    {
        if (currentHP <= 0) return;

        // Verifica daca trebuie sa porneasca efectul de foc
        CheckFireEffect();
    }

    // Porneste efectul de foc cand baza are HP scazut.
    void CheckFireEffect()
    {
        if (fireEffectPrefab == null) return;
        
        // Porneste focul cand HP scade sub 150
        if (!fireStarted && currentHP <= 150f && currentHP > 0)
        {
            fireStarted = true;
            activeFireEffect = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity, transform);
            
            // Asigura-te ca efectul de foc este in loop
            ParticleSystem ps = activeFireEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.loop = true;
            }
            
            Debug.Log($"{(isPlayerBase ? "Player" : "Enemy")} Base is on fire! HP: {currentHP:F0}");
        }
    }

    // Suprascriere: primeste damage si verifica distrugerea bazei.
    public override void TakeDamage(float amount)
    {
        currentHP -= amount;
        Debug.Log($"{(isPlayerBase ? "Player" : "Enemy")} Base HP: {currentHP}/{hp}");
        
        if (currentHP <= 0)
        {
            Die();
        }
    }

    // Proceseaza distrugerea bazei.
    void Die()
    {
        Debug.Log("Baza a fost distrusa!");
        
        // Opreste efectul de foc
        if (activeFireEffect != null)
        {
            Destroy(activeFireEffect);
        }
        
        // Spawn efect de explozie
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            // Distruge explozia dupa 5 secunde
            Destroy(explosion, 5f);
        }
        
        // Asteapta cateva secunde pentru a vedea explozia inainte de a termina jocul
        if (GameManager.Instance != null)
        {
            StartCoroutine(DelayedEndGame());
        }
        
        Destroy(gameObject, 0.5f);
    }

    // Termina jocul dupa un mic delay.
    System.Collections.IEnumerator DelayedEndGame()
    {
        // Asteapta 3 secunde pentru a vedea explozia
        yield return new WaitForSeconds(3f);
        
        // Acum terminam jocul
        GameManager.Instance.EndGame(!isPlayerBase);
    }
}
