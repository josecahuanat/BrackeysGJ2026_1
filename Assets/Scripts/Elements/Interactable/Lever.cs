using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public class Lever : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private string promptOn  = "Presiona E para activar palanca";
    [SerializeField] private string promptOff = "Presiona E para desactivar palanca";

    [Header("Animación visual")]
    [SerializeField] private Transform leverHandle;
    [SerializeField] private float anguloOn  = -60f;
    [SerializeField] private float anguloOff =  60f;
    [SerializeField] private float velocidadAnim = 5f;

    [Header("Eventos - IInteractable")]
    [SerializeField] private UnityEvent<GameObject> onInteracted;

    [Header("Eventos propios")]
    public UnityEvent OnActivado;
    public UnityEvent OnDesactivado;

    // ── IInteractable ──────────────────────────────────────
    public string InteractPrompt               => isOn ? promptOff : promptOn;
    public bool   CanInteract                  => true;
    public UnityEvent<GameObject> OnInteracted => onInteracted;

    public void Interact(GameObject player)
    {
        isOn = !isOn;
        targetRot = Quaternion.Euler(isOn ? anguloOn : anguloOff, 0, 0);

        if (isOn) OnActivado?.Invoke();
        else      OnDesactivado?.Invoke();

        onInteracted?.Invoke(player);
    }
    // ───────────────────────────────────────────────────────

    private bool isOn = false;
    private Quaternion targetRot;

    void Start()  => targetRot = Quaternion.Euler(anguloOff, 0, 0);
    void Update()
    {
        if (leverHandle)
            leverHandle.localRotation = Quaternion.Lerp(
                leverHandle.localRotation, targetRot, velocidadAnim * Time.deltaTime);
    }
}