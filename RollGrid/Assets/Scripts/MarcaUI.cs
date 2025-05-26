using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum TipoMarca
{
    Enemigo,
    Tesoro,
    Puerta
}

public enum EstadoMarca
{
    Activo = 0,
    Inactivo = 1,
    Oculto = 2
}

public class MarcaUI : MonoBehaviour, IPointerClickHandler
{
    public string nombre;
    public Sprite icono;
    public TipoMarca tipo;
    public EstadoMarca estado;
    [TextArea(2, 5)] public string notas;
    public MarcaFilaUI filaAsociada;

    public MarcasManager managerMarcas;

    public void Inicializar(string nombre, TipoMarca tipo, Sprite sprite, MarcasManager manager)
    {
        this.nombre = nombre;
        this.tipo = tipo;
        this.estado = EstadoMarca.Activo;
        this.notas = "";
        icono = sprite;
        managerMarcas = manager;
    }

    public void ActualizarIcono()
    {
        this.icono = managerMarcas.GetIconoPorTipo(this.tipo);
    }

    public void ActualizarFilaAsociada()
    {
        filaAsociada.marcaUIAsociada = this;
        filaAsociada.icono.sprite = icono;
        filaAsociada.texto.text = nombre;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Seleccionar visualmente esta fila
        filaAsociada?.navegadorController.FilaSeleccionada(filaAsociada);
    }
}
