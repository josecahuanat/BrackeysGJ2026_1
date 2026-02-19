using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

// Configuración de qué IDs de llave corresponden a qué socket/puerta,
// definida directamente en el prefab desde el Inspector.
[System.Serializable]
public class SocketKeyBinding
{
    public ItemSocket socket;
    public string     requiredKeyID; // debe coincidir con Key.keyID de la llave spawneada
    public string     conditionName = "Llave en socket";
}

public class PuzzleZoneLinker : MonoBehaviour
{
    [SerializeField] MultiConditionPuzzle puzzle;
    
    [Header("Condiciones a cablear automáticamente")]
    [Tooltip("Deja vacío para auto-descubrir todos los hijos")]
    [SerializeField] private List<Key> keys;
    [SerializeField] private List<PressurePlate> pressurePlates;
    [SerializeField] private List<Lever> levers;
    [SerializeField] private List<SocketKeyBinding> socketBindings;

    [Header("Opciones")]
    [SerializeField] private bool autoDiscoverChildren = true;
    [SerializeField] private bool requiereOrden = false;


    void Awake()
    {
        if (autoDiscoverChildren)
            DiscoverChildren();

        WireConditions();
    }

    void DiscoverChildren()
    {        
        // Auto-descubre si las listas están vacías
        if (keys.Count == 0)
            keys.AddRange(GetComponentsInChildren<Key>());

        if (pressurePlates.Count == 0)
            pressurePlates.AddRange(GetComponentsInChildren<PressurePlate>());

        if (levers.Count == 0)
        {
            levers.AddRange(GetComponentsInChildren<Lever>());
        }

        // Sockets: los descubre y crea bindings vacíos si no están configurados
        if (socketBindings.Count == 0)
        {
            foreach (var socket in GetComponentsInChildren<ItemSocket>())
            {
                socketBindings.Add(new SocketKeyBinding
                {
                    socket = socket,
                    conditionName = $"Socket: {socket.name}"
                });
            }
        }
    }

    void WireConditions()
    {
        var _placas   = new List<CondicionPlaca>();
        var _levers   = new List<CondicionLever>();
        var _botones  = new List<CondicionBoton>();
        var _sockets  = new List<CondicionSocket>();
        var _items    = new List<CondicionItemRecogido>();

        // Condiciones: Llaves recogidas
        foreach (var key in keys)
        {
            if (key == null) continue;
            _items.Add(new CondicionItemRecogido
            {
                nombre = $"Recoger: {key.ItemName}",
                item   = key
            });
        }

        // Condiciones: Placas de presión
        foreach (var plate in pressurePlates)
        {
            if (plate == null) continue;
            _placas.Add(new CondicionPlaca
            {
                nombre = $"Placa: {plate.name}",
                placa  = plate
            });
        }

        // Condiciones: Levers
        foreach (var lever in levers)
        {
            if (lever == null) continue;
            _levers.Add(new CondicionLever
            {
                nombre = $"Lever: {lever.name}",
                lever  = lever
            });
        }

        // Condiciones: Sockets
        foreach (var binding in socketBindings)
        {
            if (binding.socket == null) continue;
            _sockets.Add(new CondicionSocket
            {
                nombre = binding.conditionName,
                socket = binding.socket
            });
        }

        puzzle.InicializarDesdeLinker(_placas, _levers, _botones, _sockets, _items);
    }

    // Llamado desde Ground.cs para reubicar elementos del puzzle
    // en las estructuras del chunk (tumba, mausoleo, etc.)
    public void RelocateElements(List<Transform> tombSlots, List<Transform> mausoleumSlots)
    {
        // Mueve las llaves a tumbas aleatorias
        List<Transform> availableTombs = new List<Transform>(tombSlots);
        foreach (var key in keys)
        {
            if (availableTombs.Count == 0) break;
            int idx = Random.Range(0, availableTombs.Count);
            key.transform.SetParent(availableTombs[idx]);
            key.transform.localPosition = Vector3.zero;
            availableTombs.RemoveAt(idx);
        }

        // Mueve las placas/levers a mausoleums
        List<Transform> availableMauseolums = new List<Transform>(mausoleumSlots);
        foreach (var plate in pressurePlates)
        {
            if (availableMauseolums.Count == 0) break;
            int idx = Random.Range(0, availableMauseolums.Count);
            plate.transform.SetParent(availableMauseolums[idx]);
            plate.transform.localPosition = Vector3.zero;
            availableMauseolums.RemoveAt(idx);
        }
    }

    public void Spawn()
    {
        keys = new List<Key>();
        pressurePlates = new List<PressurePlate>();
        levers = new List<Lever>();
        socketBindings = new List<SocketKeyBinding>();

        PuzzleDifficulty puzzle = Level.Instance.GetPuzzle();
        foreach(var item in puzzle.items)
        {
            for (int i = 0 ; i < item.quantity ; i++)
            {
                GameObject puzzleItem = PoolManager.Instance.Spawn(item.GetPoolName(), transform, Vector3.zero, Quaternion.identity);
                switch(item.name)
                {
                    case PuzzleItemName.Lever: levers.Add(puzzleItem.GetComponent<Lever>()); break;
                }
            }
        }
    }

    public void Despawn()
    {
        foreach(var lever in levers)
        {
            PoolManager.Instance.Despawn(PoolName.Lever, lever.gameObject);
        }
    }
}