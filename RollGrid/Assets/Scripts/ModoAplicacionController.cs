using UnityEngine;
using System;

public enum ModoAplicacion
{
    Edicion,
    Juego
}

public class ModoAplicacionController : MonoBehaviour
{
    public static ModoAplicacionController Instancia { get; private set; }

    public ModoAplicacion ModoActual { get; private set; } = ModoAplicacion.Edicion;

    public event Action<ModoAplicacion> OnModoCambiado;

    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
    }

    public void CambiarModo(ModoAplicacion nuevoModo)
    {
        ModoActual = nuevoModo;
        OnModoCambiado?.Invoke(nuevoModo);
        Debug.Log("Modo cambiado a: " + nuevoModo);
    }
}
