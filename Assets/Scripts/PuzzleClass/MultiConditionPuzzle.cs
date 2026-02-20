using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;
public class MultiConditionPuzzle : MonoBehaviour
{
    [Header("Condiciones (deben cumplirse TODAS)")]
    [SerializeField] private List<CondicionPlaca>        placas   = new();
    [SerializeField] private List<CondicionLever>        levers   = new();
    [SerializeField] private List<CondicionPuerta>        puertas  = new();
    [SerializeField] private List<CondicionItemRecogido> items    = new();
    [SerializeField] private List<CondicionVela>         velas        = new();
    [SerializeField] private List<CondicionFlorEnAltar>  flores       = new();
    [SerializeField] private List<CondicionPiezaTumba>   piezasTumba  = new();
    [SerializeField] private List<CondicionSocket>       sockets  = new();
    //[SerializeField] private List<CondicionMultiSocket>   multySockets  = new();
    
    // Añade en el header UI:
    [SerializeField] private TextMeshProUGUI textoPista;

// Añade las nuevas listas:

    [Header("Recompensas al completar")]
    [SerializeField] private List<PuzzleReward> recompensas = new();
    [Header("¿Requieren cumplirse en orden?")]
    [SerializeField] private bool requiereOrden = false;

    [Header("UI de progreso (opcional)")]
    [SerializeField] private TextMeshProUGUI textoProgreso;

    [Header("Eventos")]
    public UnityEvent OnTodasCumplidas;
    public UnityEvent OnPuzzleReseteado;
    public UnityEvent<int, int> OnProgresoActualizado; // (cumplidas, total)
    
    private List<PuzzleCondition> todasLasCondiciones = new();
    private bool puzzleCompleto = false;

    // ══════════════════════════════════════════════════════
    
    public bool Inicializado { get; private set; } = false;
    public int TotalCondiciones() => todasLasCondiciones.Count;
// Llamado por PuzzleZoneLinker en vez de Start
    public void InicializarDesdeLinker(
    List<CondicionPlaca>        _placas,
    List<CondicionLever>        _levers,
    List<CondicionPuerta>        _puertas,
    List<CondicionVela>        _velas,
    List<CondicionFlorEnAltar>        _altarFlowers,
    List<CondicionItemRecogido> _items,
    List<CondicionSocket>       _sockets)
    {
    placas   = _placas;
    levers   = _levers;
    puertas  = _puertas;
    velas = _velas;
    flores = _altarFlowers;
    sockets  = _sockets;
    items    = _items;
    Inicializado = true;
    // Ejecuta la misma lógica que Start()
    InicializarCondiciones();
    }

    void Start()
    {
        if (!Inicializado)      // Si nadie llamó InicializarDesdeLinker, funciona normal
            InicializarCondiciones();
    }

    void InicializarCondiciones()
    {
        // Debug.Log($"[Puzzle] InicializarCondiciones llamado. Stack: {System.Environment.StackTrace}");
        todasLasCondiciones.Clear();
        todasLasCondiciones.Clear();
        todasLasCondiciones.AddRange(placas);
        todasLasCondiciones.AddRange(levers);
        todasLasCondiciones.AddRange(puertas);
        todasLasCondiciones.AddRange(velas);
        todasLasCondiciones.AddRange(flores);
        todasLasCondiciones.AddRange(piezasTumba);
        int max = Mathf.Max(items.Count, sockets.Count);
        for (int i = 0; i < max; i++)
        {
            if (i < items.Count)   todasLasCondiciones.Add(items[i]);
            if (i < sockets.Count) todasLasCondiciones.Add(sockets[i]);
        }


            foreach (var cond in todasLasCondiciones)
                cond.Inicializar(VerificarPuzzle);

        ActualizarProgreso();
            for (int i = 0; i < todasLasCondiciones.Count; i++)
            // Debug.Log($"[{i}] {todasLasCondiciones[i].nombre}");
                if (todasLasCondiciones.Count > 0) 
        {
            // Debug.LogWarning("Ya inicializado, ignorando segunda llamada");
            return;
        }
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

    public int ContarCumplidas() 
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
        int cumplidas = ContarCumplidas();
        int total     = todasLasCondiciones.Count;

        if (textoProgreso != null)
            textoProgreso.text = $"{cumplidas} / {total}";
        ActualizarPista();
         GameUIManager.Instance?.UpdatePuzzleProgress(
        $"{cumplidas} / {total}",     
         ObtenerPistaActual()
          );
        
    }

    public string ObtenerPistaActual()
    {
        if (!requiereOrden) return null;
        foreach (var cond in todasLasCondiciones)
            if (!cond.cumplida) return $"Siguiente: {cond.nombre}";
        return "¡Listo!";
    }

    void ActualizarPista()
    {
        if (!requiereOrden) return;

        foreach (var cond in todasLasCondiciones)
        {
            if (!cond.cumplida)
            {
                string hint = $"Siguiente: {cond.nombre}";
                if (textoPista != null) textoPista.text = hint;
                return;
            }
        }
        if (textoPista != null) textoPista.text = "¡Listo!";
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