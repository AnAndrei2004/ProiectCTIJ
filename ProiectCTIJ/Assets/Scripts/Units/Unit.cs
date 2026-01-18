using UnityEngine;
using System;
using System.Collections.Generic;

public enum Team { Player, Enemy }

public class Unit : MonoBehaviour
{
    [Header("Unit Settings")]
    public Team team;
    
    [Header("Attack Type")]
    public bool isRanged = false; // TRUE pentru Priest/Peasant(archer)
    public GameObject projectilePrefab; // Doar pentru ranged
    [Tooltip("Offset local (x,y) fata de pivot pentru a lansa proiectilul din mana/arcul unitatii.")]
    public Vector2 projectileSpawnOffset = new Vector2(0.1f, 0.45f);

    [Header("Projectile Spawn Safety")]
    [Tooltip("Scale-ul aplicat proiectilelor de tip unitate (skeleton etc). 0.5 = vizibil, 1.0 = mai mare.")]
    public float spawnedProjectileMaxWorldSize = 0.5f;

    [Tooltip("Multiplicator aplicat peste spawnedProjectileMaxWorldSize (ex: Priest 10x). Se aplica doar pentru proiectile mari (skeleton/VFX) care sunt sanitizate in runtime, nu pentru sageti normale.")]
    public float spawnedProjectileSizeMultiplier = 1f;

    [Tooltip("Fallback scale daca nu gasim niciun renderer pe proiectil.")]
    public float spawnedProjectileFallbackScale = 0.15f;

    [Tooltip("Dezactiveaza coliziuni/rigidbody pe proiectilul instantiat (recomandat pentru prefabs mari).")]
    public bool spawnedProjectileDisablePhysics = true;

    [Tooltip("Dezactiveaza scripturi de tip Unit/BaseUnit pe proiectil (daca ai pus din greseala un prefab de unitate ca proiectil).")]
    public bool spawnedProjectileDisableUnitBehaviours = true;

    [Tooltip("Dezactiveaza Animator pe proiectilul instantiat. Pentru VFX animate (ex: cap de schelet), lasa FALSE.")]
    public bool spawnedProjectileDisableAnimator = false;
    
    [Header("Stats")]
    public string unitName = "Unit";
    public int cost = 25;
    public int killReward = 15;
    public float hp = 100f;
    public float damage = 10f;
    public float speed = 2f;
    public float attackRange = 0.8f;  // Redus pentru melee, 3-5 pentru ranged
    public float attackRate = 1f;

    [Header("Components")]
    private Animator animator;
    private Collider2D mainCollider;

    private bool hasCastTrigger;
    
    [Header("Runtime")]
    public float currentHP; // Public pentru a putea fi accesat din alte scripturi
    private float nextAttackTime;
    private Unit currentTarget;
    private float currentTargetDistance = float.MaxValue;
    private float currentTargetEdgeDistance = float.MaxValue;

    // Ranged: spawn proiectil din event de animatie
    private bool pendingRangedShot;
    private Unit pendingRangedTarget;
    private float pendingRangedDamage;
    private float pendingRangedFireTime;

    // Mic spatiu intre aliati ca sa nu se impinga prin fizica.
    private const float AllyQueueGap = 0.18f;

    private readonly HashSet<Collider2D> ignoredAllyColliders = new HashSet<Collider2D>();

    // Permite aliatilor sa treaca prin aceasta unitate ranged cand ataca.
    public bool AllowsAlliesToPassThrough()
    {
        if (!isRanged) return false;
        if (currentTarget == null || currentTarget.currentHP <= 0) return false;
        return currentTargetEdgeDistance <= attackRange + 0.15f;
    }

    // Initializeaza componentele si setarile unitatii.
    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        mainCollider = GetComponent<Collider2D>();
        
        // DEZACTIVEAZA ROOT MOTION - altfel animatiile misca unitatile pe Y
        if (animator != null)
        {
            animator.applyRootMotion = false;

            hasCastTrigger = HasAnimatorParameter(animator, "cast");

            // Asigura ca nu raman triggere vechi active.
            try
            {
                animator.ResetTrigger("attack");
                if (hasCastTrigger) animator.ResetTrigger("cast");
                animator.ResetTrigger("hurt");
                animator.ResetTrigger("die");
                animator.SetBool("isWalking", false);
                animator.SetBool("isAttacking", false);
            }
            catch { }
        }

        // Aplica preseturi simple pe baza numelui prefabului.
        // (Sare peste baze; ele isi suprascriu stats in BaseUnit)
        if (GetComponent<BaseUnit>() == null)
        {
            ApplyRolePreset();
        }

        currentHP = hp;
        
        // Asigura-te ca Rigidbody2D este setat corect
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f; // Dezactiveaza gravitatea
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY; // Nu roti si nu te misti pe Y
        }
    }

    // Aplica preseturi de rol in functie de nume.
    private void ApplyRolePreset()
    {
        // Dupa spawn, numele poate deveni "<Name> (Team)"; normalizam.
        string rawName = !string.IsNullOrWhiteSpace(unitName) && unitName != "Unit" ? unitName : gameObject.name;
        string nameOnly = rawName;
        int parenIndex = nameOnly.IndexOf(" (", StringComparison.Ordinal);
        if (parenIndex >= 0) nameOnly = nameOnly.Substring(0, parenIndex);
        nameOnly = nameOnly.Trim();

        // Defaults (safe)
        float presetHp = hp;
        float presetDamage = damage;
        float presetSpeed = speed;
        float presetRange = attackRange;
        float presetAttackRate = attackRate;
        bool presetIsRanged = isRanged;

        // Roluri: Tank (Knight/Soldier), Light (Thief/Merchant), Ranged (Peasant/Priest)
        if (nameOnly.Contains("Knight", StringComparison.OrdinalIgnoreCase) ||
            nameOnly.Contains("Soldier", StringComparison.OrdinalIgnoreCase))
        {
            // Tank
            presetHp = 200f;
            presetDamage = 15f;
            presetSpeed = 1.8f;
            presetRange = 0.9f;
            presetAttackRate = 1.0f;
            presetIsRanged = false;

            cost = 50;
            killReward = 20;
        }
        else if (nameOnly.Contains("Thief", StringComparison.OrdinalIgnoreCase) ||
                 nameOnly.Contains("Merchant", StringComparison.OrdinalIgnoreCase))
        {
            // Light
            presetHp = 100f;
            presetDamage = 7f;
            presetSpeed = 2.5f;
            presetRange = 0.9f;
            presetAttackRate = 0.9f;
            presetIsRanged = false;

            cost = 20;
            killReward = 12;
        }
        else if (nameOnly.Contains("Peasant", StringComparison.OrdinalIgnoreCase))
        {
            // Ranged (archer). Are nevoie de projectilePrefab pe prefab.
            presetHp = 70f;
            presetDamage = 8f;
            presetSpeed = 2.3f;
            presetRange = 8f;
            presetAttackRate = 0.85f;
            presetIsRanged = true;

            if (projectilePrefab == null)
            {
                Debug.LogWarning($"{gameObject.name}: Peasant is missing projectilePrefab; unitatea ramane ranged dar nu va trage proiectile.");
            }

            cost = 30;
            killReward = 16;
        }
        else if (nameOnly.Contains("Priest", StringComparison.OrdinalIgnoreCase))
        {
            // Ranged
            presetHp = 70f;
            presetDamage = 8f;
            presetSpeed = 2.3f;
            presetRange = 8f;
            presetAttackRate = 1.1f;
            presetIsRanged = true;

            // Cerinta: proiectile Priest mai mari (doar pentru prefabs tip unit/VFX)
            spawnedProjectileSizeMultiplier = 10f;

            cost = 30;
            killReward = 18;
        }

        hp = presetHp;
        damage = presetDamage;
        speed = presetSpeed;
        attackRange = presetRange;
        attackRate = presetAttackRate;
        isRanged = presetIsRanged;

        // Pastreaza unitName consistent pentru log/UI.
        if (unitName == "Unit")
            unitName = nameOnly;
    }

    // Update pentru logica unitatii.
    protected virtual void Update()
    {
        if (currentHP <= 0) return;

        RefreshAllyPassThrough();

        // Daca eventul de animatie lipseste, trage dupa un delay.
        if (pendingRangedShot && Time.time >= pendingRangedFireTime)
        {
            FireRangedProjectile();
        }

        DetectEnemy();

        bool hasTarget = (currentTarget != null && currentTarget.currentHP > 0);
        // Trateaza attackRange ca distanta edge-to-edge
        bool inAttackRange = hasTarget && (currentTargetEdgeDistance <= attackRange + 0.02f);

        if (inAttackRange)
            Attack();
        else
            Move();
    }

    // Deplaseaza unitatea spre inamici.
    void Move()
    {
        float direction = (team == Team.Player) ? 1f : -1f;

        // Sta in spatele aliatilor ca sa nu se impinga
        if (IsAllyBlockingInFront(direction))
        {
            Rigidbody2D rbStop = GetComponent<Rigidbody2D>();
            if (rbStop != null)
                rbStop.linearVelocity = Vector2.zero;

            if (animator != null && animator.isActiveAndEnabled)
            {
                try
                {
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isAttacking", false);
                }
                catch { }
            }
            return;
        }
        
        // Folosim Rigidbody2D pentru miscare mai buna
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Miscare doar pe X, Y ramane fix la 0
            rb.linearVelocity = new Vector2(direction * speed, 0f);
        }
        else
        {
            transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
        }
        
        if (animator != null && animator.isActiveAndEnabled)
        {
            try
            {
                animator.SetBool("isWalking", true);
                animator.SetBool("isAttacking", false);
            }
            catch { }
        }
    }

    // Detecteaza cel mai apropiat inamic in fata.
    void DetectEnemy()
    {
        // Pentru detectarea tintelor, folosim un range mai mare (baza poate fi mai sus pe Y)
        float detectionRange = Mathf.Max(attackRange, 6f); // minim 6 ca sa prinda si bazele
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        
        currentTarget = null;
        float closestDistance = float.MaxValue;
        currentTargetDistance = float.MaxValue;
        currentTargetEdgeDistance = float.MaxValue;

        float direction = (team == Team.Player) ? 1f : -1f;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == this.gameObject) continue;
            
            // IMPORTANT: multe prefab-uri au collider pe child, iar scriptul Unit pe parent.
            // Daca folosim doar GetComponent<Unit>(), unitatile nu detecteaza baza/tinta.
            Unit targetUnit = hit.GetComponentInParent<Unit>();
            if (targetUnit != null && targetUnit.team != this.team && targetUnit.currentHP > 0)
            {
                float distance = Mathf.Abs(targetUnit.transform.position.x - transform.position.x);

                // Nu targeta lucruri "in spate" (previne comportamente ciudate cand se suprapun / trec unul de altul)
                float deltaX = targetUnit.transform.position.x - transform.position.x;
                if (Mathf.Sign(deltaX) != Mathf.Sign(direction) && Mathf.Abs(deltaX) > 0.01f)
                    continue;
                
                // Verifica ca inamicul este intr-adevar in range
                if (distance <= detectionRange + 0.2f && distance < closestDistance)
                {
                    closestDistance = distance;
                    currentTarget = targetUnit;
                    currentTargetDistance = distance;
                    currentTargetEdgeDistance = GetEdgeDistanceTo(targetUnit, distance);
                }
            }
        }

        // Debug pentru a vedea ce detecteaza
        if (currentTarget != null)
        {
            Debug.DrawLine(transform.position, currentTarget.transform.position, Color.red);
        }
    }

    // Ataca tinta curenta.
    void Attack()
    {
        if (currentTarget == null || currentTarget.currentHP <= 0) return;

        // Daca tinta nu e inca in range, continua sa mergi
        if (currentTargetEdgeDistance > attackRange + 0.02f) return;
        
        // Opreste miscarea cand ataca
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (animator != null && animator.isActiveAndEnabled)
        {
            try
            {
                animator.SetBool("isWalking", false);
                // Pentru ranged, folosim trigger de cast; nu fortam melee.
                animator.SetBool("isAttacking", !isRanged);
            }
            catch { }
        }

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackRate;
            
            if (isRanged && projectilePrefab != null)
            {
                // RANGED ATTACK - lanseaza proiectil din event de animatie
                pendingRangedShot = true;
                pendingRangedTarget = currentTarget;
                pendingRangedDamage = damage;
                pendingRangedFireTime = Time.time + 0.35f;
            }
            else
            {
                // MELEE ATTACK - damage direct
                currentTarget.TakeDamage(damage);
            }
            
            if (animator != null && animator.isActiveAndEnabled)
            {
                // Pentru toate unitatile (ranged sau melee), folosim trigger "attack"
                // "cast" era doar pentru vrajitori cu animatie separata
                try { animator.SetTrigger("attack"); } catch { }
            }
            else
            {
                // Fara animator: trage imediat pentru ranged.
                if (isRanged && projectilePrefab != null)
                    FireRangedProjectile();
            }
            
            Debug.Log(gameObject.name + " a atacat pe " + currentTarget.gameObject.name);
        }
    }

    // Foloseste Animation Event din clipul de cast: FireRangedProjectile
    public void FireRangedProjectile()
    {
        if (!pendingRangedShot) return;
        pendingRangedShot = false;

        if (projectilePrefab == null) return;
        if (pendingRangedTarget == null || pendingRangedTarget.currentHP <= 0) return;

        // Daca tinta a iesit din range pana la momentul lansarii, anulam shot-ul.
        float centerDistance = Mathf.Abs(pendingRangedTarget.transform.position.x - transform.position.x);
        float edgeDistanceNow = GetEdgeDistanceTo(pendingRangedTarget, centerDistance);
        if (edgeDistanceNow > attackRange + 0.15f)
        {
            pendingRangedTarget = null;
            return;
        }

        Debug.Log($"{gameObject.name} fires projectile at {pendingRangedTarget.gameObject.name} (dist {(pendingRangedTarget.transform.position - transform.position).magnitude:F2})");

        // Spawn langa arc, nu din picioare. Offset-ul pe X respecta directia echipei.
        float dirSign = (team == Team.Player) ? 1f : -1f;

        // Ridica spawn-ul in functie de collider ca sa nu porneasca din picioare.
        float colliderHeight = 0f;
        Collider2D selfCol = GetComponent<Collider2D>();
        if (selfCol != null)
            colliderHeight = selfCol.bounds.extents.y; // jumatate din inaltimea collider-ului

        Vector3 spawnPos = transform.position + new Vector3(projectileSpawnOffset.x * dirSign, colliderHeight + projectileSpawnOffset.y, 0f);

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        
        // FORTAM SCALE-UL PENTRU PRIEST (sau orice unitate care are spawnedProjectileMaxWorldSize != default)
        // Verificam daca unitatea e Priest sau daca user-ul a setat un scale custom
        bool isPriest = unitName.Contains("Priest", StringComparison.OrdinalIgnoreCase) || 
                        gameObject.name.Contains("Priest", StringComparison.OrdinalIgnoreCase);
        
        if (isPriest)
        {
            // Dezactiveaza TOT ce ar putea face prefab-ul sa se comporte ca unitate
            foreach (var c in proj.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (c == null) continue;
                if (c is Projectile) continue; // Pastreaza scriptul Projectile
                c.enabled = false;
            }
            foreach (var col in proj.GetComponentsInChildren<Collider2D>(true))
                if (col != null) col.enabled = false;
            foreach (var rb in proj.GetComponentsInChildren<Rigidbody2D>(true))
            {
                if (rb == null) continue;
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }
            
            // SCALE FIX HARDCODAT: 0.04 pentru skeleton (mic dar vizibil)
            float priestScale = 0.04f;
            proj.transform.localScale = new Vector3(priestScale, priestScale, priestScale);
            Debug.Log($"[Priest] Projectile scale set to {priestScale}. Object: {proj.name}");
        }
        else
        {
            // Pentru alte unitati (archer etc), nu modificam scale-ul
            ConfigureSpawnedProjectile(proj);
        }
        
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile == null)
        {
            // Daca prefab-ul e doar visual, adauga comportament runtime.
            projectile = proj.AddComponent<Projectile>();
            Debug.LogWarning($"{gameObject.name} spawned projectile prefab without Projectile component; added it at runtime: {proj.name}");
        }

        projectile.Initialize(pendingRangedTarget, (int)pendingRangedDamage, team);
    }

    // Configureaza proiectilul daca prefab-ul seamana cu o unitate.
    private void ConfigureSpawnedProjectile(GameObject proj)
    {
        if (proj == null) return;

        // Verifica daca prefab-ul e un "unit prefab" (are Unit/BaseUnit/Animator)
        bool hasUnitScript = (proj.GetComponentInChildren<Unit>(true) != null) || (proj.GetComponentInChildren<BaseUnit>(true) != null);
        bool hasAnimator = (proj.GetComponentInChildren<Animator>(true) != null);
        
        // Daca nu e un prefab de tip unitate, nu face nimic (sagetile raman cum sunt)
        if (!hasUnitScript && !hasAnimator)
            return;

        // Dezactiveaza AI-ul si physics pe proiectil (sa nu se comporte ca o unitate)
        Unit[] units = proj.GetComponentsInChildren<Unit>(true);
        foreach (var u in units) if (u != null) u.enabled = false;
        
        BaseUnit[] bases = proj.GetComponentsInChildren<BaseUnit>(true);
        foreach (var b in bases) if (b != null) b.enabled = false;
        
        Collider2D[] cols = proj.GetComponentsInChildren<Collider2D>(true);
        foreach (var c in cols) if (c != null) c.enabled = false;
        
        Rigidbody2D[] rbs = proj.GetComponentsInChildren<Rigidbody2D>(true);
        foreach (var rb in rbs) 
        { 
            if (rb == null) continue;
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; 
        }

        // Opreste root motion (sa nu miste proiectilul singur)
        Animator[] anims = proj.GetComponentsInChildren<Animator>(true);
        foreach (var a in anims) if (a != null) a.applyRootMotion = false;

        // SCALE FIX: pune scale-ul direct la 0.5 (vizibil, nu gigant, nu microscopic)
        // Poti ajusta din Inspector pe Unit -> spawnedProjectileMaxWorldSize
        float scale = spawnedProjectileMaxWorldSize;
        proj.transform.localScale = new Vector3(scale, scale, 1f);
    }

    // Verifica daca un aliat blocheaza in fata.
    private bool IsAllyBlockingInFront(float direction)
    {
        // Daca avem inamic in fata si suntem in range, nu ne oprim in coada.
        if (currentTarget != null && currentTarget.currentHP > 0 && currentTargetEdgeDistance <= attackRange + 0.02f)
            return false;

        // Daca tintim o baza, permitem stacking pentru damage maxim.
        if (currentTarget is BaseUnit)
            return false;

        float probeDistance = 0.35f;
        Vector2 probeCenter = new Vector2(transform.position.x + direction * probeDistance, transform.position.y);
        float probeRadius = 0.25f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(probeCenter, probeRadius);
        if (hits == null || hits.Length == 0) return false;

        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit.gameObject == this.gameObject) continue;

            Unit other = hit.GetComponent<Unit>();
            if (other == null || other.currentHP <= 0) continue;

            // Permite trecerea prin ranged aliat care ataca.
            if (other.team == this.team && other.AllowsAlliesToPassThrough())
                continue;

            // Doar coada in spatele aliatilor.
            if (other.team != this.team) continue;

            // Trebuie sa fie in fata.
            float deltaX = other.transform.position.x - transform.position.x;
            if (Mathf.Sign(deltaX) != Mathf.Sign(direction) && Mathf.Abs(deltaX) > 0.01f)
                continue;

            float centerDistance = Vector2.Distance(transform.position, other.transform.position);
            float edgeDistance = GetEdgeDistanceTo(other, centerDistance);
            if (edgeDistance <= AllyQueueGap)
                return true;
        }

        return false;
    }

    // Gestioneaza ignorarea coliziunilor intre aliati in anumite cazuri.
    private void RefreshAllyPassThrough()
    {
        if (mainCollider == null) return;

        const float passThroughRadius = 1.2f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, passThroughRadius);
        if (hits == null) return;

        HashSet<Collider2D> shouldIgnore = new HashSet<Collider2D>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit == mainCollider) continue;

            Unit other = hit.GetComponentInParent<Unit>();
            if (other == null || other == this) continue;
            if (other.team != this.team) continue;
            if (other is BaseUnit) continue;

            if (other.AllowsAlliesToPassThrough())
            {
                shouldIgnore.Add(hit);
                if (!ignoredAllyColliders.Contains(hit))
                {
                    Physics2D.IgnoreCollision(mainCollider, hit, true);
                    ignoredAllyColliders.Add(hit);
                }
            }
        }

        if (ignoredAllyColliders.Count == 0) return;

        List<Collider2D> toRemove = new List<Collider2D>();
        foreach (var col in ignoredAllyColliders)
        {
            if (col == null || !shouldIgnore.Contains(col))
            {
                if (col != null)
                    Physics2D.IgnoreCollision(mainCollider, col, false);
                toRemove.Add(col);
            }
        }

        foreach (var col in toRemove)
            ignoredAllyColliders.Remove(col);
    }

    // Calculeaza distanta edge-to-edge dintre doua unitati.
    private float GetEdgeDistanceTo(Unit other, float centerDistance)
    {
        float myRadius = GetHorizontalRadius(this);
        float otherRadius = GetHorizontalRadius(other);
        return centerDistance - (myRadius + otherRadius);
    }

    // Calculeaza raza orizontala pe baza collider-ului.
    private static float GetHorizontalRadius(Unit unit)
    {
        if (unit == null) return 0.1f;
        Collider2D col = unit.GetComponent<Collider2D>();
        if (col == null) return 0.1f;
        return Mathf.Max(0.05f, col.bounds.extents.x);
    }

    // Primeste damage si verifica moartea.
    public virtual void TakeDamage(float amount)
    {
        currentHP -= amount;
        
        if (animator != null && animator.isActiveAndEnabled)
        {
            try { animator.SetTrigger("hurt"); }
            catch { }
        }
        
        Debug.Log($"{unitName} took {amount} damage. Current HP: {currentHP}/{hp}");
        
        if (currentHP <= 0)
        {
            Die();
        }
    }

    // Seteaza echipa si orientarea sprite-ului.
    public virtual void SetTeam(Team newTeam)
    {
        team = newTeam;
        
        float absScale = Mathf.Abs(transform.localScale.x);
        
        if (team == Team.Player)
        {
            transform.localScale = new Vector3(-absScale, transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(absScale, transform.localScale.y, transform.localScale.z);
        }
    }

    // Proceseaza moartea unitatii.
    void Die()
    {
        Debug.Log(gameObject.name + " a murit.");
        
        if (animator != null && animator.isActiveAndEnabled)
        {
            try
            {
                animator.SetTrigger("die");
                animator.SetBool("isWalking", false);
                animator.SetBool("isAttacking", false);
            }
            catch { }
        }
        
        if (team == Team.Enemy && GameManager.Instance != null)
        {
            GameManager.Instance.AddKillReward(killReward);
        }
        
        Destroy(gameObject, 0.5f);
    }

    // Verifica daca animatorul are un parametru.
    private static bool HasAnimatorParameter(Animator anim, string name)
    {
        if (anim == null || string.IsNullOrEmpty(name)) return false;
        var parameters = anim.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].name == name) return true;
        }
        return false;
    }
}
