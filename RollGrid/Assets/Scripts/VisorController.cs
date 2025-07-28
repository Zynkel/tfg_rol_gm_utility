using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ListaMarcasUI;

public class VisorController : MonoBehaviour, IDropHandler
{
    [Header("Controladores")]
    public NavegadorController navegadorManager; //Script que gestiona el navegador.
    public MarcasManager marcasManager; //Script que gestiona las marcas.
    public MenuContextualController menuContextualController;
    public ListaMapasController.MapaData mapaCargado;

    [Header("Elementos UI")]
    public RectTransform contenedorMarcas;
    public RawImage mapaImagenCargada;
    public string mapaNombreCargado;
    public GameObject prefabMarcaColocada;
    public GameObject cuadricula;
    public Image botonCuadricula; //Botón para cambiar su color.
    public GridOverlay gridOverlay;

    public float zoomMin = 0.5f;
    public float zoomMax = 2.5f;
    public float zoomStep = 0.1f;
    public ScrollRect scrollRect;

    private bool arrastrandoConRueda = false;
    private Vector2 ultimaPosicionMouse;
    private bool cuadriculaVisible = false;
    private MarcaUI marcaMovida;
    [SerializeField] private DetalleMarcaController panelDetalles;
    private Vector3 escalaOriginal; //Zoom original del mapa.

    private Color defaultColor = new Color32(0x1F, 0x1F, 0x1F, 0xFF); //1F1F1F
    private Color pressedColor = new Color32(0x3F, 0x3F, 0x3F, 0xFF); //3F3F3F

    void Start()
    {
        escalaOriginal = contenedorMarcas.localScale;
    }

    void Update()
    {
        //Control para mover la marca.
        if (marcaMovida != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 localPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    this.transform as RectTransform,
                    Input.mousePosition,
                    null,
                    out localPos);

                // Limitar posición dentro del visor
                localPos = UIUtils.LimitarPosicionDentroRectTransform(this.transform as RectTransform,
                                                                        localPos, 20f);

                marcaMovida.transform.localPosition = localPos;

                marcaMovida = null;

                //Habilitamos de nuevo el panel de detalle.
                CanvasGroup cg = panelDetalles.GetComponent<CanvasGroup>();
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }

        //Zoom y desplazamiento
        if (hayMapaCargado())
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (Mathf.Abs(scroll) > 0.01f)
            {
                float nuevoZoom = contenedorMarcas.localScale.x + scroll * zoomStep * 10f;
                nuevoZoom = Mathf.Clamp(nuevoZoom, zoomMin, zoomMax);
                contenedorMarcas.localScale = Vector3.one * nuevoZoom;
                gridOverlay.SetVerticesDirty();
            }

            // Comienza el arrastre con la rueda del ratón (botón 2)
            if (Input.GetMouseButtonDown(2))
            {
                arrastrandoConRueda = true;
                ultimaPosicionMouse = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(2))
            {
                arrastrandoConRueda = false;
            }

            if (arrastrandoConRueda)
            {
                Vector2 actualMouse = Input.mousePosition;
                Vector2 delta = actualMouse - ultimaPosicionMouse;

                // Invertir el delta porque queremos mover el mapa en sentido opuesto al mouse
                scrollRect.content.anchoredPosition += delta;
                gridOverlay.SetVerticesDirty();

                ultimaPosicionMouse = actualMouse;
            }

            if (Input.GetMouseButtonUp(1))
            {
                ResetZoom();
                gridOverlay.SetVerticesDirty();
            }
        }       
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (mapaImagenCargada.texture != null)
        {
            GameObject marca = eventData.pointerDrag;

            if (marca != null
                && marca.GetComponent<MarcaItemUI>() != null)
            {
                MarcaItemUI marcaItemUIDropped = marca.GetComponent<MarcaItemUI>();                
                crearMarca(marcaItemUIDropped);
            }
        }        
    }

    public void crearMarca(MarcaItemUI marcaDropped)
    {
        GameObject marcaColocadaInstancia = Instantiate(prefabMarcaColocada, contenedorMarcas, false);
        MarcaColocada marca = marcaColocadaInstancia.GetComponent<MarcaColocada>();

        // Posicionar en el punto donde se suelta (centro del cursor)
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            this.transform as RectTransform,
            Input.mousePosition,
            null,
            out localPos);

        marca.Inicializar(marcaDropped.GetTextoMarca(), marcaDropped.tipo, marcaDropped.icono.sprite, marcasManager, menuContextualController, localPos, prefabMarcaColocada.transform);
        
        navegadorManager.AnyadirMarca(marca.icono, marca.nombre, marcaColocadaInstancia);
    }

    public void MoverMarca(MarcaUI marca)
    {
        marcaMovida = marca;
    }

    public void LimpiarMarcas()
    {
        MarcaColocada[] marcas = UnityEngine.Object.FindObjectsByType<MarcaColocada>(FindObjectsSortMode.None);

        foreach (MarcaColocada marca in marcas)
        {
            // Elimina su fila del navegador si está vinculada
            if (marca.marcaUI.filaAsociada != null)
            {
                Destroy(marca.marcaUI.filaAsociada.gameObject);
            }

            Destroy(marca.gameObject);
        }
    }   

    public void CargarImagenDesdeArchivo()
    {
        var extensiones = new[] {
            new ExtensionFilter("Imagenes", "png", "jpg", "jpeg")
        };

        // Abre el diálogo de selección de archivo
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Selecciona un mapa", "", extensiones, false);

        if (paths.Length > 0 && File.Exists(paths[0]))
        {            
            StartCoroutine(CargarImagen(paths[0]));
        }
    }

    public void CargarMapaDesdeLista(Texture2D textura)
    {
        mapaImagenCargada.texture = textura;
        LimpiarMarcas();
    }

    /**
     * Se llama para evitar realizar acciones que requieren un mapa previo cargado en el visor.
     */
    public Boolean hayMapaCargado()
    {
        return mapaImagenCargada != null;
    }

    public void ZoomIn()
    {
        float nuevoZoom = Mathf.Min(contenedorMarcas.localScale.x + zoomStep, zoomMax);
        contenedorMarcas.localScale = Vector3.one * nuevoZoom;
    }

    public void ZoomOut()
    {
        float nuevoZoom = Mathf.Max(contenedorMarcas.localScale.x - zoomStep, zoomMin);
        contenedorMarcas.localScale = Vector3.one * nuevoZoom;
    }

    public void ResetZoom()
    {
        contenedorMarcas.localScale = escalaOriginal;
    }

    public void MostrarOcultarCuadricula()
    {
        if (!cuadriculaVisible)
        {
            cuadricula.SetActive(true);
            cuadriculaVisible = true;
            botonCuadricula.color = pressedColor;
        } else
        {
            cuadricula.SetActive(false);
            cuadriculaVisible = false;
            botonCuadricula.color = defaultColor;
        }
    }

    private System.Collections.IEnumerator CargarImagen(string path)
    {
        // Cargar archivo como textura
        var www = new WWW("file://" + path);
        yield return www;

        mapaNombreCargado = Path.GetFileName(path); // Solo el nombre del archivo

        Texture2D tex = www.texture;
        if (tex != null)
        {
            mapaImagenCargada.texture = tex;
            LimpiarMarcas();
        }
        else
        {
            Debug.LogError("Error al cargar la imagen.");
        }
    }
}
