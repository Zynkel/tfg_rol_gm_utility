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
    public VinculacionController vinculacionController;
    public TMP_InputField inputNombre;
    public TMP_Dropdown dropdownTipo;
    public TMP_Dropdown dropdownEstado;
    public TMP_InputField inputNotas;
    public TMP_InputField textoCoordenadaX;
    public TMP_InputField textoCoordenadaY;
    public Button botonGuardar;
    public Button botonRecolocar;
    public Button botonVincular;
    [SerializeField] private Texture2D cursorDeRecolocar;
    private GameObject marcaActual;
    private RectTransform panelDetalles;
    private Vector2 offset;

    [Header("Controladores")]
    public ListaMapasController listaMapasController;
    [SerializeField] private MarcasManager marcasManager;
    [SerializeField] private VisorController visorController; //Controlador del visor para mover marca.

    private MarcaUI marcaUI;

    public void MostrarDetalles(GameObject marca)
    {
        InicializarDropdowns();
        panel.GetComponent<CanvasGroup>().alpha = 1;
        panel.GetComponent<CanvasGroup>().interactable = true;
        panel.GetComponent<CanvasGroup>().blocksRaycasts = true;
        marcaActual = marca;

        marcaUI = marca.GetComponent<MarcaUI>();
        //Seleccionar la fila en el navegador si no est�.
        MarcaFilaUI filaDeLaMarca = marca.GetComponent<MarcaFilaUI>();
        filaDeLaMarca.Seleccionar();

        inputNombre.text = marcaUI.nombre;

        dropdownTipo.SetValueWithoutNotify((int)marcaUI.tipo);
        dropdownEstado.SetValueWithoutNotify((int)marcaUI.estado);

        inputNotas.text = marcaUI.notas;
        textoCoordenadaX.text = $"({marca.transform.localPosition.x:0})";
        textoCoordenadaY.text = $"({marca.transform.localPosition.y:0})";

        // Activar el bot�n de vincular solo si es de tipo "puerta"
        botonVincular.interactable = (marcaUI.tipo == TipoMarca.Puerta);
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

        panel.GetComponent<CanvasGroup>().alpha = 0;
        panel.GetComponent<CanvasGroup>().interactable = false;
        panel.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void RecolocarMarca()
    {
        if (marcaActual == null) return;

        //Atenuamos el panel para dar indicaciones de que estamos en modo mover y para que se visualice mejor el visor.
        CanvasGroup cg = this.GetComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;
        cg.alpha = 0.3f;

        //Cambiamos el cursor
        Cursor.SetCursor(cursorDeRecolocar, Vector2.zero, CursorMode.Auto);

        MarcaUI marcaUI = marcaActual.GetComponent<MarcaUI>();
        visorController.MoverMarca(marcaUI);
    }

    public void ActualizarCoordenadas(Vector2 localPos)
    {
        textoCoordenadaX.text = $"({Mathf.RoundToInt(localPos.x)}";
        textoCoordenadaY.text = $"({Mathf.RoundToInt(localPos.y)})";

        //Volvemos a mostrar el panel completo.
        CanvasGroup cg = this.GetComponent<CanvasGroup>();
        cg.interactable = true;
        cg.blocksRaycasts = true;
        cg.alpha = 1.0f;

        //Restaurar cursor.
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void Cancelar()
    {
        panel.GetComponent<CanvasGroup>().alpha = 0;
        panel.GetComponent<CanvasGroup>().interactable = false;
        panel.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void CambiarTipoMarca(int index)
    {
        if (marcaActual == null
            && marcaActual.GetComponent<MarcaUI>() == null) return;

        MarcaUI marcaUI = marcaActual.GetComponent<MarcaUI>();
        marcaUI.tipo = (TipoMarca)index;

        //Al cambiar el tipo se comprueba si es tipo marca.
        botonVincular.interactable = (marcaUI.tipo == TipoMarca.Puerta);

        marcaUI.ActualizarIcono();
    }

    public void CambiarEstadoMarca(int index)
    {
        if (marcaActual == null
            && marcaActual.GetComponent<MarcaFilaUI>() == null) return;

        MarcaFilaUI marcaFila = marcaActual.GetComponent<MarcaFilaUI>();
    }

    //Comprueba si el tipo de la marca es de Puerta, en caso afirmativo activa el bot�n Vincular.
    public void ComprobarTipo(int index)
    {
        //Al cambiar el tipo se comprueba si es tipo marca.
        botonVincular.interactable = ((TipoMarca)index == TipoMarca.Puerta);
    }

    public void AlPulsarBotonVincular()
    {
        vinculacionController.Mostrar(marcaUI, listaMapasController.listaMapasNombres.images);
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
