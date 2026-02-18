using UnityEngine;
public class ProximityDetector : MonoBehaviour
{
    // Este componente va en un GameObject HIJO del Player
    // con un SphereCollider (IsTrigger = true)
    private PlayerInteraction playerInteraction;

    void Start()
    {
        // Busca PlayerInteraction en el padre
        playerInteraction = GetComponentInParent<PlayerInteraction>();
        if (playerInteraction == null)
            Debug.LogError("ProximityDetector necesita un PlayerInteraction en el padre!");
    }

    void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
            playerInteraction?.OnNearbyEnter(interactable);
        if (other.CompareTag("Flower"))
        {
            Destroy(other.gameObject);
            return;
        }
    }

    void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
            playerInteraction?.OnNearbyExit(interactable);
    }
}