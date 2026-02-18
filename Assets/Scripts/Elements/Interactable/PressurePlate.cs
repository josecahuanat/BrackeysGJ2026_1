using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public class PressurePlate : MonoBehaviour, ITriggerable
{
    [Header("Configuración")]
    [SerializeField] private bool jugadorActiva = true;
    [SerializeField] private bool objetoActiva  = false;
    [SerializeField] private string tagObjeto   = "PuzzleBox";
    [SerializeField] private bool quedaPresionada = false;

    [Header("Visual")]
    [SerializeField] private Transform plateVisual;
    [SerializeField] private float     hundimiento  = 0.05f;
    [SerializeField] private Color     colorActivo   = Color.green;
    [SerializeField] private Color     colorInactivo = Color.gray;

    [Header("Eventos - ITriggerable")]
    [SerializeField] private UnityEvent<GameObject, GameObject> onItemUsed;

    [Header("Eventos propios")]
    public UnityEvent OnPresionada;
    public UnityEvent OnLiberada;

    // ── ITriggerable ───────────────────────────────────────
    // (La placa no necesita item, así que ItemName es vacío)
    public string ItemName                               => "";
    public UnityEvent<GameObject, GameObject> OnItemUsed => onItemUsed;

    public bool CanBeUsedOn(GameObject target) => !isPressed;

    public void Use(GameObject target, GameObject activator)
    {
        Presionar(activator);
    }
    // ───────────────────────────────────────────────────────

    private bool isPressed     = false;
    private int  objetosEncima = 0;
    private Vector3 posOriginal;
    private Renderer rend;

    public bool IsPressed => isPressed;

    void Start()
    {
        if (plateVisual) posOriginal = plateVisual.localPosition;
        rend = GetComponentInChildren<Renderer>();
        if (rend) rend.material.color = colorInactivo;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!EsValido(other)) return;
        objetosEncima++;
        if (!isPressed) Presionar(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (!EsValido(other) || quedaPresionada) return;
        objetosEncima = Mathf.Max(0, objetosEncima - 1);
        if (objetosEncima == 0) Liberar();
    }

    bool EsValido(Collider other)
    {
        if (jugadorActiva && other.CompareTag("Player"))    return true;
        if (objetoActiva  && other.CompareTag(tagObjeto))   return true;
        return false;
    }

    void Presionar(GameObject activador)
    {
        isPressed = true;
        if (plateVisual) plateVisual.localPosition = posOriginal - new Vector3(0, hundimiento, 0);
        if (rend)        rend.material.color = colorActivo;
        onItemUsed?.Invoke(gameObject, activador);
        OnPresionada?.Invoke();
    }

    void Liberar()
    {
        isPressed = false;
        if (plateVisual) plateVisual.localPosition = posOriginal;
        if (rend)        rend.material.color = colorInactivo;
        OnLiberada?.Invoke();
    }
}