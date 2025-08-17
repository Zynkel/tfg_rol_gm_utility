using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum TipoMarca
{
    objeto, //Tipo 0 genérico
    enemigo, //1
    tesoro, //2
    puerta //3
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
    public string mapaVinculado;
    public string estadoVinculado;

    public MarcasManager managerMarcas;
    public MenuContextualController menuContextual;

    public void Inicializar(string nombre, TipoMarca tipo, Sprite sprite, MarcasManager manager, MenuContextualController menu)
    {
        this.nombre = nombre;
        this.tipo = tipo;
        this.estado = EstadoMarca.Activo;
        this.notas = "";
        mapaVinculado = "";
        icono = sprite;
        managerMarcas = manager;
        menuContextual = menu;
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

        Vector2 posicionPantalla = Input.mousePosition;
        if (gameObject!= null)
        {
            menuContextual.Mostrar(posicionPantalla, gameObject);
        }    
    }
}
