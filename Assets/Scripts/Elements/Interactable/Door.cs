using UnityEngine;
using UnityEngine.Events;
public class Door : MonoBehaviour, IInteractable, IUsable
{
    [Header("Door Settings")]
    [SerializeField] private string requiredKeyID = "GoldenKey";
    [SerializeField] private Animator doorAnimator;
    
    [Header("Events")]
    public UnityEvent<GameObject> onInteracted;
    public UnityEvent<GameObject, GameObject> onUnlocked;
    
    private bool isUnlocked = false;
     private bool isOpen = false;

    public string InteractPrompt 
    {
        get
        {
            if (isOpen) return "Puerta abierta";
            if (isUnlocked) return "Presiona E para abrir";
            return "Puerta cerrada (necesitas una llave)";
        }
    }
    
    public bool CanInteract => true;
    public string ItemName => requiredKeyID;
    public UnityEvent<GameObject> OnInteracted => onInteracted;
    public UnityEvent<GameObject, GameObject> OnItemUsed => onUnlocked;

    public void Interact(GameObject player)
    {
        if (isUnlocked)
        {
           // OpenDoor();
        }
        else
        {
            Inventory inventory = player.GetComponent<Inventory>();
            
            if (inventory.HasItem && inventory.CurrentItemID == requiredKeyID)
            {
                Use(gameObject, player);
            }
            else
            {
                Debug.Log($"Necesitas: {requiredKeyID}");
            }
        }
        
        OnInteracted?.Invoke(player);
    }

    public bool CanBeUsedOn(GameObject target)
    {
        return target == gameObject && !isUnlocked;
    }

    public void Use(GameObject target, GameObject player)
{
        if (isUnlocked) return; // Ya está desbloqueada
        
        Inventory inventory = player.GetComponent<Inventory>();
        
        if (inventory != null && inventory.HasItem && inventory.CurrentItemID == requiredKeyID)
        {
            isUnlocked = true;
            Debug.Log("¡Puerta desbloqueada!");
            
            // Consumir la llave
            inventory.DropItem();
            
            OnItemUsed?.Invoke(target, player);
            
            // Abrir automáticamente después de desbloquear
            OpenDoor();
        }
    }

    void OpenDoor()
    {
        if (isOpen) return;
        
        isOpen = true;
        
        // VALIDACIÓN: verificar que el animator existe
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }
        else
        {
            Debug.LogWarning("No hay Animator asignado en la puerta");
            // Alternativa: desactivar el collider o mover la puerta
            Collider doorCollider = GetComponent<Collider>();
            if (doorCollider != null)
            {
                doorCollider.enabled = false;
            }
        }
    }
}