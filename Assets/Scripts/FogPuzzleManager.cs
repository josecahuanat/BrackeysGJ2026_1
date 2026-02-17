using UnityEngine;

public class FogPuzzleManager : MonoBehaviour
{
    void Start()
    {
        Tomb[] tombs = FindObjectsOfType<Tomb>();
        
        foreach (Tomb tomb in tombs)
        {
            tomb.OnTombInteracted += HandleTombInteraction;
        }
    }
    
    private void HandleTombInteraction()
    {
        Debug.Log("has encendido la vela");
    }
}
