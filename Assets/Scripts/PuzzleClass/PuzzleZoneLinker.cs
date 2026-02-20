using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;
using JetBrains.Annotations;

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
    [SerializeField] private List<Key>           keys           = new();
    [SerializeField] private List<PressurePlate> pressurePlates = new();
    [SerializeField] private List<Lever>         levers         = new();
    [SerializeField] private List<Candle> candles = new();
    [SerializeField] private List<Door> doors = new();
    [SerializeField] private List<FlowerAltar> altarFlowers = new();
    [SerializeField] private List<SocketKeyBinding> socketBindings = new();
    
    [Header("Opciones")]
    // [SerializeField] private bool autoDiscoverChildren = true;
    [SerializeField] bool requiereOrden = false;
    [SerializeField] bool doSelfInitialize = false;
    PuzzleDifficulty puzzleData;


    void Awake()
    {
        if (doSelfInitialize)
        {
            DiscoverChildren();
            WireConditions();
        }
    }

    void DiscoverChildren()
    {        
        // Auto-descubre si las listas están vacías
        if (keys.Count == 0)
            keys.AddRange(GetComponentsInChildren<Key>());

        if (pressurePlates.Count == 0)
            pressurePlates.AddRange(GetComponentsInChildren<PressurePlate>());

        if (levers.Count == 0)
            levers.AddRange(GetComponentsInChildren<Lever>());
        if (doors.Count == 0)
            doors.AddRange(GetComponentsInChildren<Door>());
        if (candles.Count == 0)
            candles.AddRange(GetComponentsInChildren<Candle>());
         
        if (altarFlowers.Count == 0)
            altarFlowers.AddRange(GetComponentsInChildren<FlowerAltar>());
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
    

    public void WireConditions()
    {
        var _placas   = new List<CondicionPlaca>();
        var _levers   = new List<CondicionLever>();
        var _candles   = new List<CondicionVela>();
        var _doors   = new List<CondicionPuerta>();
        var _altarFlowers  = new List<CondicionFlorEnAltar>();
        var _items    = new List<CondicionItemRecogido>();
        var _sockets  = new List<CondicionSocket>();
        // Condiciones: Llaves recogidas
        foreach (var key in keys)
        {
            if (key == null) continue;
            _items.Add(new CondicionItemRecogido
            {
                nombre = $"Collect: {key.ItemName}",
                item   = key
            });
        }

        // Condiciones: Placas de presión
        foreach (var plate in pressurePlates)
        {
            if (plate == null) continue;
            _placas.Add(new CondicionPlaca
            {
                nombre = $"Plates: {plate.name}",
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

        foreach (var candle in candles)
        {
            if (candle == null) continue;
            _candles.Add(new CondicionVela
            {
                nombre = $"Candle: {candle.name}",
                vela = candle
            });
        }

        foreach (var door in doors)
        {
            if (door == null) continue;
            _doors.Add(new CondicionPuerta
            {
                nombre = $"Door: {door.name}",
                door  = door
            });
        }
        foreach (var altarFlower in altarFlowers)
        {
            if (altarFlowers == null) continue;
            _altarFlowers.Add(new CondicionFlorEnAltar
            {
                nombre = $"AltarFlower: {altarFlower.name}",
                altar = altarFlower
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

        puzzle.InicializarDesdeLinker(_placas, _levers, _doors, _candles, _altarFlowers, _items, _sockets);
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

    public void Spawn(List<Transform> itemPositions)
    {
        keys = new List<Key>();
        pressurePlates = new List<PressurePlate>();
        levers = new List<Lever>();
        socketBindings = new List<SocketKeyBinding>();

        puzzleData = Level.Instance.GetPuzzle();

        foreach(var item in puzzleData.items)
        {
            for (int i = 0 ; i < item.quantity ; i++)
            {
                int randomItemPositionIndex = Random.Range(0, itemPositions.Count);
                Transform randomItemPosition = itemPositions[randomItemPositionIndex];
                itemPositions.RemoveAt(randomItemPositionIndex);

                GameObject puzzleItem = PoolManager.Instance.Spawn(item.GetPoolName(), randomItemPosition, Vector3.zero, Quaternion.identity);
                switch(item.name)
                {
                    case PuzzleItemName.Lever: levers.Add(puzzleItem.GetComponent<Lever>()); break;
                }
            }
        }
        
        WireConditions();
    }

    public void Despawn()
    {
        foreach(var lever in levers)
        {
            PoolManager.Instance.Despawn(PoolName.Lever, lever.gameObject);
        }
    }
}