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

    protected virtual void Update()
    {
        if (hp <= 0) return;

        // 1. Detectare inamici în față
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
        // Tragem o rază invizibilă în față pentru a vedea dacă e cineva
        float direction = (team == Team.Player) ? 1f : -1f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * direction, attackRange);

        if (hit.collider != null)
        {
            Unit targetUnit = hit.collider.GetComponent<Unit>();
            if (targetUnit != null && targetUnit.team != this.team)
            {
                isStopped = true;
                return;
            }
        }
        isStopped = false;
    }

    void Attack()
    {
        if (Time.time >= nextAttackTime)
        {
            // Căutăm din nou ținta pentru a-i da damage
            float direction = (team == Team.Player) ? 1f : -1f;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * direction, attackRange);

            if (hit.collider != null)
            {
                Unit targetUnit = hit.collider.GetComponent<Unit>();
                if (targetUnit != null && targetUnit.team != this.team)
                {
                    targetUnit.TakeDamage(damage);
                    nextAttackTime = Time.time + attackRate;
                    Debug.Log(gameObject.name + " a atacat pe " + targetUnit.gameObject.name);
                }
            }
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
