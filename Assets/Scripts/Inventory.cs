using UnityEngine;
using System.Collections.Generic;
public class Inventory : MonoBehaviour
{
    private List<string> items = new List<string>();

    public void AddItem(string itemID)
    {
        items.Add(itemID);
        Debug.Log($"Agregado: {itemID}");
    }

    public bool HasItem(string itemID)
    {
        return items.Contains(itemID);
    }

    public void RemoveItem(string itemID)
    {
        items.Remove(itemID);
    }
}
