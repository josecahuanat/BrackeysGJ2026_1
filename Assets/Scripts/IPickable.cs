using UnityEngine;

public interface  IPickable
{
    string ItemName { get; }
    void OnPickup(GameObject player);
}
