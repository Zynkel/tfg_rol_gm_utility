using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ListaMarcasUI : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    [System.Serializable]
    public class Marca
    {
        public string nombre;
        public Sprite icono;
        public TipoMarca tipo;

        public Marca(string nombre, Sprite icono, TipoMarca tipo)
        {
            this.nombre = nombre;
            this.icono = icono;
            this.tipo = tipo;
        }
    }

    public Transform contenedor;
    public GameObject prefabMarca;

    public Sprite iconoEnemigo;
    public Sprite iconoTesoro;
    public Sprite iconoPuerta;
    public Sprite iconoObjeto;

    public CanvasGroup ventanaCanvasGroup;
    public Image imagenBoton;

    public GameObject ventanaMarcas;

    private RectTransform panelMarcas;
    private Vector2 offset;
    private Color defaultColor = new Color32(0x1F, 0x1F, 0x1F, 0xFF); //1F1F1F
    private Color pressedColor = new Color32(0x3F, 0x3F, 0x3F, 0xFF); //3F3F3F

    private MarcaItemUI marcaSeleccionada;

    void Start()
    {
        // Crear la lista de marcas
        Marca[] marcas = new Marca[]
        {
            new Marca("Enemigo", iconoEnemigo, TipoMarca.enemigo),
            new Marca("Tesoro", iconoTesoro, TipoMarca.tesoro),
            new Marca("Puerta", iconoPuerta, TipoMarca.puerta),
            new Marca("Objeto", iconoObjeto, TipoMarca.objeto),
        };

        // Por cada marca se crea el objeto MarcaItemUI para asignar texto e imagen.
        foreach (Marca marca in marcas)
        {
            CrearItem(marca);
        }
    }

    void CrearItem(Marca marca)
    {
        GameObject item = Instantiate(prefabMarca, contenedor);
        MarcaItemUI ui = item.GetComponent<MarcaItemUI>();
        ui.Configurar(marca.nombre, marca.icono, marca.tipo);
        item.GetComponent<Button>().onClick.AddListener(() => SeleccionarMarca(ui));
    }

    void SeleccionarMarca(MarcaItemUI marca)
    {
        if (marcaSeleccionada != null)
            marcaSeleccionada.Desmarcar();

        marcaSeleccionada = marca;
        marcaSeleccionada.Seleccionar();
    }

    public void TogglePanel()
    {
        // Se oculta el panel, el botón aparece sin pulsar.
        if (ventanaCanvasGroup.alpha == 1f)
        {
            OcultarPanel();
        }
        else // Se activa el panel, el botón se muestra presionado.
        {
            MostrarPanel();
        }
    }

    public void MostrarPanel()
    {
        imagenBoton.color = pressedColor;
        ventanaCanvasGroup.alpha = 1f;
        ventanaCanvasGroup.interactable = true;
        ventanaCanvasGroup.blocksRaycasts = true;
        ventanaMarcas.SetActive(true);
    }

    public void OcultarPanel()
    {
        imagenBoton.color = defaultColor;
        ventanaCanvasGroup.alpha = 0f;
        ventanaCanvasGroup.interactable = false;
        ventanaCanvasGroup.blocksRaycasts = false;
        ventanaMarcas.SetActive(false);
    }

    //En Awake, guardamos la referencia al RectTransform del panel. Esto se hace una vez al iniciar.
    void Awake()
    {
        panelMarcas = GetComponent<RectTransform>();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelMarcas,
            eventData.position,
            eventData.pressEventCamera,
            out offset
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localMousePosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelMarcas.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localMousePosition))
        {
            panelMarcas.localPosition = localMousePosition - offset;
        }
    }
}