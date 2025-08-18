using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using static ListaMarcasUI;
using System.Collections.Generic;

public class DetalleMarcaController : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public GameObject panel; 

    [Header("Referencias UI")]
    public TMP_InputField inputNombre;
    public TMP_Dropdown dropdownTipo;
    public TMP_Dropdown dropdownEstado;
    public TMP_InputField inputNotas;
    public Button botonGuardar;
    public Button botonRecolocar;
    public Button botonVincular;
    public ScrollRect scrollRect;
    public Scrollbar scrollbar;
    [SerializeField] private Texture2D cursorDeRecolocar;
    private GameObject marcaActual;
    private RectTransform panelDetalles;
    private Vector2 offset;

    [Header("Controladores")]
    public VinculacionController vinculacionController;
    public ListaMapasController listaMapasController;
    [SerializeField] public VisorController visorController; //Controlador del visor para mover marca.
    [SerializeField] private MarcasManager marcasManager;

    private MarcaUI marcaUI;

    void Update()
    {
        if (scrollRect.verticalNormalizedPosition < 1f)
            scrollbar.gameObject.SetActive(true);
        else
            scrollbar.gameObject.SetActive(false);
    }

    public void MostrarDetalles(GameObject marca, ModoAplicacion modo)
    {
        panel.SetActive(true);
        InicializarDropdowns();
        panel.GetComponent<CanvasGroup>().interactable = true;
        panel.GetComponent<CanvasGroup>().blocksRaycasts = true;
        marcaActual = marca;

        marcaUI = marca.GetComponent<MarcaUI>();

        inputNombre.text = marcaUI.nombre;

        dropdownTipo.SetValueWithoutNotify((int)marcaUI.tipo);
        dropdownEstado.SetValueWithoutNotify((int)marcaUI.estado);

        inputNotas.text = marcaUI.notas;

        // Aplicar bloqueo según modo
        bool esJuego = (modo == ModoAplicacion.Juego);
        inputNombre.interactable = !esJuego;
        dropdownTipo.interactable = !esJuego;
        dropdownEstado.interactable = !esJuego;
        inputNotas.interactable = !esJuego;
        botonRecolocar.interactable = !esJuego;

        // Activar el botón de vincular solo si es de tipo "puerta"
        botonVincular.interactable = (marcaUI.tipo == TipoMarca.puerta) && !esJuego;
    }

    public void InicializarDropdowns()
    {
        dropdownTipo.ClearOptions();
        dropdownTipo.AddOptions(marcasManager.ObtenerOpcionesTipoMarca());
        dropdownTipo.onValueChanged.AddListener(ComprobarTipo);

        dropdownEstado.ClearOptions();
        dropdownEstado.AddOptions(marcasManager.ObtenerOpcionesEstadoMarca());
    }

    public void GuardarCambios()
    {
        if (marcaActual == null) return;

        //Detalles de MarcaUI
        MarcaUI marcaUI = marcaActual.GetComponent<MarcaUI>();
        marcaUI.nombre = inputNombre.text;
        marcaUI.tipo = (TipoMarca)dropdownTipo.value;
        marcaUI.estado = (EstadoMarca)dropdownEstado.value;
        marcaUI.notas = inputNotas.text;
        marcaUI.ActualizarIcono();

        //Actualizar imagen de la MarcaColocada para visualizar en el visor.
        Image iconoMarcaColocada = marcaActual.GetComponent<Image>();
        iconoMarcaColocada.sprite = marcaUI.icono;

        marcaUI.ActualizarFilaAsociada(); //Actualiza la MarcaFilaUI asociada para visualizarse en el navegador.

        CerrarPanel();
    }

    public void CerrarPanel()
    {
        panel.GetComponent<CanvasGroup>().interactable = false;
        panel.GetComponent<CanvasGroup>().blocksRaycasts = false;
        panel.SetActive(false);
    }

    public void RecolocarMarca()
    {
        if (marcaActual == null) return;

        //Atenuamos el panel para dar indicaciones de que estamos en modo mover y para que se visualice mejor el visor.
        CanvasGroup cg = this.GetComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;

        //Cambiamos el cursor
        Cursor.SetCursor(cursorDeRecolocar, Vector2.zero, CursorMode.Auto);

        MarcaUI marcaUI = marcaActual.GetComponent<MarcaUI>();
        visorController.MoverMarca(marcaUI);

        //Volvemos al cursor normal
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void CambiarTipoMarca(int index)
    {
        if (marcaActual == null
            && marcaActual.GetComponent<MarcaUI>() == null) return;

        MarcaUI marcaUI = marcaActual.GetComponent<MarcaUI>();
        marcaUI.tipo = (TipoMarca)index;

        //Al cambiar el tipo se comprueba si es tipo marca.
        botonVincular.interactable = (marcaUI.tipo == TipoMarca.puerta);

        marcaUI.ActualizarIcono();
    }

    public void CambiarEstadoMarca(int index)
    {
        if (marcaActual == null
            && marcaActual.GetComponent<MarcaFilaUI>() == null) return;

        MarcaFilaUI marcaFila = marcaActual.GetComponent<MarcaFilaUI>();


    }

    //Comprueba si el tipo de la marca es de Puerta, en caso afirmativo activa el botón Vincular.
    public void ComprobarTipo(int index)
    {
        //Al cambiar el tipo se comprueba si es tipo marca.
        botonVincular.interactable = ((TipoMarca)index == TipoMarca.puerta);
    }

    public void AlPulsarBotonVincular()
    {
        vinculacionController.Mostrar(marcaUI, listaMapasController.listaMapasNombres.mapas);
    }

    //En Awake, guardamos la referencia al RectTransform del panel. Esto se hace una vez al iniciar.
    void Awake()
    {
        panelDetalles = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelDetalles,
            eventData.position,
            eventData.pressEventCamera,
            out offset
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localMousePosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelDetalles.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localMousePosition))
        {
            panelDetalles.localPosition = localMousePosition - offset;
        }
    }
}
