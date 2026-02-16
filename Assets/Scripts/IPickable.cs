using UnityEngine;
using UnityEngine.Events;
public interface  IPickable
{
    string ItemName { get; }
    GameObject ItemPrefab { get; } // Visual del objeto
    void OnPickup(GameObject player);
    UnityEvent<GameObject> OnItemPickedUp { get; }
}
