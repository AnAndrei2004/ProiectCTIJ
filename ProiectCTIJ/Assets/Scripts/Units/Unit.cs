using UnityEngine;

public enum Team { Player, Enemy }

public class Unit : MonoBehaviour
{
    [Header("Unit Settings")]
    public Team team;
    
    [Header("Stats")]
    public string unitName = "Unit";
    public int cost = 25;
    public int killReward = 15;
    public float hp = 100f;
    public float damage = 10f;
    public float speed = 2f;
    public float attackRange = 1.5f;
    public float attackRate = 1f;

    [Header("Runtime")]
    private float currentHP;
    private float nextAttackTime;
    private bool isStopped = false;
    private Unit currentTarget;

    protected virtual void Start()
    {
        currentHP = hp; // Inițializează HP-ul curent
    }

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
        // Player (în stânga) merge spre dreapta (X pozitiv)
        // Enemy (în dreapta) merge spre stânga (X negativ)
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

    public virtual void SetTeam(Team newTeam)
    {
        team = newTeam;
        
        // Schimbă direcția vizuală (Facing)
        // Player (în stânga) trebuie să privească spre DREAPTA -> Scale X NEGATIV
        // Enemy (în dreapta) trebuie să privească spre STÂNGA -> Scale X POZITIV
        
        float absScale = Mathf.Abs(transform.localScale.x);
        
        if (team == Team.Player)
        {
            // Player privește spre dreapta (scale X negativ pentru a întoarce sprite-ul)
            transform.localScale = new Vector3(-absScale, transform.localScale.y, transform.localScale.z);
        }
        else // Enemy
        {
            // Enemy privește spre stânga (scale X pozitiv)
            transform.localScale = new Vector3(absScale, transform.localScale.y, transform.localScale.z);
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " a murit.");
        
        // Dacă unitatea ucisă este inamic, jucătorul primește recompensă
        if (team == Team.Enemy && GameManager.Instance != null)
        {
            GameManager.Instance.AddKillReward(killReward);
        }
        
        Destroy(gameObject);
    }
}
