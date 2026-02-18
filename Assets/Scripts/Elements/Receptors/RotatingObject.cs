using UnityEngine;
using UnityEngine.Events;
public class RotatingObject : MonoBehaviour
{
    public enum Modo { Continuo, HaciaAngulo, IrYVolver, UnaVez }

    [Header("Configuración")]
    [SerializeField] private Modo   modo           = Modo.HaciaAngulo;
    [SerializeField] private Vector3 eje           = Vector3.up;
    [SerializeField] private float  velocidad      = 90f;
    [SerializeField] private float  anguloObjetivo = 90f;
    [SerializeField] private bool   activoAlInicio = false;

    [Header("Eventos")]
    public UnityEvent OnRotacionCompleta;

    private bool  activo         = false;
    private float anguloRecorrido = 0f;
    private int   direccion       = 1;

    void Start() { if (activoAlInicio) Activar(); }

    void Update()
    {
        if (!activo) return;
        float delta = velocidad * Time.deltaTime;

        switch (modo)
        {
            case Modo.Continuo:
                transform.Rotate(eje * delta);
                break;

            case Modo.HaciaAngulo:
            case Modo.UnaVez:
                transform.Rotate(eje * delta);
                anguloRecorrido += delta;
                if (anguloRecorrido >= anguloObjetivo)
                {
                    activo = false;
                    OnRotacionCompleta?.Invoke();
                }
                break;

            case Modo.IrYVolver:
                transform.Rotate(eje * delta * direccion);
                anguloRecorrido += delta;
                if (anguloRecorrido >= anguloObjetivo) { direccion *= -1; anguloRecorrido = 0; }
                break;
        }
    }

    public void Activar()   { activo = true;  anguloRecorrido = 0; }
    public void Desactivar(){ activo = false; }
    public void Toggle()    { if (activo) Desactivar(); else Activar(); }
}
