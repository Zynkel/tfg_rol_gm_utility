using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class MapasNombres
{
    public List<string> images; //Se tiene que llamar igual que el objeto del JSON, en este caso se llama 'images'
}

public class ListaMapasController : MonoBehaviour
{
    public string apiUrlBase = "http://localhost:8000/"; // URL de la APi
    public MapasNombres listaMapasNombres; //Listado de nombres de mapas obtenidos de la API.

    [Header("Elementos UI")]
    public GameObject panelListado;
    public GameObject filaMapaPrefab;
    public RectTransform contenedorFila;
    public CanvasGroup ventanaCanvasGroup;
    public Image imagenBoton;

    public VisorController visorController;




    private MapaFilaItemUI filaSeleccionada;
    private Color defaultColor = new Color32(0x1F, 0x1F, 0x1F, 0xFF); //1F1F1F
    private Color pressedColor = new Color32(0x3F, 0x3F, 0x3F, 0xFF); //3F3F3F

    /**
     * Clase auxiliar para guardar los datos del mapa, tanto para cargar
     * como para enviar.
     */
    public class MapaData
    {
        public string name;
        public string data;
        public string image_base64;
    }

    public void TogglePanel()
    {
        if (ventanaCanvasGroup.alpha == 1f)
        {
            OcultarPanel();
        }
        else 
        {
            MostrarListadoMapas();
        }
    }

    public void MostrarListadoMapas()
    {
        imagenBoton.color = pressedColor;
        ventanaCanvasGroup.alpha = 1f;
        ventanaCanvasGroup.interactable = true;
        ventanaCanvasGroup.blocksRaycasts = true;

        //Evitamos cargar continuamente la lista de mapa haciendo llamadas a la API cada vez que se abre el listado, se usará un botón de actualizar para hacerlo manualmente.
        if (listaMapasNombres.images.Count == 0)
        {
            StartCoroutine(ObtenerListadoMapas());
        } 
    }
    public void OcultarPanel()
    {
        imagenBoton.color = defaultColor;
        ventanaCanvasGroup.alpha = 0f;
        ventanaCanvasGroup.interactable = false;
        ventanaCanvasGroup.blocksRaycasts = false;
    }
    /*
     * Carga el listado de nombres llamando a la API y mostrando
     * la lista de nombres de todos los mapas
     */
    IEnumerator ObtenerListadoMapas()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(apiUrlBase+"images"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al obtener mapas: " + www.error);
                yield break;
            }

            string json = www.downloadHandler.text;
            listaMapasNombres = JsonUtility.FromJson<MapasNombres>(json);

            // Eliminar filas antiguos si se vuelven a cargar los mapas.
            foreach (Transform child in contenedorFila)
                Destroy(child.gameObject);

            // Crear fila por mapa
            if (listaMapasNombres != null)
            {
                bool primeraFila = true;

                foreach (var nombre in listaMapasNombres.images)
                {
                    GameObject filaGO = Instantiate(filaMapaPrefab, contenedorFila);
                    var itemUI = filaGO.GetComponent<MapaFilaItemUI>();

                    itemUI.Inicializar(nombre, this);

                    if (primeraFila)
                    {
                        /*
                        * Esto fuerza a Unity a recalcular el layout de la tabla para evitar que la primera fila salga 
                        * entrecortada se recalcula el layout para colocarla correctamente. 
                        */
                        LayoutRebuilder.ForceRebuildLayoutImmediate(contenedorFila);
                        primeraFila = false;
                    }
                }
            }            
        }
    }

    public void SeleccionarFila(MapaFilaItemUI fila)
    {
        if (filaSeleccionada != null)
            filaSeleccionada.EstablecerSeleccionado(false);

        filaSeleccionada = fila;
        filaSeleccionada.EstablecerSeleccionado(true);
    }

    /**
     * Obtiene el mapa y sus elementos del mapa pasado por parámetro,
     * llama a la API para obtener el mapa y muestra el mapa en el visor.
     */
    public IEnumerator CargarMapaDesdeAPI(string nombre)
    {
        string urlImagen = $"{apiUrlBase}image/{nombre}";

        using (UnityWebRequest www = UnityWebRequest.Get(urlImagen))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error cargando imagen: " + www.error);
                yield break;
            }

            string json = www.downloadHandler.text;
            MapaData mapa = JsonUtility.FromJson<MapaData>(json);

            // Convertir Base64 a textura
            byte[] imageBytes = Convert.FromBase64String(mapa.image_base64);
            Texture2D textura = new Texture2D(2, 2); // Tamaño temporal, se ajustará
            textura.LoadImage(imageBytes);

            visorController.CargarMapaDesdeLista(textura);
            visorController.mapaNombreCargado = nombre;
            OcultarPanel();
        }
    }

    public void GuardarMapaEnBD()
    {
        //Tenemos un marca cargado y no está en la lista que hemos obtenido de la API.
        if (visorController.hayMapaCargado()
                && !listaMapasNombres.images.Contains(visorController.mapaNombreCargado))
        {
            StartCoroutine(GuardarMapaCoroutine((Texture2D) visorController.mapaImagenCargada.texture));
        }
    }

    private IEnumerator GuardarMapaCoroutine(Texture2D textura)
    {
        string url = $"{apiUrlBase}upload"; // Sustituye con la URL real

        string base64 = ConvertirImagenABase64(textura);

        // Crear la solicitud POST
        UnityWebRequest www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(base64);

        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "text/plain"); // o application/octet-stream si tu API lo requiere

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al guardar el mapa: " + www.error);
        }
        else
        {
            Debug.Log("Mapa guardado correctamente en la base de datos.");
        }
        
    }

    private string ConvertirImagenABase64(Texture2D textura)
    {
        byte[] bytes = textura.EncodeToPNG(); // O EncodeToJPG()
        return Convert.ToBase64String(bytes);
    }
}