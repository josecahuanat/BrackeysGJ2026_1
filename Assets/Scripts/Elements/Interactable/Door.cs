using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private string requiredKeyID = "MainKey";
    // [SerializeField] private Animator doorAnimator;
    private bool isOpen = false;

    public string InteractPrompt 
    { 
        get 
        {
            if (isOpen) return "Puerta abierta";
            return "Presiona E para abrir (se necesita llave)";
        }
    }
    
    public bool CanInteract => !isOpen;

    public void Interact(GameObject player)
    {
        Inventory inventory = player.GetComponent<Inventory>();
        
        if (inventory.HasItem(requiredKeyID))
        {
            OpenDoor();
            inventory.RemoveItem(requiredKeyID);
        }
        else
        {
            Debug.Log("Necesitas una llave!");
        }
    }

    void OpenDoor()
    {
        isOpen = true;
        // doorAnimator.SetTrigger("Open");
        Debug.Log("puerta abierta");
    }
}