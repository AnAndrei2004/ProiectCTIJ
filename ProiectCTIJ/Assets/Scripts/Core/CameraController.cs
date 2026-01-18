using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    public float scrollSpeed = 10f;        // Viteza de miscare
    public float edgeThreshold = 50f;      // Cati pixeli de la margine pentru a activa scroll-ul
    public float minX = -10f;              // Limita stanga
    public float maxX = 10f;               // Limita dreapta
    
    private Camera cam;
    
    // Initializeaza referinta la camera.
    void Start()
    {
        cam = GetComponent<Camera>();
        
        // Poti seta automat limitele bazate pe dimensiunea canvas-ului/background-ului
        // Seteaza manual in Inspector pentru acum
    }
    
    // Miscare camera in functie de pozitia mouse-ului.
    void Update()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Vector3 moveDirection = Vector3.zero;
        
        // Verifica daca mouse-ul e aproape de marginea dreapta
        if (mousePos.x >= Screen.width - edgeThreshold)
        {
            moveDirection.x = 1f; // Muta camera la dreapta
        }
        // Verifica daca mouse-ul e aproape de marginea stanga
        else if (mousePos.x <= edgeThreshold)
        {
            moveDirection.x = -1f; // Muta camera la stanga
        }
        
        // Aplica miscarea
        if (moveDirection != Vector3.zero)
        {
            Vector3 newPosition = transform.position + moveDirection * scrollSpeed * Time.deltaTime;
            
            // Limiteaza pozitia camerei intre minX si maxX
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            
            transform.position = newPosition;
        }
    }
    
    // Ajuta la debugging - deseneaza zonele active
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        
        // Arata limita stanga
        Gizmos.DrawLine(new Vector3(minX, -100, 0), new Vector3(minX, 100, 0));
        
        // Arata limita dreapta
        Gizmos.DrawLine(new Vector3(maxX, -100, 0), new Vector3(maxX, 100, 0));
    }
}
