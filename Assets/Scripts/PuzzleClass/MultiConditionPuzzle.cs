using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;
public class MultiConditionPuzzle : MonoBehaviour
{
    [Header("Condiciones (deben cumplirse TODAS)")]
    [SerializeField] List<CondicionPlaca>        placas   = new();
    [SerializeField] List<CondicionLever>        levers   = new();
    [SerializeField] List<CondicionBoton>        botones  = new();
    [SerializeField] List<CondicionSocket>       sockets  = new();
    [SerializeField] List<CondicionItemRecogido> items    = new();
    [Header("Recompensas al completar")]
    [SerializeField] List<PuzzleReward> recompensas = new();
    [Header("¿Requieren cumplirse en orden?")]
    [SerializeField] bool requiereOrden = false;

    [Header("UI de progreso (opcional)")]
    [SerializeField] TextMeshProUGUI textoProgreso;

    [Header("Eventos")]
    public UnityEvent OnTodasCumplidas;
    public UnityEvent OnPuzzleReseteado;
    public UnityEvent<int, int> OnProgresoActualizado; // (cumplidas, total)
    // ── Privados ───────────────────────────────────────────
    List<PuzzleCondition> todasLasCondiciones = new();
    bool puzzleCompleto = false;

    // ══════════════════════════════════════════════════════
    
    public bool Inicializado { get; private set; } = false;

// Llamado por PuzzleZoneLinker en vez de Start
    public void InicializarDesdeLinker(
    List<CondicionPlaca>        _placas,
    List<CondicionLever>        _levers,
    List<CondicionBoton>        _botones,
    List<CondicionSocket>       _sockets,
    List<CondicionItemRecogido> _items)
    {
        placas   = _placas;
        levers   = _levers;
        botones  = _botones;
        sockets  = _sockets;
        items    = _items;
        Inicializado = true;
        // Ejecuta la misma lógica que Start()
        InicializarCondiciones();
    }

    // void Start()
    // {
    //     if (!Inicializado)      // Si nadie llamó InicializarDesdeLinker, funciona normal
    //         InicializarCondiciones();
    // }

    void InicializarCondiciones()
    {
        todasLasCondiciones.Clear();
        todasLasCondiciones.AddRange(placas);
        todasLasCondiciones.AddRange(levers);
        todasLasCondiciones.AddRange(botones);
        todasLasCondiciones.AddRange(sockets);
        todasLasCondiciones.AddRange(items);

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
             SpawnRecompensas();
            OnTodasCumplidas?.Invoke();
        }
    }

    int ContarCumplidas() 
    {
        int c = 0;
        foreach (var cond in todasLasCondiciones) if (cond.cumplida) c++;
        return c;
    }
    void SpawnRecompensas()
{
    foreach (var recompensa in recompensas)
    {
        if (recompensa.prefab == null) continue;

        Vector3 origen = recompensa.spawnPoint != null
            ? recompensa.spawnPoint.position + recompensa.offset
            : transform.position + recompensa.offset;

        for (int i = 0; i < recompensa.cantidad; i++)
        {
            // Si hay más de uno, los separa en línea
            Vector3 pos = origen + Vector3.right * (i * recompensa.separacion);
            Instantiate(recompensa.prefab, pos, Quaternion.identity);
        }
    }
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
[System.Serializable]
public class PuzzleReward
{
    public GameObject prefab;
    
    [Tooltip("Dónde aparece. Si es null, aparece en la posición del puzzle")]
    public Transform spawnPoint;
    
    [Min(1)]
    public int cantidad = 1;
    
    [Tooltip("Offset respecto al spawnPoint")]
    public Vector3 offset = Vector3.zero;
    
    [Tooltip("Separación entre cada objeto si cantidad > 1")]
    public float separacion = 0.5f;
}