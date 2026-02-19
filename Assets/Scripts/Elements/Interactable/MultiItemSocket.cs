using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class MultiItemSocket : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class Slot
    {
        public string itemRequerido;          // qué item acepta este slot
        public Transform puntoDeDisplay;      // dónde se coloca visualmente
        [HideInInspector] public GameObject itemInstance;
        [HideInInspector] public bool ocupado;
    }

    [Header("Configuración")]
    [SerializeField] private List<Slot> slots = new();
    [SerializeField] private bool consumirItems = true; // false = se pueden retirar

    [Header("Eventos - IInteractable")]
    [SerializeField] private UnityEvent<GameObject> onInteracted;

    [Header("Eventos propios")]
    public UnityEvent OnSlotOcupado;          // cada vez que se llena un slot
    public UnityEvent OnSlotVaciado;
    public UnityEvent OnTodosLlenos;          // todos los slots llenos → puzzle
    public UnityEvent OnReseteado;

    // ── IInteractable ──────────────────────────────────────
    public bool CanInteract => true;
    public UnityEvent<GameObject> OnInteracted => onInteracted;

    public string InteractPrompt
    {
        get
        {
            int llenos = ContarLlenos();
            int total  = slots.Count;

            if (llenos == total)
                return consumirItems ? "Completo" : "Presiona E para retirar";

            // Si el jugador lleva algo, dime si cabe aquí
            return $"Coloca items: {llenos}/{total}";
        }
    }

    public void Interact(GameObject player)
    {
        Inventory inv = player.GetComponent<Inventory>();
        if (inv == null) return;

        int llenos = ContarLlenos();

        if (inv.HasItem)
        {
            // Intenta colocar el item en un slot libre que lo acepte
            Slot slotLibre = GetSlotLibreParaItem(inv.CurrentItemID);
            if (slotLibre != null)
            {
                ColocarItem(slotLibre, inv);
            }
            else
            {
                string needed = GetNecesitados();
                GameUIManager.Instance?.ShowFeedback(
                    llenos == slots.Count
                        ? "Ya está completo"
                        : $"Necesitas: {needed}");
            }
        }
        else if (!consumirItems && llenos > 0)
        {
            // Sin item en mano → retira el último colocado
            Slot ultimo = GetUltimoOcupado();
            if (ultimo != null) RetirarItem(ultimo, inv);
        }
        else
        {
            string needed = GetNecesitados();
            GameUIManager.Instance?.ShowFeedback($"Necesitas: {needed}");
        }

        onInteracted?.Invoke(player);
    }
    // ───────────────────────────────────────────────────────

    void ColocarItem(Slot slot, Inventory inv)
    {
        slot.ocupado = true;
        slot.itemInstance = inv.GetCurrentItemObject();

        if (slot.itemInstance != null && slot.puntoDeDisplay != null)
        {
            slot.itemInstance.transform.SetParent(slot.puntoDeDisplay);
            slot.itemInstance.transform.localPosition = Vector3.zero;
            slot.itemInstance.transform.localRotation = Quaternion.identity;
            slot.itemInstance.transform.localScale    = Vector3.one;

            foreach (var col in slot.itemInstance.GetComponentsInChildren<Collider>())
                col.enabled = false;
        }

        inv.ClearInventory();
        OnSlotOcupado?.Invoke();

        if (ContarLlenos() == slots.Count)
            OnTodosLlenos?.Invoke();
    }

    void RetirarItem(Slot slot, Inventory inv)
    {
        if (!inv.PickupItem(slot.itemRequerido, slot.itemInstance)) return;

        slot.ocupado      = false;
        slot.itemInstance = null;
        OnSlotVaciado?.Invoke();
    }

    public void Resetear()
    {
        foreach (var slot in slots)
        {
            if (slot.itemInstance != null)
                Destroy(slot.itemInstance);
            slot.ocupado      = false;
            slot.itemInstance = null;
        }
        OnReseteado?.Invoke();
    }

    // ── Helpers ────────────────────────────────────────────
    int ContarLlenos()
    {
        int c = 0;
        foreach (var s in slots) if (s.ocupado) c++;
        return c;
    }

    Slot GetSlotLibreParaItem(string itemID)
    {
        foreach (var s in slots)
            if (!s.ocupado && s.itemRequerido == itemID)
                return s;
        return null;
    }

    Slot GetUltimoOcupado()
    {
        for (int i = slots.Count - 1; i >= 0; i--)
            if (slots[i].ocupado) return slots[i];
        return null;
    }

    string GetNecesitados()
    {
        // Agrupa los items que faltan: "Flower x2, Candle x1"
        Dictionary<string, int> faltantes = new();
        foreach (var s in slots)
        {
            if (s.ocupado) continue;
            if (!faltantes.ContainsKey(s.itemRequerido))
                faltantes[s.itemRequerido] = 0;
            faltantes[s.itemRequerido]++;
        }
        var partes = new List<string>();
        foreach (var kv in faltantes)
            partes.Add(kv.Value > 1 ? $"{kv.Key} x{kv.Value}" : kv.Key);
        return string.Join(", ", partes);
    }

    public bool EstaCompleto() => ContarLlenos() == slots.Count;
}