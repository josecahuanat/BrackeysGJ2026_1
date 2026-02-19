using UnityEngine;
[System.Serializable]
public abstract class PuzzleCondition
{
    [Tooltip("Nombre para identificar esta condición en el Inspector")]
    public string nombre = "Condición";

    [HideInInspector]
    public bool cumplida = false;

    // Cada condición se suscribe a lo que necesita escuchar
    public abstract void Inicializar(System.Action onCumplida);
    public abstract void Resetear();
}
// ── Condición: Placa de presión pisada ────────────────────
[System.Serializable]
public class CondicionPlaca : PuzzleCondition
{
    public PressurePlate placa;

    public override void Inicializar(System.Action onCumplida)
    {
        if (placa == null) { Debug.LogWarning($"[{nombre}] Placa no asignada"); return; }
        placa.OnPresionada.AddListener(() =>
        {
            if (!cumplida) { cumplida = true; onCumplida?.Invoke(); }
        });
    }

    public override void Resetear()
    {
        cumplida = false;
        // No desuscribimos para que pueda volver a cumplirse si el puzzle se resetea
    }
}


// ── Condición: Lever activado ─────────────────────────────
[System.Serializable]
public class CondicionLever : PuzzleCondition
{
    public Lever lever;

    public override void Inicializar(System.Action onCumplida)
    {
        if (lever == null) { Debug.LogWarning($"[{nombre}] Lever no asignado"); return; }
        lever.OnActivado.AddListener(() =>
        {
            if (!cumplida) { cumplida = true; onCumplida?.Invoke(); }
        });
        lever.OnDesactivado.AddListener(() => { cumplida = false; });
    }

    public override void Resetear() => cumplida = false;
}


// ── Condición: Botón pulsado ──────────────────────────────
[System.Serializable]
public class CondicionPuerta : PuzzleCondition
{
    public Door door;

    public override void Inicializar(System.Action onCumplida)
    {
       if (door == null) { Debug.LogWarning($"[{nombre}] Botón no asignado"); return; }
        door.onInteracted.AddListener((_) =>
        {
          if (!cumplida) { cumplida = true; onCumplida?.Invoke(); }
        });
    }

    public override void Resetear() => cumplida = false;
}


// ── Condición: Item colocado en socket ────────────────────
[System.Serializable]
public class CondicionSocket : PuzzleCondition
{
    public ItemSocket socket;

    public override void Inicializar(System.Action onCumplida)
    {
        if (socket == null) { Debug.LogWarning($"[{nombre}] Socket no asignado"); return; }
        socket.OnItemColocado.AddListener(() =>
        {
            if (!cumplida) { cumplida = true; onCumplida?.Invoke(); }
        });
        socket.OnItemRetirado.AddListener(() => { cumplida = false; });
    }

    public override void Resetear() => cumplida = false;
}


// ── Condición: Item recogido del suelo ────────────────────
[System.Serializable]
public class CondicionItemRecogido : PuzzleCondition
{
    public Key item; // o cualquier IPickable que uses

    public override void Inicializar(System.Action onCumplida)
    {
        if (item == null) { Debug.LogWarning($"[{nombre}] Item no asignado"); return; }
        item.OnItemPickedUp.AddListener((_) =>
        {
            if (!cumplida) { cumplida = true; onCumplida?.Invoke(); }
        });
    }

    public override void Resetear() => cumplida = false;
}
[System.Serializable]
public class CondicionVela : PuzzleCondition
{
    public Candle vela;

    public override void Inicializar(System.Action onCumplida)
    {
        if (vela == null) { Debug.LogWarning($"[{nombre}] Vela no asignada"); return; }
        vela.OnEncendida.AddListener(() =>
        {
            if (!cumplida) { cumplida = true; onCumplida?.Invoke(); }
        });
        vela.OnApagada.AddListener(() => { cumplida = false; });
    }

    public override void Resetear()
    {
        cumplida = false;
        vela?.Resetear();
    }
}

// ── Condición: Flor colocada en altar ─────────────────────
[System.Serializable]
public class CondicionFlorEnAltar : PuzzleCondition
{
    public FlowerAltar altar;

    public override void Inicializar(System.Action onCumplida)
    {
        if (altar == null) { Debug.LogWarning($"[{nombre}] Altar no asignado"); return; }
        altar.OnFlorColocada.AddListener(() =>
        {
            if (!cumplida) { cumplida = true; onCumplida?.Invoke(); }
        });
        altar.OnFlorRetirada.AddListener(() => { cumplida = false; });
    }

    public override void Resetear()
    {
        cumplida = false;
        altar?.Resetear();
    }
}

// ── Condición: Pieza de tumba completada ──────────────────
[System.Serializable]
public class CondicionPiezaTumba : PuzzleCondition
{
    public TombPiece pieza;

    public override void Inicializar(System.Action onCumplida)
    {
        if (pieza == null) { Debug.LogWarning($"[{nombre}] Pieza no asignada"); return; }
        pieza.OnPiezaCompletada.AddListener(() =>
        {
            if (!cumplida) { cumplida = true; onCumplida?.Invoke(); }
        });
    }

    public override void Resetear()
    {
        cumplida = false;
        pieza?.Resetear();
    }
}
[System.Serializable]
public class CondicionMultiSocket : PuzzleCondition
{
    public MultiItemSocket socket;

    public override void Inicializar(System.Action onCumplida)
    {
        if (socket == null) { Debug.LogWarning($"[{nombre}] MultiItemSocket no asignado"); return; }

        socket.OnTodosLlenos.AddListener(() =>
        {
            if (!cumplida) { cumplida = true; onCumplida?.Invoke(); }
        });

        socket.OnSlotVaciado.AddListener(() => { cumplida = false; });
    }

    public override void Resetear()
    {
        cumplida = false;
        socket?.Resetear();
    }
}