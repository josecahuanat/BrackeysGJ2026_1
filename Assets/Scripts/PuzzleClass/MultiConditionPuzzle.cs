using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;
public class MultiConditionPuzzle : MonoBehaviour
{
    [Header("Condiciones (deben cumplirse TODAS)")]
    [SerializeField] private List<CondicionPlaca>        placas   = new();
    [SerializeField] private List<CondicionLever>        levers   = new();
    [SerializeField] private List<CondicionBoton>        botones  = new();
    [SerializeField] private List<CondicionSocket>       sockets  = new();
    [SerializeField] private List<CondicionItemRecogido> items    = new();

    [Header("¿Requieren cumplirse en orden?")]
    [SerializeField] private bool requiereOrden = false;

    [Header("UI de progreso (opcional)")]
    [SerializeField] private TextMeshProUGUI textoProgreso;

    [Header("Eventos")]
    public UnityEvent OnTodasCumplidas;
    public UnityEvent OnPuzzleReseteado;
    public UnityEvent<int, int> OnProgresoActualizado; // (cumplidas, total)

    // ── Privados ───────────────────────────────────────────
    private List<PuzzleCondition> todasLasCondiciones = new();
    private bool puzzleCompleto = false;

    // ══════════════════════════════════════════════════════
    void Start()
    {
        // Recolectar todas las condiciones en una sola lista
        todasLasCondiciones.AddRange(placas);
        todasLasCondiciones.AddRange(levers);
        todasLasCondiciones.AddRange(botones);
        todasLasCondiciones.AddRange(sockets);
        todasLasCondiciones.AddRange(items);

        // Suscribir cada condición al callback de verificación
        foreach (var cond in todasLasCondiciones)
            cond.Inicializar(VerificarPuzzle);

        ActualizarProgreso();
    }

    void VerificarPuzzle()
    {
        if (puzzleCompleto) return;

        int cumplidas = ContarCumplidas();
        int total     = todasLasCondiciones.Count;

        ActualizarProgreso();
        OnProgresoActualizado?.Invoke(cumplidas, total);

        if (requiereOrden)
        {
            // Verificar que se cumplieron en orden secuencial
            // (la condición N no puede cumplirse antes que N-1)
            for (int i = 0; i < todasLasCondiciones.Count; i++)
            {
                if (!todasLasCondiciones[i].cumplida)
                {
                    // Si hay una sin cumplir antes de las cumplidas → orden incorrecto
                    bool hayPosterioresCumplidas = false;
                    for (int j = i + 1; j < todasLasCondiciones.Count; j++)
                        if (todasLasCondiciones[j].cumplida) { hayPosterioresCumplidas = true; break; }

                    if (hayPosterioresCumplidas)
                    {
                        Debug.Log("Orden incorrecto, reseteando");
                        Resetear();
                        return;
                    }
                    break;
                }
            }
        }

        if (cumplidas >= total)
        {
            puzzleCompleto = true;
            Debug.Log($"[MultiConditionPuzzle] ¡Puzzle '{name}' completado!");
            OnTodasCumplidas?.Invoke();
        }
    }

    int ContarCumplidas() 
    {
        int c = 0;
        foreach (var cond in todasLasCondiciones) if (cond.cumplida) c++;
        return c;
    }

    void ActualizarProgreso()
    {
        if (textoProgreso == null) return;
        int cumplidas = ContarCumplidas();
        int total     = todasLasCondiciones.Count;
        textoProgreso.text = $"{cumplidas} / {total}";
    }

    public void Resetear()
    {
        foreach (var cond in todasLasCondiciones) cond.Resetear();
        puzzleCompleto = false;
        ActualizarProgreso();
        OnPuzzleReseteado?.Invoke();
    }
}
