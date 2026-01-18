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

    private Vector3 lastPosition;
    private readonly RaycastHit2D[] segmentHits = new RaycastHit2D[16];
    
    // Initializeaza proiectilul cu tinta, damage si echipa.
    public void Initialize(Unit targetUnit, int projectileDamage, Team sourceTeam)
    {
        initialized = true;
        hasHit = false;
        
        target = targetUnit;
        damage = projectileDamage;
        team = sourceTeam;
        spawnPosition = transform.position;
        lastPosition = transform.position;
        
        // Zboara ORIZONTAL - doar pe X, fara sa intre in pamant
        direction = (team == Team.Player) ? Vector3.right : Vector3.left;
        
        // Rotatia e fixa - orizontala
        float angle = (team == Team.Player) ? 0f : 180f;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffsetDeg);
        CancelInvoke();
        Destroy(gameObject, 4f);
        
        Debug.Log("[Arrow] Fired at " + (target != null ? target.gameObject.name : "none"));
    }

    // Update miscarea proiectilului si coliziunile.
    void Update()
    {
        if (!initialized || hasHit) return;

        // Zboara doar orizontal - nu urmareste tinta pe Y
        Vector3 nextPosition = transform.position + direction * speed * Time.deltaTime;

        // IMPORTANT: anti-tunneling. Proiectile rapide pot trece prin collider fara OnTrigger.
        // Verificam segmentul parcurs in acest frame.
        TryHitAlongSegment(transform.position, nextPosition);
        if (hasHit) return;

        transform.position = nextPosition;
        lastPosition = nextPosition;
        
        float traveled = Vector3.Distance(transform.position, spawnPosition);
        if (traveled > 0.25f)
        {
            // Fallback: overlap (pentru cazuri rare)
            CheckHit();
        }
    }
    
    // Rotatia e setata o singura data in Initialize()
    
    // Verifica lovirea cu overlap simplu.
    void CheckHit()
    {
        if (hasHit) return;
        
        // Verifica coliziune cu ORICE inamic in cale - raza mare pe Y pentru ca zboara orizontal
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, 0.6f);
        if (cols != null)
        {
            foreach (Collider2D col in cols)
            {
                if (col == null) continue;
                
                Unit unit = col.GetComponentInParent<Unit>();
                if (unit != null && unit.team != team && unit.currentHP > 0)
                {
                    // Verifica ca suntem aproape pe X (sageata zboara orizontal)
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

    // Verifica lovirea pe segmentul parcurs in acest frame.
    private void TryHitAlongSegment(Vector3 from, Vector3 to)
    {
        if (hasHit) return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = ~0;
        filter.useTriggers = true; // include triggers indiferent de setarile globale

        int count = Physics2D.Linecast(from, to, filter, segmentHits);
        if (count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = segmentHits[i].collider;
            if (col == null) continue;

            Unit unit = col.GetComponentInParent<Unit>();
            if (unit != null && unit.team != team && unit.currentHP > 0)
            {
                ApplyHit(unit);
                return;
            }
        }
    }

    // Aplica damage si distruge proiectilul.
    void ApplyHit(Unit unit)
    {
        if (hasHit || unit == null) return;
        
        hasHit = true;
        Debug.Log("[Arrow] Hit " + unit.gameObject.name + " for " + damage);
        unit.TakeDamage(damage);
        Destroy(gameObject);
    }

    // Coliziune prin trigger (fallback).
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
