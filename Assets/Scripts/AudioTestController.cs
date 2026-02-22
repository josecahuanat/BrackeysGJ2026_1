using UnityEngine;

public class AudioTestController : MonoBehaviour
{
    private AdaptiveAudioManager audioManager;
    
    void Start()
    {
        audioManager = GetComponent<AdaptiveAudioManager>();
    }
    
    void Update()
    {
        // Presiona teclas 0-4 para cambiar de capa
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            audioManager.SwitchToLayer(0);
            Debug.Log("Cambiado a Layer 0: Base Exploration");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            audioManager.SwitchToLayer(1);
            Debug.Log("Cambiado a Layer 1: Easy Level");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            audioManager.SwitchToLayer(2);
            Debug.Log("Cambiado a Layer 2: Normal Level");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            audioManager.SwitchToLayer(3);
            Debug.Log("Cambiado a Layer 3: Hard Level");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            audioManager.SwitchToLayer(4);
            Debug.Log("Cambiado a Layer 4: Very Hard Level");
        }
    }
}
