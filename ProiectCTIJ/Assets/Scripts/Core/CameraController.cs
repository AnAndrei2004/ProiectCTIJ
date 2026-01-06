using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    public float scrollSpeed = 10f;        // Viteza de mișcare
    public float edgeThreshold = 50f;      // Câți pixeli de la margine pentru a activa scroll-ul
    public float minX = -10f;              // Limita stângă
    public float maxX = 10f;               // Limita dreaptă
    
    private Camera cam;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        
        // Poți seta automat limitele bazate pe dimensiunea canvas-ului/background-ului
        // Setează manual în Inspector pentru acum
    }
    
    void Update()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Vector3 moveDirection = Vector3.zero;
        
        // Check dacă mouse-ul e aproape de marginea dreaptă
        if (mousePos.x >= Screen.width - edgeThreshold)
        {
            moveDirection.x = 1f; // Mișcă camera la dreapta
        }
        // Check dacă mouse-ul e aproape de marginea stângă
        else if (mousePos.x <= edgeThreshold)
        {
            moveDirection.x = -1f; // Mișcă camera la stânga
        }
        
        // Aplică mișcarea
        if (moveDirection != Vector3.zero)
        {
            Vector3 newPosition = transform.position + moveDirection * scrollSpeed * Time.deltaTime;
            
            // Limitează poziția camerei între minX și maxX
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            
            transform.position = newPosition;
        }
    }
    
    // Ajută la debugging - desenează zonele active
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        
        // Arată limita stângă
        Gizmos.DrawLine(new Vector3(minX, -100, 0), new Vector3(minX, 100, 0));
        
        // Arată limita dreaptă
        Gizmos.DrawLine(new Vector3(maxX, -100, 0), new Vector3(maxX, 100, 0));
    }
}
