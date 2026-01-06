using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;
    public Team team;
    
    private Unit target;
    private Vector3 direction;
    
    public void Initialize(Unit targetUnit, int projectileDamage, Team sourceTeam)
    {
        target = targetUnit;
        damage = projectileDamage;
        team = sourceTeam;
        
        if (target != null)
        {
            direction = (target.transform.position - transform.position).normalized;
        }
        
        // Distruge după 3 secunde dacă nu lovește nimic
        Destroy(gameObject, 3f);
    }
    
    void Update()
    {
        if (target != null && target.currentHP > 0)
        {
            // Actualizează direcția spre țintă
            direction = (target.transform.position - transform.position).normalized;
        }
        
        // Mișcă proiectilul
        transform.position += direction * speed * Time.deltaTime;
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        Unit hitUnit = collision.GetComponent<Unit>();
        
        if (hitUnit != null && hitUnit.team != team && hitUnit.currentHP > 0)
        {
            hitUnit.TakeDamage(damage);
            Destroy(gameObject); // Distruge proiectilul după impact
        }
    }
}
