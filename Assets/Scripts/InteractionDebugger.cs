using UnityEngine;

public class InteractionDebugger : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private float checkRange = 10f;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            DebugPlayerState();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            DebugNearbyTombs();
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            DebugRaycast();
        }
    }
    
    void DebugPlayerState()
    {
        Debug.Log("=== PLAYER DEBUG ===");
        
        if (playerInteraction == null)
        {
            playerInteraction = GetComponent<PlayerInteraction>();
        }
        
        if (playerInteraction == null)
        {
            Debug.LogError("❌ NO se encontró PlayerInteraction!");
            return;
        }
        else
        {
            Debug.Log("✓ PlayerInteraction encontrado");
        }
        
        // Verificar ProximityDetector
        ProximityDetector proximity = GetComponentInChildren<ProximityDetector>();
        if (proximity == null)
        {
            Debug.LogWarning("❌ NO se encontró ProximityDetector en los hijos!");
        }
        else
        {
            Debug.Log("✓ ProximityDetector encontrado");
            SphereCollider sphereCol = proximity.GetComponent<SphereCollider>();
            if (sphereCol != null)
            {
                Debug.Log($"  - Radius: {sphereCol.radius}");
                Debug.Log($"  - IsTrigger: {sphereCol.isTrigger}");
            }
        }
        
        // Verificar Inventory
        Inventory inv = GetComponent<Inventory>();
        if (inv == null)
        {
            Debug.LogWarning("❌ NO se encontró Inventory!");
        }
        else
        {
            Debug.Log("✓ Inventory encontrado");
        }
        
        // Verificar Camera
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("❌ NO se encontró Main Camera!");
        }
        else
        {
            Debug.Log($"✓ Main Camera encontrada: {cam.name}");
        }
    }
    
    void DebugNearbyTombs()
    {
        Debug.Log("=== TUMBAS CERCANAS ===");
        
        Tomb[] allTombs = FindObjectsOfType<Tomb>();
        Debug.Log($"Total de tumbas en la escena: {allTombs.Length}");
        
        int nearbyCount = 0;
        foreach (Tomb tomb in allTombs)
        {
            float dist = Vector3.Distance(transform.position, tomb.transform.position);
            if (dist <= checkRange)
            {
                nearbyCount++;
                Debug.Log($"  - {tomb.name}: distancia={dist:F2}m, CanInteract={tomb.CanInteract}, IsLit={tomb.IsLit}");
                
                // Verificar collider
                Collider col = tomb.GetComponent<Collider>();
                if (col == null)
                {
                    Debug.LogError($"    ❌ {tomb.name} NO tiene Collider!");
                }
                else
                {
                    Debug.Log($"    ✓ Collider: {col.GetType().Name}, IsTrigger={col.isTrigger}, Layer={LayerMask.LayerToName(tomb.gameObject.layer)}");
                }
            }
        }
        
        Debug.Log($"Tumbas a menos de {checkRange}m: {nearbyCount}");
    }
    
    void DebugRaycast()
    {
        Debug.Log("=== RAYCAST DEBUG ===");
        
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No hay Main Camera");
            return;
        }
        
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        Debug.Log($"Ray Origin: {ray.origin}");
        Debug.Log($"Ray Direction: {ray.direction}");
        
        // Raycast sin layer mask
        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            Debug.Log($"✓ Hit algo: {hit.collider.name}");
            Debug.Log($"  - Distance: {hit.distance}");
            Debug.Log($"  - Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                Debug.Log($"  ✓ Tiene IInteractable!");
                Debug.Log($"    - CanInteract: {interactable.CanInteract}");
                Debug.Log($"    - Prompt: {interactable.InteractPrompt}");
            }
            else
            {
                Debug.LogWarning($"  ❌ NO tiene IInteractable!");
            }
        }
        else
        {
            Debug.Log("❌ Raycast no detectó nada");
        }
        
        // Raycast con todos los layers
        RaycastHit[] allHits = Physics.RaycastAll(ray, 10f);
        Debug.Log($"Total objetos en el camino del ray: {allHits.Length}");
        foreach (var h in allHits)
        {
            Debug.Log($"  - {h.collider.name} (Layer: {LayerMask.LayerToName(h.collider.gameObject.layer)})");
        }
    }
}
