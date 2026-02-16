using UnityEngine;
using UnityEngine.Events;
public class Key : MonoBehaviour, IInteractable, IPickable
{
    [Header("Item Settings")]
    [SerializeField] private string keyID = "GoldenKey";
    [SerializeField] private GameObject handModelPrefab;
    
    [Header("Events")]
    public UnityEvent<GameObject> onInteracted;
    public UnityEvent<GameObject> onPickedUp;

    public string InteractPrompt => $"Presiona E para recoger {keyID}";
    public bool CanInteract => true;
    public string ItemName => keyID;
    public GameObject ItemPrefab => handModelPrefab;
    public UnityEvent<GameObject> OnInteracted => onInteracted;
    public UnityEvent<GameObject> OnItemPickedUp => onPickedUp;

    public void Interact(GameObject player)
    {
        OnPickup(player);
    }

 public void OnPickup(GameObject player)
    {
        Inventory inventory = player.GetComponent<Inventory>();
        
        if (inventory != null && inventory.PickupItem(keyID, handModelPrefab))
        {
            OnItemPickedUp?.Invoke(player);
            OnInteracted?.Invoke(player); // MOVER AQUÍ
            Destroy(gameObject); // Destruir DESPUÉS de invocar eventos
        }
        else
        {
            Debug.LogWarning("No se pudo recoger el item");
        }
    }
}