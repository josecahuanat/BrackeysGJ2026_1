using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;
public class Inventory : MonoBehaviour
{
   [Header("Inventory")]
    private string currentItemID = null;
    private GameObject currentItemPrefab = null;
    
    [Header("Hand Transform")]
    [SerializeField] private Transform handTransform;
    private GameObject currentItemInstance;
    [Header("Drop Settings")]
    [SerializeField] private float dropDistance = 1.5f;  // Distancia frente al jugador
    [SerializeField] private float dropForce = 2f;       // Fuerza para lanzar el item
    [SerializeField] private bool dropWithPhysics = true; // Si lanzar o solo colocar
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    
    [Header("Events")]
    public UnityEvent<string> OnItemPickedUp;
    public UnityEvent<string> OnItemDropped;
    public UnityEvent<string, GameObject> OnItemUsed;

    public bool HasItem => currentItemID != null;
    public string CurrentItemID => currentItemID;

    void Start()
    {
        // VALIDACIÓN
        if (handTransform == null)
        {
            Debug.LogError("Hand Transform no asignado en Inventory!");
        }
    }

    public bool PickupItem(string itemID, GameObject itemPrefab)
    {
        if (HasItem)
        {
            Debug.Log("Ya tienes un objeto. Suelta el actual primero (Q).");
            return false;
        }
        
        currentItemID = itemID;
        currentItemPrefab = itemPrefab;
        
        // Instanciar visual en la mano
        if (itemPrefab != null && handTransform != null)
        {
            currentItemInstance = Instantiate(itemPrefab, handTransform);
            currentItemInstance.transform.localPosition = Vector3.zero;
            currentItemInstance.transform.localRotation = Quaternion.identity;
            currentItemInstance.transform.localScale = Vector3.one;
            
            // Desactivar colliders y scripts del objeto en la mano
            foreach (var col in currentItemInstance.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }
            
            // Desactivar scripts de interacción
            foreach (var interactable in currentItemInstance.GetComponentsInChildren<MonoBehaviour>())
            {
                if (interactable is IInteractable || interactable is IPickable)
                {
                    interactable.enabled = false;
                }
            }
        }
        
        UpdateUI();
        OnItemPickedUp?.Invoke(itemID);
        
        Debug.Log($"Recogido: {itemID}");
        return true;
    }

     public void DropItem()
    {
        if (!HasItem)
        {
            Debug.Log("No tienes ningún item para soltar");
            return;
        }
        
        string droppedItem = currentItemID;
        
        // ═══════════════════════════════════════════════════════
        // CREAR EL OBJETO EN EL MUNDO (frente al jugador)
        // ═══════════════════════════════════════════════════════
        
        if (currentItemPrefab != null)
        {
            // Calcular posición de drop (frente al jugador)
            Vector3 dropPosition = transform.position + transform.forward * dropDistance;
            dropPosition.y = transform.position.y; // Misma altura que el jugador
            
            // Instanciar el objeto en el mundo
            GameObject droppedObject = Instantiate(currentItemPrefab, dropPosition, Quaternion.identity);
            
            // Asegurar que tenga todos sus componentes activos
            foreach (var col in droppedObject.GetComponentsInChildren<Collider>())
            {
                col.enabled = true;
            }
            
            // Reactivar scripts de interacción
            foreach (var interactable in droppedObject.GetComponentsInChildren<MonoBehaviour>())
            {
                if (interactable is IInteractable || interactable is IPickable)
                {
                    interactable.enabled = true;
                }
            }
            
            Debug.Log($"Item '{droppedItem}' soltado en el mundo en posición: {dropPosition}");
        }
        
        // ═══════════════════════════════════════════════════════
        // LIMPIAR INVENTARIO
        // ═══════════════════════════════════════════════════════
        
        // Destruir el visual de la mano
        if (currentItemInstance != null)
        {
            Destroy(currentItemInstance);
        }
        
        // Resetear variables
        currentItemID = null;
        currentItemPrefab = null;
        currentItemInstance = null;
        
        UpdateUI();
        OnItemDropped?.Invoke(droppedItem);
        
        Debug.Log($"Soltado: {droppedItem}");
    }

    public bool UseItem(GameObject target)
    {
        if (!HasItem) return false;
        
        IUsable usable = target.GetComponent<IUsable>();
        if (usable != null && usable.CanBeUsedOn(target))
        {
            usable.Use(target, gameObject);
            OnItemUsed?.Invoke(currentItemID, target);
            DropItem();
            return true;
        }
        
        return false;
    }

    void UpdateUI()
    {
        if (itemNameText != null)
        {
            itemNameText.text = HasItem ? currentItemID : "Vacío";
            itemNameText.gameObject.SetActive(true);
        }
    }
}