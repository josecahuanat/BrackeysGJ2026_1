using UnityEngine;
using System.Collections.Generic;
public class ProximityInteractable : MonoBehaviour
{
    
    private List<IInteractable> nearbyInteractables = new List<IInteractable>();
    void OnTriggerEnter(Collider other)
    {
        
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            nearbyInteractables.Add(interactable);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            nearbyInteractables.Remove(interactable);
        }
    }
}
