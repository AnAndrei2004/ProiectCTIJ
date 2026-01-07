using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 12f;
    
    [Header("Visual")]
    public float rotationOffsetDeg = -90f;
    
    private int damage;
    private Team team;
    private Unit target;
    private Vector3 direction;
    private Vector3 spawnPosition;
    private bool initialized;
    private bool hasHit;
    
    public void Initialize(Unit targetUnit, int projectileDamage, Team sourceTeam)
    {
        initialized = true;
        hasHit = false;
        
        target = targetUnit;
        damage = projectileDamage;
        team = sourceTeam;
        spawnPosition = transform.position;
        
        // Zboară ORIZONTAL - doar pe X, fără să intre în pământ
        direction = (team == Team.Player) ? Vector3.right : Vector3.left;
        
        // Rotația e fixă - orizontală
        float angle = (team == Team.Player) ? 0f : 180f;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffsetDeg);
        CancelInvoke();
        Destroy(gameObject, 4f);
        
        Debug.Log("[Arrow] Fired at " + (target != null ? target.gameObject.name : "none"));
    }
    
    void Update()
    {
        if (!initialized || hasHit) return;
        
        // Zboară doar orizontal - nu urmărește ținta pe Y
        transform.position += direction * speed * Time.deltaTime;
        
        float traveled = Vector3.Distance(transform.position, spawnPosition);
        if (traveled > 0.25f)
        {
            CheckHit();
        }
    }
    
    // Rotația e setată o singură dată în Initialize()
    
    void CheckHit()
    {
        if (hasHit) return;
        
        // Verifică coliziune cu ORICE inamic în cale - rază mare pe Y pentru că zboară orizontal
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, 0.6f);
        if (cols != null)
        {
            foreach (Collider2D col in cols)
            {
                if (col == null) continue;
                
                Unit unit = col.GetComponentInParent<Unit>();
                if (unit != null && unit.team != team && unit.currentHP > 0)
                {
                    // Verifică că suntem aproape pe X (săgeata zboară orizontal)
                    float deltaX = Mathf.Abs(transform.position.x - unit.transform.position.x);
                    if (deltaX < 0.5f)
                    {
                        ApplyHit(unit);
                        return;
                    }
                }
            }
        }
    }
    
    void ApplyHit(Unit unit)
    {
        if (hasHit || unit == null) return;
        
        hasHit = true;
        Debug.Log("[Arrow] Hit " + unit.gameObject.name + " for " + damage);
        unit.TakeDamage(damage);
        Destroy(gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!initialized || hasHit) return;
        
        float traveled = Vector3.Distance(transform.position, spawnPosition);
        if (traveled < 0.25f) return;
        
        Unit unit = collision.GetComponentInParent<Unit>();
        if (unit != null && unit.team != team && unit.currentHP > 0)
        {
            ApplyHit(unit);
        }
    }
}
