using SFB;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;
using static ListaMarcasUI;

public class VisorController : MonoBehaviour, IDropHandler
{
    public NavegadorController navegadorManager; //Script que gestiona el navegador.
    public MarcasManager marcasManager; //Script que gestiona las marcas.
    public Transform contenedorMarcas;
    public RawImage mapaImagenCargada;
    public ListaMapasController.MapaData mapaCargado;
    public MenuContextualController menuContextualController;

    private MarcaUI marcaMovida;
    [SerializeField] private DetalleMarcaController panelDetalles;

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
        GameObject nuevaMarca = new GameObject("MarcaColocada");
        nuevaMarca.transform.SetParent(contenedorMarcas, false);
        Image imagenMarca = marcaDropped.icono;

        nuevaMarca.AddComponent<CircleCollider2D>();

        Image img = nuevaMarca.AddComponent<Image>();
        img.sprite = imagenMarca.sprite;
        img.raycastTarget = true;

        RectTransform rt = nuevaMarca.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(40, 40); // Ajusta el tamaño de la marca

        // Posicionar en el punto donde se suelta (centro del cursor)
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            this.transform as RectTransform,
            Input.mousePosition,
            null,
            out localPos);

        rt.localPosition = localPos;

        string textoMarca = marcaDropped.GetTextoMarca();

        MarcaUI marcaUI = nuevaMarca.AddComponent<MarcaUI>();
        marcaUI.Inicializar(textoMarca, marcaDropped.tipo, img.sprite, marcasManager, menuContextualController);


        navegadorManager.AnyadirMarca(img.sprite, textoMarca, nuevaMarca);
    }

    public void MoverMarca(MarcaUI marca)
    {
        marcaMovida = marca;
    }

    void Update()
    {
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

                if (panelDetalles != null)
                {
                    panelDetalles.ActualizarCoordenadas(localPos);
                }

                marcaMovida = null;
            }
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
    }

    /**
     * Se llama para evitar realizar acciones que requieren un mapa previo cargado en el visor.
     */
    public Boolean hayMapaCargado()
    {
        return mapaImagenCargada != null;
    }

    private System.Collections.IEnumerator CargarImagen(string path)
    {
        // Cargar archivo como textura
        var www = new WWW("file://" + path);
        yield return www;

        Texture2D tex = www.texture;
        if (tex != null)
        {
            mapaImagenCargada.texture = tex;
        }
        else
        {
            Debug.LogError("Error al cargar la imagen.");
        }
    }
}
