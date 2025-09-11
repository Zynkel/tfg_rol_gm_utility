using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MarcaFilaUI : MonoBehaviour
{
    public Image icono;
    public TextMeshProUGUI texto;
    public Button botonEditar;
    public Button botonEliminar;
    [HideInInspector] public NavegadorController navegadorController;
    public MarcaUI marcaUIAsociada;

    [SerializeField] private GameObject marcaAsociada;
    private bool estaSeleccionada = false;
    private Color colorSeleccionado = new Color(0.3f, 0.3f, 0.3f);// Seleccionado azulado
    private Color colorNormal = new Color(0.8f, 0.9f, 0f, 0f); //Normal transparente
    private DetalleMarcaController detalleController;
    private CanvasGroup canvasGroup;

    private void OnEnable()
    {
        if (ModoAplicacionController.Instancia != null)
        {
            ModoAplicacionController.Instancia.OnModoCambiado += OnModoCambiado;
            OnModoCambiado(ModoAplicacionController.Instancia.ModoActual);
        }
    }

    private void OnDisable()
    {
        if (ModoAplicacionController.Instancia != null)
        {
            ModoAplicacionController.Instancia.OnModoCambiado -= OnModoCambiado;
        }
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void ActualizarEstiloVisual(bool esJuego)
    {
        if (esJuego && marcaUIAsociada != null && marcaUIAsociada.estado == EstadoMarca.Inactivo)
        {
            canvasGroup.alpha = 0.7f;
        }
        else
        {
            canvasGroup.alpha = 1f;
        }
    }

    private void OnModoCambiado(ModoAplicacion nuevoModo)
    {
        bool esJuego = nuevoModo == ModoAplicacion.Juego;

        // En modo juego, botón eliminar deshabilitado
        if (botonEliminar != null)
        {
            botonEliminar.interactable = !esJuego;
        }

        // Filtrar visibilidad
        if (marcaAsociada != null)
        {
            bool mostrar = !esJuego || (marcaUIAsociada.estado == EstadoMarca.Activo || marcaUIAsociada.estado == EstadoMarca.Inactivo);
            this.gameObject.SetActive(mostrar);
        }

        // Aplicar estilo grisado si está inactiva
        ActualizarEstiloVisual(esJuego);
    }

    public void Configurar(Sprite sprite, string nombre, GameObject marca, DetalleMarcaController detalles, NavegadorController navController, MarcaUI marcaUI)
    {
        icono.sprite = sprite;
        texto.text = nombre;
        marcaAsociada = marca;
        marcaUIAsociada = marcaUI;
        detalleController = detalles;
        navegadorController = navController;

        MarcaFilaUI filaMarcaAsociada = marcaAsociada.AddComponent<MarcaFilaUI>();
        filaMarcaAsociada.icono = icono;
        filaMarcaAsociada.texto = texto;
        filaMarcaAsociada.marcaAsociada = marca;
        filaMarcaAsociada.marcaUIAsociada = marcaUI;

        botonEliminar.onClick.AddListener(EliminarMarca);
        botonEditar.onClick.AddListener(() => OnBotonEditar());

        Deseleccionar();
    }   

    public void Seleccionar()
    {
        estaSeleccionada = true;
        GetComponent<Image>().color = colorSeleccionado;

        if (marcaAsociada != null)
        {
            // Ejemplo: resaltar marca aumentando su tamaño o cambiando color
            marcaAsociada.transform.localScale = Vector3.one * 1.4f;
            Outline outline = marcaAsociada.GetComponent<Outline>();
            outline.enabled = true;
        }
    }

    public void Deseleccionar()
    {
        estaSeleccionada = false;
        GetComponent<Image>().color = colorNormal;

        if (marcaAsociada != null)
        {
            marcaAsociada.transform.localScale = Vector3.one;
            Outline outline = marcaAsociada.GetComponent<Outline>();
            outline.enabled = false;
        }
    }

    public void OnBotonEditar()
    {
        if (marcaAsociada != null)
        {
            var modo = ModoAplicacionController.Instancia.ModoActual;
            detalleController.MostrarDetalles(marcaAsociada, modo);
        }
    }

    public void AlClickear()
    {
        navegadorController.FilaSeleccionada(this);
    }

    public bool isSeleccionada()
    {
        return estaSeleccionada;
    }

    public void EliminarMarca()
    {        
        // Limpia el objeto seleccionado si es este mismo
        if (EventSystem.current.currentSelectedGameObject == this.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // Limpia también cualquier referencia de puntero activa
        PointerEventData pointer = new PointerEventData(EventSystem.current)
        {
            pointerEnter = null
        };

        // Opcional: desactiva el raycast antes de destruir (por seguridad)
        Graphic graphic = GetComponent<Graphic>();
        if (graphic != null)
        {
            graphic.raycastTarget = false;
        }

        // Destruye primero la marca asociada si existe
        if (marcaAsociada != null)
        {
            Destroy(marcaAsociada);
            marcaAsociada = null;
        }

        //Elimina la fila del listado
        if (navegadorController.listaFilasNavegador.Contains(this))
        {
            navegadorController.listaFilasNavegador.Remove(this);
        }

        // Luego destruye esta fila del navegador
        this.gameObject.SetActive(false);

        // Destruir desde un objeto activo
        SafeDestroyer.Instance.DestroySafely(this.gameObject);

        detalleController.CerrarPanel();
        marcaUIAsociada.menuContextual.Cerrar();
    }
}