using UnityEngine;
using System;

public class Tomb : MonoBehaviour, IInteractable
{
    public event Action OnTombInteracted;
    
    public string InteractPrompt => "Esta es la tumba";
    
    public bool CanInteract => true;
    
    public void Interact(GameObject player)
    {
        OnTombInteracted?.Invoke();
    }
}
