using UnityEngine;
using System;

public enum Team { Player, Enemy }

public class Unit : MonoBehaviour
{
    [Header("Unit Settings")]
    public Team team;
    
    [Header("Attack Type")]
    public bool isRanged = false; // TRUE pentru Priest/Peasant(archer)
    public GameObject projectilePrefab; // Doar pentru ranged
    [Tooltip("Offset local (x,y) față de pivot pentru a lansa proiectilul din mâna/arcul unității.")]
    public Vector2 projectileSpawnOffset = new Vector2(0.1f, 0.45f);
    
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

    private bool hasCastTrigger;
    
    [Header("Runtime")]
    public float currentHP; // Public pentru a putea fi accesat din alte scripturi
    private float nextAttackTime;
    private Unit currentTarget;
    private float currentTargetDistance = float.MaxValue;
    private float currentTargetEdgeDistance = float.MaxValue;

    // Ranged: spawn projectile from animation event (so arrow releases on the right frame)
    private bool pendingRangedShot;
    private Unit pendingRangedTarget;
    private float pendingRangedDamage;
    private float pendingRangedFireTime;

    // Small gap to keep between allies so physics never starts pushing.
    private const float AllyQueueGap = 0.18f;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        
        // DEZACTIVEAZĂ ROOT MOTION - altfel animațiile mișcă unitățile pe Y!
        if (animator != null)
        {
            animator.applyRootMotion = false;

            hasCastTrigger = HasAnimatorParameter(animator, "cast");

            // Safety: ensure we don't spawn in a stale/true trigger state.
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

        // Apply simple role presets based on prefab/name so the gameplay is readable and strategic.
        // (Skip bases; they override hp/stats in BaseUnit)
        if (GetComponent<BaseUnit>() == null)
        {
            ApplyRolePreset();
        }

        currentHP = hp;
        
        // Asigură-te că Rigidbody2D este setat corect
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f; // Dezactivează gravitatea
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY; // Nu roti și nu te miști pe Y
        }
    }

    private void ApplyRolePreset()
    {
        // After spawn, the object name may become "<Name> (Team)"; normalize it.
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

        // Roles requested: Soldier (heavy), Thief (fast/low dmg), Priest (ranged mid dmg / low hp)
        if (nameOnly.Contains("Soldier", StringComparison.OrdinalIgnoreCase) ||
            nameOnly.Contains("Knight", StringComparison.OrdinalIgnoreCase) ||
            nameOnly.Contains("Merchant", StringComparison.OrdinalIgnoreCase))
        {
            // Heavy/frontliner
            presetHp = 160f;
            presetDamage = 14f;
            presetSpeed = 1.5f;
            presetRange = 0.9f;
            presetAttackRate = 1.0f;
            presetIsRanged = false;

            cost = 35;
            killReward = 20;
        }
        else if (nameOnly.Contains("Thief", StringComparison.OrdinalIgnoreCase))
        {
            // Fast/light (half dmg vs Soldier)
            presetHp = 110f;
            presetDamage = 7f;
            presetSpeed = 2.8f;
            presetRange = 0.9f;
            presetAttackRate = 0.9f;
            presetIsRanged = false;

            cost = 20;
            killReward = 12;
        }
        else if (nameOnly.Contains("Peasant", StringComparison.OrdinalIgnoreCase))
        {
            // Archer (ranged). Needs projectilePrefab on the prefab.
            presetHp = 75f;
            presetDamage = 6.4f;   // -20% damage
            presetSpeed = 2.2f;
            presetRange = 8f;     // -2 range
            presetAttackRate = 0.85f;
            presetIsRanged = (projectilePrefab != null);

            if (!presetIsRanged)
            {
                // If no projectile is set, fall back to a light melee so it still functions.
                presetHp = 110f;
                presetDamage = 7f;
                presetSpeed = 2.8f;
                presetRange = 0.9f;
                presetAttackRate = 0.9f;
                presetIsRanged = false;
                Debug.LogWarning($"{gameObject.name}: Peasant is missing projectilePrefab; falling back to melee preset.");
            }

            cost = presetIsRanged ? 28 : 20;
            killReward = presetIsRanged ? 16 : 12;
        }
        else if (nameOnly.Contains("Priest", StringComparison.OrdinalIgnoreCase))
        {
            // Ranged support
            presetHp = 80f;
            presetDamage = 8f;    // -20% damage
            presetSpeed = 2.0f;
            presetRange = 8f;     // -2 range
            presetAttackRate = 1.1f;
            presetIsRanged = true;

            cost = 30;
            killReward = 18;
        }

        hp = presetHp;
        damage = presetDamage;
        speed = presetSpeed;
        attackRange = presetRange;
        attackRate = presetAttackRate;
        isRanged = presetIsRanged;

        // Keep unitName consistent for logging/UI.
        if (unitName == "Unit")
            unitName = nameOnly;
    }

    protected virtual void Update()
    {
        if (currentHP <= 0) return;

        // If the casting animation event is missing/misconfigured, still fire after a short delay.
        if (pendingRangedShot && Time.time >= pendingRangedFireTime)
        {
            FireRangedProjectile();
        }

        DetectEnemy();

        bool hasTarget = (currentTarget != null && currentTarget.currentHP > 0);
        // Treat attackRange as edge-to-edge distance (not center-to-center)
        bool inAttackRange = hasTarget && (currentTargetEdgeDistance <= attackRange + 0.02f);

        if (inAttackRange)
            Attack();
        else
            Move();
    }

    void Move()
    {
        float direction = (team == Team.Player) ? 1f : -1f;

        // Queue behind allies so they don't collide/push each other.
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
        
        // Folosim Rigidbody2D pentru mișcare mai bună
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Mișcare doar pe X, Y rămâne fix la 0
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

    void DetectEnemy()
    {
        // Pentru detectarea țintelor, folosim un range mai mare (inclusiv pentru bazele)
        float detectionRange = Mathf.Max(attackRange, 2f); // Minim 2 pentru a detecta bazele
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
            // Dacă folosim doar GetComponent<Unit>(), unitățile nu detectează baza/ținta.
            Unit targetUnit = hit.GetComponentInParent<Unit>();
            if (targetUnit != null && targetUnit.team != this.team && targetUnit.currentHP > 0)
            {
                float distance = Vector2.Distance(transform.position, targetUnit.transform.position);

                // Nu targeta lucruri "în spate" (previne comportamente ciudate când se suprapun / trec unul de altul)
                float deltaX = targetUnit.transform.position.x - transform.position.x;
                if (Mathf.Sign(deltaX) != Mathf.Sign(direction) && Mathf.Abs(deltaX) > 0.01f)
                    continue;
                
                // Verifică că inamicul este într-adevăr în range
                if (distance <= detectionRange + 0.2f && distance < closestDistance)
                {
                    closestDistance = distance;
                    currentTarget = targetUnit;
                    currentTargetDistance = distance;
                    currentTargetEdgeDistance = GetEdgeDistanceTo(targetUnit, distance);
                }
            }
        }

        // Debug pentru a vedea ce detectează
        if (currentTarget != null)
        {
            Debug.DrawLine(transform.position, currentTarget.transform.position, Color.red);
        }
    }

    void Attack()
    {
        if (currentTarget == null || currentTarget.currentHP <= 0) return;

        // Dacă ținta nu e încă în range, continuă să mergi (nu ataca de la distanță la melee)
        if (currentTargetEdgeDistance > attackRange + 0.02f) return;
        
        // Oprește mișcarea când atacă
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
                // For ranged, prefer casting animation via trigger; don't force melee attack state.
                animator.SetBool("isAttacking", !isRanged);
            }
            catch { }
        }

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackRate;
            
            if (isRanged && projectilePrefab != null)
            {
                // RANGED ATTACK - launch projectile from animation event (casting)
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
                // Pentru toate unitățile (ranged sau melee), folosim trigger "attack"
                // "cast" era doar pentru vrăjitori cu animație separată de casting
                try { animator.SetTrigger("attack"); } catch { }
            }
            else
            {
                // No animator: fire immediately for ranged.
                if (isRanged && projectilePrefab != null)
                    FireRangedProjectile();
            }
            
            Debug.Log(gameObject.name + " a atacat pe " + currentTarget.gameObject.name);
        }
    }

    // Hook this from the casting animation clip using an Animation Event.
    // Event name: FireRangedProjectile
    public void FireRangedProjectile()
    {
        if (!pendingRangedShot) return;
        pendingRangedShot = false;

        if (projectilePrefab == null) return;
        if (pendingRangedTarget == null || pendingRangedTarget.currentHP <= 0) return;

        // Dacă ținta a ieșit din range până la momentul lansării (animation event / fallback timer), anulăm shot-ul.
        float centerDistance = Vector2.Distance(transform.position, pendingRangedTarget.transform.position);
        float edgeDistanceNow = GetEdgeDistanceTo(pendingRangedTarget, centerDistance);
        if (edgeDistanceNow > attackRange + 0.15f)
        {
            pendingRangedTarget = null;
            return;
        }

        Debug.Log($"{gameObject.name} fires projectile at {pendingRangedTarget.gameObject.name} (dist {(pendingRangedTarget.transform.position - transform.position).magnitude:F2})");

        // Spawn lângă arc, nu din picioare. Offset-ul pe X respectă direcția echipei.
        float dirSign = (team == Team.Player) ? 1f : -1f;

        // Ridică spawn-ul în funcție de collider-ul unității ca să nu pornească din picioare.
        float colliderHeight = 0f;
        Collider2D selfCol = GetComponent<Collider2D>();
        if (selfCol != null)
            colliderHeight = selfCol.bounds.extents.y; // jumătate din înălțimea collider-ului

        Vector3 spawnPos = transform.position + new Vector3(projectileSpawnOffset.x * dirSign, colliderHeight + projectileSpawnOffset.y, 0f);

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(pendingRangedTarget, (int)pendingRangedDamage, team);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} spawned projectile prefab without Projectile component: {proj.name}");
        }
    }

    private bool IsAllyBlockingInFront(float direction)
    {
        // If we have an enemy in front and we are close enough to attack, don't queue-stop.
        if (currentTarget != null && currentTarget.currentHP > 0 && currentTargetEdgeDistance <= attackRange + 0.02f)
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

            // Only queue behind allies.
            if (other.team != this.team) continue;

            // Must be in front.
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

    private float GetEdgeDistanceTo(Unit other, float centerDistance)
    {
        float myRadius = GetHorizontalRadius(this);
        float otherRadius = GetHorizontalRadius(other);
        return centerDistance - (myRadius + otherRadius);
    }

    private static float GetHorizontalRadius(Unit unit)
    {
        if (unit == null) return 0.1f;
        Collider2D col = unit.GetComponent<Collider2D>();
        if (col == null) return 0.1f;
        return Mathf.Max(0.05f, col.bounds.extents.x);
    }

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
