using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;
public class Inventory : MonoBehaviour
{
    [Header("Inventory")]
    private string currentItemID = null;
    private GameObject currentItemObject = null;  // ← CAMBIO: ahora guarda el objeto real
    private Rigidbody currentItemRigidbody = null;

    [Header("Hand Transform")]
    [SerializeField] private Transform handTransform;
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
    public bool PickupItem(string itemID, GameObject itemObject)  // ← CAMBIO: recibe GameObject, no prefab
    {
        if (HasItem)
        {
            Debug.Log("Ya tienes un objeto. Suelta el actual primero (Q).");
            return false;
        }

        currentItemID = itemID;
        currentItemObject = itemObject;  // ← Guardar el objeto original

        // Desactivar física
        currentItemRigidbody = itemObject.GetComponent<Rigidbody>();
        if (currentItemRigidbody != null)
        {
            currentItemRigidbody.isKinematic = true;
        }

        // Desactivar colliders
        foreach (var col in itemObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        // Desactivar scripts de interacción
        foreach (var interactable in itemObject.GetComponentsInChildren<MonoBehaviour>())
        {
            if (interactable is IInteractable || interactable is IPickable)
            {
                interactable.enabled = false;
            }
        }

        // Mover a la mano
        if (handTransform != null)
        {
            itemObject.transform.SetParent(handTransform);
            itemObject.transform.localPosition = Vector3.zero;
            itemObject.transform.localRotation = Quaternion.identity;
            itemObject.transform.localScale = Vector3.one;
        }

        UpdateUI();
        OnItemPickedUp?.Invoke(itemID);
        Debug.Log($"Recogido: {itemID}");
        return true;
    }

    // ══════════ MODIFICAR DropItem() ══════════
    public void DropItem()
    {
        if (!HasItem || currentItemObject == null)
        {
            if (!HasItem)
                Debug.Log("No tienes ningún item para soltar");
            else
                Debug.LogWarning("Referencia de item perdida (probablemente ya fue usado)");
            return;
        }

        string droppedItem = currentItemID;

        if (currentItemObject != null)
        {
            // Calcular posición frente al jugador
            Vector3 dropPosition = transform.position + transform.forward * dropDistance;
            dropPosition.y = transform.position.y;

            // Desparentar del hand
            currentItemObject.transform.SetParent(null);
            currentItemObject.transform.position = dropPosition;
            currentItemObject.transform.rotation = Quaternion.identity;

            // Reactivar colliders
            foreach (var col in currentItemObject.GetComponentsInChildren<Collider>())
            {
                col.enabled = true;
            }

            // Reactivar scripts
            foreach (var interactable in currentItemObject.GetComponentsInChildren<MonoBehaviour>())
            {
                if (interactable is IInteractable || interactable is IPickable)
                {
                    interactable.enabled = true;
                }
            }

            // Reactivar física
            if (currentItemRigidbody != null)
            {
                currentItemRigidbody.isKinematic = false;

                if (dropWithPhysics)
                {
                    Vector3 dropDirection = transform.forward + Vector3.up * 0.3f;
                    currentItemRigidbody.AddForce(dropDirection * dropForce, ForceMode.Impulse);
                }
            }

            Debug.Log($"Item '{droppedItem}' soltado en posición: {dropPosition}");
        }

        // Limpiar referencias
        currentItemID = null;
        currentItemObject = null;
        currentItemRigidbody = null;

        UpdateUI();
        OnItemDropped?.Invoke(droppedItem);
    }

    // ══════════ NUEVO MÉTODO: GetCurrentItem() ══════════
    public GameObject GetCurrentItemObject()
    {
        return currentItemObject;
    }

    public bool UseItem(GameObject target)
    {
        if (!HasItem) return false;
        
        IUsable usable = target.GetComponent<IUsable>();
        if (usable != null && usable.CanBeUsedOn(target))
        {
            usable.Use(target, gameObject);
            OnItemUsed?.Invoke(currentItemID, target);
            return true;
        }
        
        return false;
    }
    public void ClearInventory()
    {
        // (usado cuando el objeto se coloca en un socket)
        currentItemID = null;
        currentItemObject = null;
        currentItemRigidbody = null;
        UpdateUI();
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