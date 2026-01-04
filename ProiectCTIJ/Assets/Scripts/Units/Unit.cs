using UnityEngine;

public enum Team { Player, Enemy }

public class Unit : MonoBehaviour
{
    [Header("Settings")]
    public Team team;
    public float speed = 2f;
    public float hp = 100f;
    public float damage = 10f;
    public float attackRange = 1f;
    public float attackRate = 1f;

    private float nextAttackTime;
    private bool isStopped = false;
    private Unit currentTarget;

    protected virtual void Update()
    {
        if (hp <= 0) return;

        // 1. Detectare inamici în apropiere
        DetectEnemy();

        // 2. Mișcare (dacă nu este oprit de un inamic)
        if (!isStopped)
        {
            Move();
        }
        else
        {
            // 3. Atac
            Attack();
        }
    }

    void Move()
    {
        // Jucătorul merge la dreapta (X pozitiv), Inamicul la stânga (X negativ)
        float direction = (team == Team.Player) ? 1f : -1f;
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    void DetectEnemy()
    {
        // Folosim OverlapCircle pentru a detecta toți inamicii în raza de atac
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        
        currentTarget = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == this.gameObject) continue; // Ignoră propria unitate
            
            Unit targetUnit = hit.GetComponent<Unit>();
            if (targetUnit != null && targetUnit.team != this.team && targetUnit.hp > 0)
            {
                float distance = Vector2.Distance(transform.position, targetUnit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    currentTarget = targetUnit;
                }
            }
        }

        isStopped = (currentTarget != null);
    }

    void Attack()
    {
        if (currentTarget == null || currentTarget.hp <= 0)
        {
            isStopped = false;
            return;
        }

        if (Time.time >= nextAttackTime)
        {
            currentTarget.TakeDamage(damage);
            nextAttackTime = Time.time + attackRate;
            Debug.Log(gameObject.name + " a atacat pe " + currentTarget.gameObject.name);
        }
    }

    public virtual void TakeDamage(float amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            Die();
        }
    }

    public virtual void Initialize(UnitData unitData, Team unitTeam)
    {
        // Această metodă va fi folosită de Spawner pentru a seta statisticile din JSON
        team = unitTeam;
        if (unitData != null)
        {
            hp = unitData.hp;
            damage = unitData.damage;
            speed = unitData.speed;
            attackRange = unitData.range;
            attackRate = unitData.attackRate;
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " a murit.");
        Destroy(gameObject);
    }
}
