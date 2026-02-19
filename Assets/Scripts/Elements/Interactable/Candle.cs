using UnityEngine;
using UnityEngine.Events;

public class Candle : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private GameObject llama;          // ParticleSystem o mesh de fuego
    [SerializeField] private Light luzVela;

    [Header("Eventos")]
    [SerializeField] private UnityEvent<GameObject> onInteracted;
    public UnityEvent OnEncendida;
    public UnityEvent OnApagada;
    public bool MostrarProgresoPuzzle => true;
    public bool EstaEncendida { get; private set; } = false;

    // ── IInteractable ──────────────────────────────────────
    public string InteractPrompt => EstaEncendida ? "Apagar vela" : "Encender vela";
    public bool CanInteract => true;
    public UnityEvent<GameObject> OnInteracted => onInteracted;

    public void Interact(GameObject player)
    {
        if (EstaEncendida) Apagar();
        else               Encender();
        onInteracted?.Invoke(player);
    }
    // ───────────────────────────────────────────────────────

    public void Encender()
    {
        EstaEncendida = true;
        if (llama)    llama.SetActive(true);
        if (luzVela)  luzVela.enabled = true;
        OnEncendida?.Invoke();
    }

    public void Apagar()
    {
        EstaEncendida = false;
        if (llama)    llama.SetActive(false);
        if (luzVela)  luzVela.enabled = false;
        OnApagada?.Invoke();
    }

    // Para resetear desde el puzzle
    public void Resetear() => Apagar();
}