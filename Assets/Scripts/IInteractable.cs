using UnityEngine;
using UnityEngine.Events;
// IInteractable.cs - Para objetos que se interactúan con E
public interface IInteractable
{
    string InteractPrompt { get; } // "Presiona E para abrir"
    bool CanInteract { get; } // Si puede interactuarse ahora
    void Interact(GameObject player);
    UnityEvent<GameObject> OnInteracted { get; }
}

