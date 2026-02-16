using UnityEngine;
using UnityEngine.Events;
public interface IUsable
{
    string ItemName { get; }
    bool CanBeUsedOn(GameObject target);
    void Use(GameObject target, GameObject player);
    UnityEvent<GameObject, GameObject> OnItemUsed { get; }
}