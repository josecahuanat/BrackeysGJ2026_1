using UnityEngine;
using UnityEngine.Events;

public class TombPiece : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private string nombrePieza = "Lápida";
    [SerializeField] private string itemRequerido = "";   // vacío = no necesita item
    
    [Header("Visual")]
    [SerializeField] private GameObject visualCompletado;
    [SerializeField] private GameObject visualPendiente;

    [Header("Eventos")]
    [SerializeField] private UnityEvent<GameObject> onInteracted;
    public UnityEvent OnPiezaCompletada;
    public UnityEvent OnPiezaReseteada;

    public bool EstaCompletada { get; private set; } = false;

    // ── IInteractable ──────────────────────────────────────
    public bool CanInteract => !EstaCompletada;
    public string InteractPrompt => EstaCompletada
        ? $"{nombrePieza} completada"
        : string.IsNullOrEmpty(itemRequerido)
            ? $"Completar: {nombrePieza}"
            : $"Necesitas {itemRequerido} para {nombrePieza}";
    public UnityEvent<GameObject> OnInteracted => onInteracted;

    public void Interact(GameObject player)
    {
        if (EstaCompletada) return;

        if (!string.IsNullOrEmpty(itemRequerido))
        {
            Inventory inv = player.GetComponent<Inventory>();
            if (inv == null || !inv.HasItem || inv.CurrentItemID != itemRequerido)
            {
                Debug.Log($"Necesitas: {itemRequerido}");
                return;
            }
            inv.DropItem(); // consume el item
        }

        Completar();
        onInteracted?.Invoke(player);
    }
    // ───────────────────────────────────────────────────────

    void Completar()
    {
        EstaCompletada = true;
        if (visualCompletado) visualCompletado.SetActive(true);
        if (visualPendiente)  visualPendiente.SetActive(false);
        OnPiezaCompletada?.Invoke();
    }

    public void Resetear()
    {
        EstaCompletada = false;
        if (visualCompletado) visualCompletado.SetActive(false);
        if (visualPendiente)  visualPendiente.SetActive(true);
        OnPiezaReseteada?.Invoke();
    }
}