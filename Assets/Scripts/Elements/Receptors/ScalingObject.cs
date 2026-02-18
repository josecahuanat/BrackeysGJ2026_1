using UnityEngine;
using UnityEngine.Events;
using System.Collections;
public class ScalingObject : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Vector3        escalaObjetivo = new Vector3(3, 3, 3);
    [SerializeField] private float          duracion       = 1.5f;
    [SerializeField] private AnimationCurve curva          = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Eventos")]
    public UnityEvent OnEscalaCompletada;

    private Vector3   escalaOriginal;
    private Coroutine corutina;

    void Start() => escalaOriginal = transform.localScale;

    public void EscalarGrande()   => Animar(escalaObjetivo);
    public void EscalarOriginal() => Animar(escalaOriginal);
    public void Toggle()          { if (transform.localScale == escalaOriginal) EscalarGrande(); else EscalarOriginal(); }

    void Animar(Vector3 objetivo)
    {
        if (corutina != null) StopCoroutine(corutina);
        corutina = StartCoroutine(Lerp(objetivo));
    }

    IEnumerator Lerp(Vector3 objetivo)
    {
        Vector3 inicio = transform.localScale;
        float t = 0;
        while (t < duracion)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(inicio, objetivo, curva.Evaluate(t / duracion));
            yield return null;
        }
        transform.localScale = objetivo;
        OnEscalaCompletada?.Invoke();
    }
}