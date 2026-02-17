using UnityEngine;
using UnityEngine.Events;
using TMPro;
public class ItemSocket : MonoBehaviour, IInteractable, IUsable
{
    [Header("Configuración")]
    [SerializeField] private string itemRequerido = "CrystalOrb";
    [SerializeField] private Transform puntoDeDisplay;
    [SerializeField] private bool consumirItem = true;

    [Header("Eventos - IInteractable")]
    [SerializeField] private UnityEvent<GameObject> onInteracted;

    [Header("Eventos - IUsable")]
    [SerializeField] private UnityEvent<GameObject, GameObject> onItemUsed;

    [Header("Eventos propios")]
    public UnityEvent OnItemColocado;
    public UnityEvent OnItemRetirado;

    // ── IInteractable ──────────────────────────────────────
    public string InteractPrompt => tieneItem
        ? (consumirItem ? "Slot ocupado" : "Presiona E para retirar")
        : $"Coloca {itemRequerido} aquí";
    public bool   CanInteract                  => true;
    public UnityEvent<GameObject> OnInteracted => onInteracted;

    public void Interact(GameObject player)
    {
        Inventory inv = player.GetComponent<Inventory>();
        if (inv == null) return;

        if (!tieneItem)
        {
            // Intentar colocar usando IUsable
            if (inv.HasItem && inv.CurrentItemID == itemRequerido)
            {
                Use(gameObject, player);
            }
            else
            {
                Debug.Log($"Necesitas: {itemRequerido}");
            }
        }
        else if (!consumirItem)
        {
            tieneItem = false;
            if (itemInstance) Destroy(itemInstance);
            OnItemRetirado?.Invoke();
        }

        onInteracted?.Invoke(player);
    }
    // ───────────────────────────────────────────────────────

    // ── IUsable ────────────────────────────────────────────
    public string ItemName                                   => itemRequerido;
    public UnityEvent<GameObject, GameObject> OnItemUsed     => onItemUsed;

    public bool CanBeUsedOn(GameObject target) => target == gameObject && !tieneItem;

    public void Use(GameObject target, GameObject player)
    {
        Inventory inv = player.GetComponent<Inventory>();
        if (inv == null || !inv.HasItem) return;

        tieneItem = true;

        // Mostrar visual del item en el altar
        if (puntoDeDisplay != null)
        {
            // Clonar el prefab visual en el altar (sin destruir el inventario todavía)
            itemInstance = new GameObject($"{itemRequerido}_Display");
            itemInstance.transform.SetPositionAndRotation(puntoDeDisplay.position, puntoDeDisplay.rotation);
            itemInstance.transform.SetParent(puntoDeDisplay);
        }

        if (consumirItem) inv.DropItem();

        onItemUsed?.Invoke(target, player);
        OnItemColocado?.Invoke();
    }
    // ───────────────────────────────────────────────────────

    private bool tieneItem = false;
    private GameObject itemInstance;
}