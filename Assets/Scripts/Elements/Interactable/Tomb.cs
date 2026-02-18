using UnityEngine;
using System;
using UnityEngine.Events;

public class Tomb : MonoBehaviour, IInteractable
{
    [Header("Candle State")]
    private bool isLit = false;
    
    [Header("Visual Settings")]
    [SerializeField] private Color litColor = new Color(1f, 1f, 0f); 
    [SerializeField] private Color unlitColor = new Color(0.01f, 0.01f, 0.01f);
    [SerializeField] private float emissionIntensity = 2f;
    
    private MeshRenderer meshRenderer;
    
    public event Action OnTombInteracted;
    
    public string InteractPrompt => isLit ? "Vela encendida" : "Presiona E para encender vela";
    
    public bool CanInteract => !isLit;
    
    public bool IsLit => isLit;

    public UnityEvent<GameObject> OnInteracted => throw new NotImplementedException();

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError($"Tomb {gameObject.name}: No se encontró MeshRenderer!");
        }
        Initialize();
    }
    
    public void Initialize()
    {
        isLit = false;
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        
        litColor = new Color(1f, 1f, 0f); 
        unlitColor = new Color(0.01f, 0.01f, 0.01f); 
        emissionIntensity = 2f;
        
        UpdateVisual();
    }
    
    public void Interact(GameObject player)
    {
        if (!isLit)
        {
            isLit = true;
            UpdateVisual();
            OnTombInteracted?.Invoke();
        }
    }
    
    private void UpdateVisual()
    {
        if (meshRenderer == null)
        {
            Debug.LogWarning($"Tomb {gameObject.name}: meshRenderer es null en UpdateVisual");
            return;
        }
        
        Material mat = new Material(meshRenderer.material);
        mat.color = isLit ? litColor : unlitColor;
        
        if (isLit)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", litColor * emissionIntensity);
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.black);
        }
        
        meshRenderer.material = mat;
    }
}
