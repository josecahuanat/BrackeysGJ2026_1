using UnityEngine;
using TMPro;
public class PlayerInteraction: MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    
    [Header("UI")]
    [SerializeField] private GameObject interactPromptUI;
    [SerializeField] private TextMeshProUGUI promptText;
    
    private IInteractable currentInteractable;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        interactPromptUI.SetActive(false);
    }

    void Update()
    {
        CheckForInteractable();
        
        if (Input.GetKeyDown(interactKey) && currentInteractable != null)
        {
            if (currentInteractable.CanInteract)
            {
                currentInteractable.Interact(gameObject);
            }
        }
    }

    void CheckForInteractable()
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable != null && interactable.CanInteract)
            {
                SetCurrentInteractable(interactable);
                return;
            }
        }
        
        SetCurrentInteractable(null);
    }

    void SetCurrentInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;
        
        if (interactable != null)
        {
            interactPromptUI.SetActive(true);
            promptText.text = interactable.InteractPrompt;
        }
        else
        {
            interactPromptUI.SetActive(false);
        }
    }
}
