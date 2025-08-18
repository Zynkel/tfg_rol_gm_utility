using Newtonsoft.Json;
using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static ListaMarcasUI;

public class VisorController : MonoBehaviour, IDropHandler
{
    [Header("Controladores")]
    public NavegadorController navegadorManager; //Script que gestiona el navegador.
    public MarcasManager marcasManager; //Script que gestiona las marcas.
    public MenuContextualController menuContextualController;
    public SeleccionarEstadoController seleccionarEstadoController;
    public GuardarEstadoController guardarEstadoUI;
    [SerializeField] private DetalleMarcaController panelDetalles; 

    [Header("Elementos UI")]
    public RectTransform contenedorMarcas;
    public RawImage mapaImagenCargada;
    public string mapaNombreCargado;
    public GameObject fondoVisor;
    public GameObject prefabMarcaColocada;
    public GameObject cuadricula;
    public Image botonCuadricula; //Botón para cambiar su color.
    public GridOverlay gridOverlay;
    public TMP_Dropdown modoAplicacionDropdown;
    public Button botonMarcas;
    public Button botonListado;
    public Button botonGuardar;
    public Button botonCuadricular;

    //Objeto mapa actual.
    public MapaAPI mapaCargado;

    //Propiedades del zoom.
    public float zoomMin = 0.5f;
    public float zoomMax = 2.5f;
    public float zoomStep = 0.1f;
    public ScrollRect scrollRect;

    public bool permitirAñadirMarcas = true; 
    private bool arrastrandoConRueda = false;
    private Vector2 ultimaPosicionMouse;
    private bool cuadriculaVisible = false;
    private MarcaUI marcaMovida;
    private Vector3 escalaOriginal; //Zoom original del mapa.

    public string apiUrlBase = "http://localhost:8000/"; // URL de la APi

    private Color defaultColor = new Color32(0x1F, 0x1F, 0x1F, 0xFF); //1F1F1F
    private Color pressedColor = new Color32(0x3F, 0x3F, 0x3F, 0xFF); //3F3F3F

    void Start()
    {
        escalaOriginal = contenedorMarcas.localScale;

        // Suscribirse al cambio del dropdown
        if (modoAplicacionDropdown != null)
        {
            modoAplicacionDropdown.onValueChanged.AddListener(OnModoDropdownCambiado);
        }
    }

    private void OnDestroy()
    {
        if (modoAplicacionDropdown != null)
        {
            modoAplicacionDropdown.onValueChanged.RemoveListener(OnModoDropdownCambiado);
        }
    }

    private void OnEnable()
    {
        if (ModoAplicacionController.Instancia != null)
        {
            ModoAplicacionController.Instancia.OnModoCambiado += OnModoCambiado;
            // Aplicar el modo actual por si ya estaba en uno concreto
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

    private void OnModoCambiado(ModoAplicacion nuevoModo)
    {
        bool esJuego = nuevoModo == ModoAplicacion.Juego;
        permitirAñadirMarcas = !esJuego;
        Debug.Log("Visor ajustado a modo: " + nuevoModo);

        if (contenedorMarcas != null)
        {
            // Filtrar visibilidad marcas
            var marcas = contenedorMarcas.GetComponentsInChildren<MarcaColocada>(true);
            foreach (var marca in marcas)
            {

                if (esJuego)
                {
                    MarcaUI marcaActualUI = marca.GetComponent<MarcaUI>();
                    bool mostrar = marcaActualUI.estado == EstadoMarca.Activo || marcaActualUI.estado == EstadoMarca.Inactivo;
                    marca.ActualizarEstiloVisual();
                    marca.gameObject.SetActive(mostrar);
                }
                else
                {
                    marca.gameObject.SetActive(true);
                }
            }
        }

        //Botones del menú.
        if (esJuego)
        {
            botonCuadricular.interactable = false;
            botonGuardar.interactable = false;
            botonListado.interactable = false;
            botonMarcas.interactable = false;
        }
        else
        {
            botonCuadricular.interactable = true;
            botonGuardar.interactable = true;
            botonListado.interactable = true;
            botonMarcas.interactable = true;
        }
    }

    private void OnModoDropdownCambiado(int index)
    {
        // Suponiendo que el index 0 = Edicion, 1 = Juego
        ModoAplicacion nuevoModo = (index == 0) ? ModoAplicacion.Edicion : ModoAplicacion.Juego;

        // Cambiar el modo en el controlador central
        ModoAplicacionController.Instancia.CambiarModo(nuevoModo);

        Debug.Log("Dropdown cambió a: " + nuevoModo);
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


                //Volvemos al cursor normal
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
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
        if (mapaImagenCargada.texture != null && permitirAñadirMarcas)
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

    public void AbrirPanelGuardarEstado()
    {
        if (mapaCargado != null)
        {
            guardarEstadoUI.Inicializar(this, mapaCargado);
        }
    }

    //Creación y gestión de marcas.
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

    /**
     * Procesamiento y gestión del mapa.
     */
    public void ProcesarMapaAPI(MapaAPI mapa)
    {
        // 1. Cargar la imagen desde base64
        if (!string.IsNullOrEmpty(mapa.imagen_base64))
        {
            Texture2D textura = new Texture2D(2, 2);
            byte[] bytes = Convert.FromBase64String(mapa.imagen_base64);
            if (textura.LoadImage(bytes))
            {
    
                mapaImagenCargada.texture = textura;    
                //mapaImagenCargada.SetNativeSize();

                LimpiarMarcas();
            }
            else
            {
                Debug.LogWarning("No se pudo cargar la imagen del mapa.");
            }
        }

        // Cargar el estado
        if (mapa.estados == null || mapa.estados.Count == 0)
        {
            Debug.LogWarning("No hay estados disponibles en el mapa.");
            return;
        }

        //Solo hay un estado.
        if (mapa.estados.Count == 1)
        {
            EstadoMapa estado = mapa.estados[0];

            foreach (ObjetoMapa obj in estado.objetos)
            {
                // Cargar solo si hay posición
                if (obj.posicion == null) continue;

                // Instanciar prefab de marca
                GameObject marcaGO = Instantiate(prefabMarcaColocada, contenedorMarcas);
                MarcaColocada marca = marcaGO.GetComponent<MarcaColocada>();

                // Determinar posición relativa en UI
                RectTransform rt = marcaGO.GetComponent<RectTransform>();
                float anchoMapa = mapaImagenCargada.texture.width;
                float altoMapa = mapaImagenCargada.texture.height;

                float xUI = (obj.posicion.x / anchoMapa) * mapaImagenCargada.rectTransform.rect.width;
                float yUI = (obj.posicion.y / altoMapa) * mapaImagenCargada.rectTransform.rect.height;

                // Ajuste para que (0,0) esté en la esquina inferior izquierda:
                Vector2 posicionLocal = new Vector2(xUI, yUI);
                Vector2 posicionOffset = posicionLocal - (mapaImagenCargada.rectTransform.rect.size / 2f);

                rt.anchoredPosition = posicionOffset;

                //Obtener TipoMarca
                if (Enum.TryParse<TipoMarca>(obj.tipo.ToLower(), out TipoMarca tipo))
                {
                    Debug.Log($"Tipo válido: {tipo}"); // Ejemplo: "enemigo"
                }

                // Obtener el sprite según su tipo.
                Sprite icono = marcasManager.GetIconoPorTipo(tipo); // este método debe existir
                string nombre = obj.tipo;

                // Inicializar la marca
                marca.Inicializar(
                    nombre,
                    tipo,
                    icono,
                    marcasManager,
                    menuContextualController,
                    rt.anchoredPosition,
                    prefabMarcaColocada.transform
                );

                //Crear fila en el navegador
                navegadorManager.AnyadirMarca(marca.icono, marca.nombre, marcaGO);

            }
        } else //Varios estados.
        {
            /**
             * Se muestra el panel con los estados del mapa a elegir y se carga el seleccionado.
             * Si no se selecciona ninguno se carga el mapa vacío.
             */
            seleccionarEstadoController.Mostrar(mapa.estados, (indiceSeleccionado) =>
            {
                if (indiceSeleccionado.HasValue)
                {
                    EstadoMapa estado = mapa.estados[indiceSeleccionado.Value];

                    if (indiceSeleccionado > 0)
                    {
                        ColocarMarcasEnMapas(estado.objetos);
                    } 
                    else
                    {
                        foreach (ObjetoMapa obj in estado.objetos)
                        {
                            // Cargar solo si hay posición
                            if (obj.posicion == null) continue;

                            // Instanciar prefab de marca
                            GameObject marcaGO = Instantiate(prefabMarcaColocada, contenedorMarcas);
                            MarcaColocada marca = marcaGO.GetComponent<MarcaColocada>();

                            // Determinar posición relativa en UI
                            RectTransform rt = marcaGO.GetComponent<RectTransform>();
                            float anchoMapa = mapaImagenCargada.texture.width;
                            float altoMapa = mapaImagenCargada.texture.height;

                            float xUI = (obj.posicion.x / anchoMapa) * mapaImagenCargada.rectTransform.rect.width;
                            float yUI = (obj.posicion.y / altoMapa) * mapaImagenCargada.rectTransform.rect.height;

                            // Ajuste para que (0,0) esté en la esquina inferior izquierda:
                            Vector2 posicionLocal = new Vector2(xUI, yUI);
                            Vector2 posicionOffset = posicionLocal - (mapaImagenCargada.rectTransform.rect.size / 2f);

                            rt.anchoredPosition = posicionOffset;

                            //Obtener TipoMarca
                            if (Enum.TryParse<TipoMarca>(obj.tipo.ToLower(), out TipoMarca tipo))
                            {
                                Debug.Log($"Tipo válido: {tipo}"); // Ejemplo: "enemigo"
                            }

                            // Obtener el sprite según su tipo.
                            Sprite icono = marcasManager.GetIconoPorTipo(tipo); // este método debe existir
                            string nombre = obj.tipo;

                            // Inicializar la marca
                            marca.Inicializar(
                                nombre,
                                tipo,
                                icono,
                                marcasManager,
                                menuContextualController,
                                rt.anchoredPosition,
                                prefabMarcaColocada.transform
                            );

                            //Crear fila en el navegador
                            navegadorManager.AnyadirMarca(marca.icono, marca.nombre, marcaGO);

                        }
                    }                    
                }
                else
                {
                    Debug.Log("Cargando solo imagen sin estado");
                }
            });
        }

        mapaCargado = mapa;
        mapaNombreCargado = mapa.nombre;

        Debug.Log("Mapa procesado: " + mapa.nombre);
    }

    public IEnumerator CargarMapaYEstado(string mapaId, string estadoId)
    {
        // Llamada a tu API para obtener datos del mapa por ID
        using (UnityWebRequest www = UnityWebRequest.Get($"{apiUrlBase}mapas/{mapaId}"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al cargar el mapa desde puerta: " + www.error);
                yield break;
            }

            // Parsear la respuesta
            MapaAPI mapa = JsonUtility.FromJson<MapaAPI>(www.downloadHandler.text);

            // Cargar la imagen desde base64
            if (!string.IsNullOrEmpty(mapa.imagen_base64))
            {
                Texture2D textura = new Texture2D(2, 2);
                byte[] bytes = Convert.FromBase64String(mapa.imagen_base64);
                if (textura.LoadImage(bytes))
                {

                    mapaImagenCargada.texture = textura;
                    //mapaImagenCargada.SetNativeSize();

                    LimpiarMarcas();
                }
                else
                {
                    Debug.LogWarning("No se pudo cargar la imagen del mapa.");
                }
            }

            // Si se ha especificado un estado, cargarlo
            if (!string.IsNullOrEmpty(estadoId))
            {
                EstadoMapa estado = mapa.estados.FirstOrDefault(e => e.id == estadoId);
                if (estado != null)
                {
                    ColocarMarcasEnMapas(estado.objetos);
                }
                else
                {
                    Debug.LogWarning("El estado vinculado no se encontró en este mapa.");
                }
            }
        }
    }

    private void ColocarMarcasEnMapas(List<ObjetoMapa> listaObjetos)
    {
        float anchoMapaUI = mapaImagenCargada.rectTransform.rect.width;
        float altoMapaUI = mapaImagenCargada.rectTransform.rect.height;

        foreach (ObjetoMapa obj in listaObjetos)
        {
            if (obj.posicion == null) continue;

            GameObject marcaGO = Instantiate(prefabMarcaColocada, contenedorMarcas);
            MarcaColocada marca = marcaGO.GetComponent<MarcaColocada>();

            RectTransform rt = marcaGO.GetComponent<RectTransform>();

            // Convertir coordenadas relativas (0-1) a posición en UI
            float xUI = obj.posicion.x * anchoMapaUI;
            float yUI = obj.posicion.y * altoMapaUI;

            // Ajustar para que el origen (0,0) esté en el centro del mapa
            Vector2 posicionOffset = new Vector2(
                xUI - (anchoMapaUI / 2f),
                yUI - (altoMapaUI / 2f)
            );

            rt.anchoredPosition = posicionOffset;

            // Obtener tipo de marca
            TipoMarca tipo = TipoMarca.objeto;
            if (Enum.TryParse<TipoMarca>(obj.tipo, true, out var tipoParseado))
            {
                tipo = tipoParseado;
            }

            // Obtener sprite del tipo
            Sprite icono = marcasManager.GetIconoPorTipo(tipo);

            // Inicializar marca
            marca.Inicializar(
                obj.tipo,
                tipo,
                icono,
                marcasManager,
                menuContextualController,
                rt.anchoredPosition,
                prefabMarcaColocada.transform
            );

            // Añadir al navegador
            navegadorManager.AnyadirMarca(marca.icono, marca.nombre, marcaGO);
        }
    }

    public void GuardarEstadoDesdeUI(MapaAPI mapa, string nombreEstado)
    {
        StartCoroutine(GuardarEstadoEnMapaExistente(mapa, nombreEstado));
    }

    public IEnumerator GuardarEstadoEnMapaExistente(MapaAPI mapa, string nombreEstado)
    {
        EstadoMapa nuevoEstado = CrearEstadoDesdeVisor(nombreEstado);

        string jsonBody = JsonConvert.SerializeObject(nuevoEstado, Newtonsoft.Json.Formatting.None);

        string url = $"{apiUrlBase}mapas/{mapa.id}/estados";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Estado guardado correctamente.");
        }
        else
        {
            Debug.LogError($"Error al guardar estado: {request.responseCode} - {request.downloadHandler.text}");
        }
    }

    private EstadoMapa CrearEstadoDesdeVisor(string nombreEstado)
    {
        EstadoMapa estado = new EstadoMapa();
        estado.id = Guid.NewGuid().ToString(); // o dejarlo nulo si la API lo genera;
        estado.nombre = nombreEstado;
        estado.descripcion = "Guardado por el usuario";
        estado.fecha_creacion = DateTime.UtcNow.ToString("o");
        estado.objetos = ObtenerObjetosDesdeMarcas(); 
        estado.personajes = new List<PersonajeMapa>(); 
        return estado;
    }

    private List<ObjetoMapa> ObtenerObjetosDesdeMarcas()
    {
        List<ObjetoMapa> listaDeObjetos = new List<ObjetoMapa>();
        //Marcas colocadas.
        MarcaColocada[] marcas = UnityEngine.Object.FindObjectsByType<MarcaColocada>(FindObjectsSortMode.None);

        RectTransform mapaRT = mapaImagenCargada.rectTransform;
        float ancho = mapaRT.rect.width;
        float alto = mapaRT.rect.height;

        foreach (MarcaColocada marca in marcas)
        {
            Vector2 posUI = ((RectTransform)marca.transform).anchoredPosition;

            // Convertir a coordenadas relativas (0-1)
            float xRel = (posUI.x + ancho / 2f) / ancho;
            float yRel = (posUI.y + alto / 2f) / alto;

            ObjetoMapa objeto = new ObjetoMapa();
            objeto.tipo = marca.tipo.ToString().ToLower();
            objeto.descripcion = null;
            objeto.contenido = null;
            objeto.estado = null;
            objeto.posicion = new Posicion
            {
                x = xRel,
                y = yRel
            };

            objeto.bounding_box = null; 
            objeto.destino_mapa_id = marca.marcaUI.mapaVinculado;
            objeto.destino_estado_id = null;

            listaDeObjetos.Add(objeto);
        }

        return listaDeObjetos;
    }


    public Boolean hayMapaCargado()
    {
        return mapaImagenCargada != null;
    }

    /**
     * Métodos de zoom
     */
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

    //Cuadrícula.
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
}
