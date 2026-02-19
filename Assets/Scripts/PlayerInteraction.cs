using UnityEngine;
using TMPro;
using System.Collections.Generic;
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
     // Objetos cercanos detectados por trigger/collider
    private List<IInteractable> nearbyInteractables = new List<IInteractable>();
    private IInteractable currentInteractable;
    private GameObject currentInteractableObject; // NUEVO: guardamos el GameObject
    private Inventory inventory;
    private Camera mainCamera;
    private enum DetectionSource { None, Raycast, Proximity }
    private DetectionSource currentSource = DetectionSource.None;

    void Start()
    {
        mainCamera = Camera.main;
        inventory = GetComponent<Inventory>();
        
        if (inventory == null)
        {
            Debug.LogError("PlayerInteraction necesita Inventory");
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
        // ── Prioridad 1: Raycast (el jugador está mirando el objeto) ──
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        if (showDebugRay) Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.yellow);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            IInteractable byRay = hit.collider.GetComponent<IInteractable>();
            if (byRay != null && byRay.CanInteract)
            {
                SetCurrent(byRay, hit.collider.gameObject, DetectionSource.Raycast);
                return;
            }
        }

        // ── Prioridad 2: Proximidad (el jugador está cerca del objeto) ──
        // Limpia los que ya no existen
        nearbyInteractables.RemoveAll(x => x == null || (x is MonoBehaviour mb && mb == null));

        if (nearbyInteractables.Count > 0)
        {
            // Toma el más cercano de los que están en rango
            IInteractable closest    = null;
            GameObject    closestObj = null;
            float         closestDist = float.MaxValue;

            foreach (var nearby in nearbyInteractables)
            {
                if (!nearby.CanInteract) continue;
                MonoBehaviour mb = nearby as MonoBehaviour;
                if (mb == null) continue;

                float dist = Vector3.Distance(transform.position, mb.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest     = nearby;
                    closestObj  = mb.gameObject;
                }
            }

            if (closest != null)
            {
                SetCurrent(closest, closestObj, DetectionSource.Proximity);
                return;
            }
        }

        // ── Nada detectado ────────────────────────────────
        SetCurrent(null, null, DetectionSource.None);
    }
  void SetCurrent(IInteractable interactable, GameObject obj, DetectionSource source)
    {
        currentInteractable       = interactable;
        currentInteractableObject = obj;
        currentSource             = source;

        if (interactable != null)
        {
            interactPromptUI?.SetActive(true);
            if (promptText != null)
                promptText.text = interactable.InteractPrompt;
                 bool esItemSinProgreso = false;
            foreach (var tipo in sinProgresoPuzzle)
                if (obj.GetComponent(tipo) != null) { esItemSinProgreso = true; break; }

            MultiConditionPuzzle puzzle = obj != null
            ? obj.GetComponentInParent<MultiConditionPuzzle>()
            : null;

            esItemSinProgreso = false;
            foreach (var tipo in sinProgresoPuzzle)
                if (obj != null && obj.GetComponent(tipo) != null) { esItemSinProgreso = true; break; }

            if (puzzle != null && !esItemSinProgreso)
            {
                GameUIManager.Instance?.ShowPuzzleObjectiveUI();
                GameUIManager.Instance?.UpdatePuzzleProgress(
                    $"{puzzle.ContarCumplidas()} / {puzzle.TotalCondiciones()}",
                    puzzle.ObtenerPistaActual()
                );
            }
            else
            {
                GameUIManager.Instance?.HidePuzzleObjectiveUI();
            }
        }
        else
        {
            interactPromptUI?.SetActive(false);
        }
    }

    // ── Llamado por ProximityDetector (componente en el Player) ──
    public void OnNearbyEnter(IInteractable interactable)
    {
        if (!nearbyInteractables.Contains(interactable))
            nearbyInteractables.Add(interactable);
    }

    public void OnNearbyExit(IInteractable interactable)
    {
        nearbyInteractables.Remove(interactable);
    }
    private readonly System.Type[] sinProgresoPuzzle = {
    typeof(Key),
    typeof(Candle)
    };
}