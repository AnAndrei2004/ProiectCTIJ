using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;
    public Team team;
    [Tooltip("Rotire suplimentară pentru sprite; folosește -90 dacă sprite-ul este orientat în sus și vrei să fie orientat spre dreapta.")]
    public float rotationOffsetDeg = -90f;
    [Header("Arc Settings")]
    [Tooltip("Dacă e true, folosește o traiectorie balistică (arc) cu gravitație simulată manual.")]
    public bool useBallisticArc = true;
    [Tooltip("Accelerație gravitațională aplicată pe Y când useBallisticArc este true (negativă pentru a cădea).")]
    public float arcGravity = -18f;
    [Tooltip("Impuls vertical inițial pentru arc; ridică săgeata în sus la start.")]
    public float arcLift = 4f;
    
    private Unit target;
    private Vector3 direction;
    private bool loggedZeroDirection;
    private Vector3 velocity; // folosit când useBallisticArc este true

    void Awake()
    {
        // Safety: neutralize gravity before Initialize in case something instanțiază fără să apeleze imediat Initialize.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
        }
    }
    
    public void Initialize(Unit targetUnit, int projectileDamage, Team sourceTeam)
    {
        target = targetUnit;
        damage = projectileDamage;
        team = sourceTeam;

        // Neutralize gravity and force kinematic to avoid physics drag/arc.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
        }
        
        if (target != null && target.currentHP > 0)
        {
            direction = (target.transform.position - transform.position).normalized;
        }
        else
        {
            // Fallback: shoot forward even dacă ținta moare exact la spawn
            direction = (team == Team.Player) ? Vector3.right : Vector3.left;
        }

        if (useBallisticArc)
        {
            // Setează viteză inițială și impuls vertical pentru arc.
            velocity = direction * speed;
            velocity.y += arcLift;
        }

        // Orientează sprite-ul pe direcția de zbor (2D: în jurul axei Z)
        float angleDeg = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffsetDeg;
        transform.rotation = Quaternion.AngleAxis(angleDeg, Vector3.forward);

        Collider2D col = GetComponent<Collider2D>();
        string colInfo = (col == null) ? "no collider" : $"collider isTrigger={col.isTrigger} layer={LayerMask.LayerToName(gameObject.layer)}";
        string rbInfo = (rb == null) ? "no rigidbody" : $"rb bodyType={rb.bodyType} sim={rb.simulated} kinematic={rb.isKinematic} g={rb.gravityScale}";

        Debug.Log($"Projectile spawn | team={team} dmg={damage} dir={direction} target={(target != null ? target.gameObject.name : "none")} | {colInfo} | {rbInfo}");
        
        // Distruge după 3 secunde dacă nu lovește nimic
        Destroy(gameObject, 3f);
    }
    
    void Update()
    {
        if (useBallisticArc)
        {
            // Traiectorie balistică: nu mai actualizăm direcția spre țintă după lansare.
            velocity.y += arcGravity * Time.deltaTime;
            transform.position += velocity * Time.deltaTime;

            float angleDeg = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg + rotationOffsetDeg;
            transform.rotation = Quaternion.AngleAxis(angleDeg, Vector3.forward);
        }
        else
        {
            if (target != null && target.currentHP > 0)
            {
                // Actualizează direcția spre țintă (homing ușor)
                direction = (target.transform.position - transform.position).normalized;
            }

            // Mișcă proiectilul
            transform.position += direction * speed * Time.deltaTime;

            // Actualizează și rotația pe traiectorie
            float angleDeg = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffsetDeg;
            transform.rotation = Quaternion.AngleAxis(angleDeg, Vector3.forward);
        }

        if (!loggedZeroDirection && direction.sqrMagnitude < 0.0001f)
        {
            loggedZeroDirection = true;
            Debug.Log($"Projectile stalled | team={team} target={(target != null ? target.gameObject.name : "none")} pos={transform.position}");
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        Unit hitUnit = collision.GetComponent<Unit>();
        
        if (hitUnit != null && hitUnit.team != team && hitUnit.currentHP > 0)
        {
            Debug.Log($"Projectile hit {hitUnit.gameObject.name} for {damage}");
            hitUnit.TakeDamage(damage);
            Destroy(gameObject); // Distruge proiectilul după impact
        }
    }
}
