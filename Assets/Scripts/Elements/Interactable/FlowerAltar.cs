using UnityEngine;
using UnityEngine.Events;

public class FlowerAltar : MonoBehaviour, IInteractable, ITriggerable
{
    [Header("Configuración")]
    [SerializeField] private string itemRequerido = "Flower";
    [SerializeField] private Transform puntoDisplay;
    [SerializeField] private bool consumirFlor = true;

    [Header("Visual")]
    [SerializeField] private GameObject visualOcupado;   // se activa al colocar la flor
    [SerializeField] private GameObject visualVacio;     // se desactiva al colocar

    [Header("Eventos - IInteractable")]
    [SerializeField] private UnityEvent<GameObject> onInteracted;

    [Header("Eventos - ITriggerable")]
    [SerializeField] private UnityEvent<GameObject, GameObject> onItemUsed;

    [Header("Eventos propios")]
    public UnityEvent OnFlorColocada;
    public UnityEvent OnFlorRetirada;

    private bool tieneFlor = false;
    private GameObject florInstance;

    // ── IInteractable ──────────────────────────────────────
    public bool CanInteract => true;
    public string InteractPrompt => tieneFlor
        ? (consumirFlor ? "Sitio ocupado" : "Retirar flor")
        : $"Colocar {itemRequerido}";
    public UnityEvent<GameObject> OnInteracted => onInteracted;

    public void Interact(GameObject player)
    {
        Inventory inv = player.GetComponent<Inventory>();
        if (inv == null) return;

        if (!tieneFlor)
        {
            if (inv.HasItem && inv.CurrentItemID == itemRequerido)
                Use(gameObject, player);
            else
                Debug.Log($"Necesitas: {itemRequerido}");
        }
        else if (!consumirFlor)
        {
            if (inv.HasItem) { Debug.Log("Suelta tu item primero (Q)"); return; }
            inv.PickupItem(itemRequerido, florInstance);
            tieneFlor = false;
            florInstance = null;
            if (visualOcupado) visualOcupado.SetActive(false);
            if (visualVacio)   visualVacio.SetActive(true);
            OnFlorRetirada?.Invoke();
        }

        onInteracted?.Invoke(player);
    }
    // ───────────────────────────────────────────────────────

    // ── ITriggerable ───────────────────────────────────────
    public string ItemName => itemRequerido;
    public UnityEvent<GameObject, GameObject> OnItemUsed => onItemUsed;
    public bool CanBeUsedOn(GameObject target) => target == gameObject && !tieneFlor;

    public void Use(GameObject target, GameObject player)
    {
        Inventory inv = player.GetComponent<Inventory>();
        if (inv == null || !inv.HasItem) return;

        tieneFlor = true;
        florInstance = inv.GetCurrentItemObject();

        if (florInstance != null && puntoDisplay != null)
        {
            florInstance.transform.SetParent(puntoDisplay);
            florInstance.transform.localPosition = Vector3.zero;
            florInstance.transform.localRotation = Quaternion.identity;
            foreach (var col in florInstance.GetComponentsInChildren<Collider>())
                col.enabled = false;
        }

        inv.ClearInventory();
        if (visualOcupado) visualOcupado.SetActive(true);
        if (visualVacio)   visualVacio.SetActive(false);

        onItemUsed?.Invoke(target, player);
        OnFlorColocada?.Invoke();
    }
    // ───────────────────────────────────────────────────────

    public void Resetear()
    {
        if (florInstance != null) Destroy(florInstance);
        tieneFlor = false;
        florInstance = null;
        if (visualOcupado) visualOcupado.SetActive(false);
        if (visualVacio)   visualVacio.SetActive(true);
        OnFlorRetirada?.Invoke();
    }
}