using UnityEngine;

public class Key : MonoBehaviour, IInteractable, IPickable
{
    [SerializeField] private string keyID = "MainKey";
    
    public string InteractPrompt => "Presiona E para recoger llave";
    public bool CanInteract => true;
    public string ItemName => keyID;

    public void Interact(GameObject player)
    {
        OnPickup(player);
    }

    public void OnPickup(GameObject player)
    {
        // Agregar al inventario
        Inventory inventory = player.GetComponent<Inventory>();
        inventory?.AddItem(keyID);
        
        // Destruir objeto visual
        Destroy(gameObject);
    }
}