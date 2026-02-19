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
        Debug.Log($"[Socket] Tiene item: {inv.HasItem} | ID inventario: '{inv.CurrentItemID}' | Requerido: '{itemRequerido}' | Iguales: {inv.CurrentItemID == itemRequerido}");
        if (!tieneItem)
        {
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
            // ═══ CORRECCIÓN: Retirar el item del socket ═══
            if (itemInstance != null && inv != null && !inv.HasItem)  // ← Verificar que el inventario esté vacío
            {
                // Primero: recoger el item (esto lo mueve a la mano)
                bool recogido = inv.PickupItem(itemRequerido, itemInstance);

                if (recogido)
                {
                    // Solo después limpiamos el socket
                    tieneItem = false;
                    itemInstance = null;
                    OnItemRetirado?.Invoke();
                    Debug.Log($"Item {itemRequerido} retirado del socket");
                }
            }
            else if (inv.HasItem)
            {
                Debug.Log("Ya tienes un item. Suelta el actual primero (Q).");
            }
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

        // Obtener el objeto real del inventario
        GameObject itemObject = inv.GetCurrentItemObject();

        if (itemObject != null && puntoDeDisplay != null)
        {
            // Mover el objeto al socket (desparentar del hand primero)
            itemObject.transform.SetParent(puntoDeDisplay);
            itemObject.transform.localPosition = Vector3.zero;
            itemObject.transform.localRotation = Quaternion.identity;
            itemObject.transform.localScale = Vector3.one;

            // Desactivar colliders (el objeto ya está "colocado")
            foreach (var col in itemObject.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            // Guardar referencia
            itemInstance = itemObject;
        }

        // Limpiar inventario sin soltar el objeto físicamente
        inv.ClearInventory();
        onItemUsed?.Invoke(target, player);
        OnItemColocado?.Invoke();
    }
    // ───────────────────────────────────────────────────────

    private bool tieneItem = false;
    private GameObject itemInstance;
}