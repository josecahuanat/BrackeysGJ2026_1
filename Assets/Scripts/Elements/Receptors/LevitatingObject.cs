using UnityEngine;
using UnityEngine.Events;
public class LevitatingObject : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float alturaLevitacion  = 2f;
    [SerializeField] private float velocidadSubida   = 2f;
    [SerializeField] private float amplitudFlotacion = 0.3f;
    [SerializeField] private float velocidadFlotacion = 1.5f;
    [SerializeField] private bool  rotarMientrasFlota = true;
    [SerializeField] private Vector3 velocidadRotacion = new Vector3(0, 45f, 0);

    [Header("Eventos")]
    public UnityEvent OnLlegaAlAire;
    public UnityEvent OnVuelveAlSuelo;

    private Vector3 posOriginal;
    private Vector3 posObjetivo;
    private bool levitando, subiendo, bajando;
    private float tiempoFlotacion;

    void Start() => posOriginal = transform.position;

    void Update()
    {
        if (subiendo)
        {
            transform.position = Vector3.MoveTowards(transform.position, posObjetivo, velocidadSubida * Time.deltaTime);
            if (Vector3.Distance(transform.position, posObjetivo) < 0.05f)
            {
                subiendo = false; levitando = true; tiempoFlotacion = 0;
                OnLlegaAlAire?.Invoke();
            }
        }
        else if (bajando)
        {
            transform.position = Vector3.MoveTowards(transform.position, posOriginal, velocidadSubida * Time.deltaTime);
            if (Vector3.Distance(transform.position, posOriginal) < 0.05f)
            {
                bajando = false; transform.position = posOriginal;
                OnVuelveAlSuelo?.Invoke();
            }
        }
        else if (levitando)
        {
            tiempoFlotacion += Time.deltaTime * velocidadFlotacion;
            transform.position = posObjetivo + new Vector3(0, Mathf.Sin(tiempoFlotacion) * amplitudFlotacion, 0);
            if (rotarMientrasFlota) transform.Rotate(velocidadRotacion * Time.deltaTime);
        }
    }

    public void Levitar() { posObjetivo = posOriginal + Vector3.up * alturaLevitacion; subiendo = true; bajando = false; levitando = false; }
    public void Bajar()   { levitando = false; subiendo = false; bajando = true; }
    public void Toggle()  { if (levitando || subiendo) Bajar(); else Levitar(); }
}