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
            Debug.LogError("Hand Transform no asignado en SingleItemInventory!");
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
        if (!HasItem) return;
        
        string droppedItem = currentItemID;
        
        // Destruir visual
        if (currentItemInstance != null)
        {
            Destroy(currentItemInstance);
        }
        
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