using UnityEngine;
using UnityEngine.Events;
public class MovingObject : MonoBehaviour
{
    [Header("Puntos de movimiento")]
    [SerializeField] private Transform puntoA;
    [SerializeField] private Transform puntoB;
    [SerializeField] private float     velocidad  = 2f;
    [SerializeField] private bool      patrullar  = false;

    [Header("Eventos")]
    public UnityEvent OnLlegaA;
    public UnityEvent OnLlegaB;

    private bool moviéndose    = false;
    private bool haciaPuntoB   = true;

    void Start() { if (puntoA) transform.position = puntoA.position; }

    void Update()
    {
        if (!moviéndose || !puntoA || !puntoB) return;

        Transform dest = haciaPuntoB ? puntoB : puntoA;
        transform.position = Vector3.MoveTowards(transform.position, dest.position, velocidad * Time.deltaTime);

        if (Vector3.Distance(transform.position, dest.position) < 0.05f)
        {
            transform.position = dest.position;
            if (haciaPuntoB) { OnLlegaB?.Invoke(); if (!patrullar) moviéndose = false; else haciaPuntoB = false; }
            else             { OnLlegaA?.Invoke(); if (!patrullar) moviéndose = false; else haciaPuntoB = true;  }
        }
    }

    public void MoverAB()  { haciaPuntoB = true;  moviéndose = true; }
    public void MoverBA()  { haciaPuntoB = false; moviéndose = true; }
    public void Toggle()   { haciaPuntoB = !haciaPuntoB; moviéndose = true; }
    public void Detener()  { moviéndose = false; }
}
