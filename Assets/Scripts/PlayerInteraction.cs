using UnityEngine;
using TMPro;
public class PlayerInteraction: MonoBehaviour
{
 [Header("Interaction")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode dropKey = KeyCode.Q;
    
    [Header("UI")]
    [SerializeField] private GameObject interactPromptUI;
    [SerializeField] private TextMeshProUGUI promptText;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;
    
    private IInteractable currentInteractable;
    private GameObject currentInteractableObject; // NUEVO: guardamos el GameObject
    private Inventory inventory;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        inventory = GetComponent<Inventory>();
        
        if (inventory == null)
        {
            Debug.LogError("PlayerInteraction necesita SingleItemInventory en el mismo GameObject!");
        }
        
        if (interactPromptUI != null)
        {
            interactPromptUI.SetActive(false);
        }
    }

    void Update()
    {
        CheckForInteractable();
        
        // Interactuar
        if (Input.GetKeyDown(interactKey) && currentInteractable != null)
        {
            if (currentInteractable.CanInteract)
            {
                Debug.Log($"Interactuando con: {currentInteractableObject.name}");
                currentInteractable.Interact(gameObject);
            }
        }
        
        // Soltar item
        if (Input.GetKeyDown(dropKey) && inventory != null && inventory.HasItem)
        {
            inventory.DropItem();
        }
    }

    void CheckForInteractable()
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        
        // Debug visual
        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.yellow);
        }
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable != null && interactable.CanInteract)
            {
                SetCurrentInteractable(interactable, hit.collider.gameObject);
                return;
            }
        }
        
        SetCurrentInteractable(null, null);
    }

    void SetCurrentInteractable(IInteractable interactable, GameObject obj)
    {
        currentInteractable = interactable;
        currentInteractableObject = obj;
        
        if (interactable != null)
        {
            if (interactPromptUI != null)
            {
                interactPromptUI.SetActive(true);
                promptText.text = interactable.InteractPrompt;
            }
        }
        else
        {
            if (interactPromptUI != null)
            {
                interactPromptUI.SetActive(false);
            }
        }
    }
}