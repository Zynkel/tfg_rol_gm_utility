using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEngine.Analytics.IAnalytic;

[System.Serializable]
public class MapasNombres
{
    public string id;
    public string nombre;
}

[System.Serializable]
public class ListaMapas
{
    public List<MapasNombres> mapas;
}

public class ListaMapasController : MonoBehaviour
{
    public string apiUrlBase = "http://localhost:8000/"; // URL de la APi
    public ListaMapas listaMapasNombres; //Listado de nombres de mapas obtenidos de la API.

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
        if (listaMapasNombres.mapas.Count == 0)
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

    public void SeleccionarFila(MapaFilaItemUI fila)
    {
        if (filaSeleccionada != null)
            filaSeleccionada.EstablecerSeleccionado(false);

        filaSeleccionada = fila;
        filaSeleccionada.EstablecerSeleccionado(true);
    }

    public void OnBotonBorrarMapa()
    {
        if (filaSeleccionada == null)
        {
            Debug.LogWarning("No hay ningún mapa seleccionado.");
            return;
        }

        StartCoroutine(BorrarMapa(filaSeleccionada.idMapa));
    }

    /*
     * Carga el listado de nombres llamando a la API y mostrando
     * la lista de nombres de todos los mapas
     */
    public IEnumerator ObtenerListadoMapas()
    {
        // Vaciar el contenido actual si hay
        foreach (Transform hijo in contenedorFila)
        {
            Destroy(hijo.gameObject);
        }

        using (UnityWebRequest www = UnityWebRequest.Get(apiUrlBase+"mapas"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al obtener mapas: " + www.error);
                yield break;
            }

            string json = www.downloadHandler.text;
            string jsonWrapped = "{\"mapas\":" + json + "}";
            listaMapasNombres = JsonUtility.FromJson<ListaMapas>(jsonWrapped);

            // Eliminar filas antiguos si se vuelven a cargar los mapas.
            foreach (Transform child in contenedorFila)
                Destroy(child.gameObject);

            // Crear fila por mapa
            if (listaMapasNombres != null)
            {
                bool primeraFila = true;

                foreach (var mapaActual in listaMapasNombres.mapas)
                {
                    GameObject filaGO = Instantiate(filaMapaPrefab, contenedorFila);
                    var itemUI = filaGO.GetComponent<MapaFilaItemUI>();

                    itemUI.Inicializar(mapaActual.nombre, mapaActual.id, this);

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

    /**
     * Obtiene el mapa y sus elementos del mapa pasado por parámetro,
     * llama a la API para obtener el mapa y muestra el mapa en el visor.
     */
    public IEnumerator CargarMapaDesdeAPI(string idMapa)
    {
        string urlImagen = $"{apiUrlBase}mapas/{idMapa}";

        using (UnityWebRequest www = UnityWebRequest.Get(urlImagen))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error cargando imagen: " + www.error);
                yield break;
            }

            string json = www.downloadHandler.text;
            MapaAPI mapa = JsonUtility.FromJson<MapaAPI>(json);

            visorController.ProcesarMapaAPI(mapa);
            OcultarPanel();
        }
    }

    /**
     * Obtiene el mapa y sus elementos del último mapa,
     * llama a la API para obtener el mapa y muestra el mapa en el visor.
     * 
     * Esto se usa tras subir un mapa generado para mostrarlo en el visor analizado.
     */
    public IEnumerator CargarUltimoMapaSubido()
    {
        MapasNombres ultimoMapa = listaMapasNombres.mapas.LastOrDefault();
        string idMapa = ultimoMapa.id;
        string urlImagen = $"{apiUrlBase}mapas/{idMapa}";

        using (UnityWebRequest www = UnityWebRequest.Get(urlImagen))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error cargando imagen: " + www.error);
                yield break;
            }

            string json = www.downloadHandler.text;
            MapaAPI mapa = JsonUtility.FromJson<MapaAPI>(json);

            visorController.ProcesarMapaAPI(mapa);
            OcultarPanel();
        }
    }

    public IEnumerator BorrarMapa(string id)
    {
        string urlImagen = $"{apiUrlBase}mapas/{id}";
        UnityWebRequest request = UnityWebRequest.Delete(urlImagen);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error borrando mapa: " + request.error);
        }
        else
        {
            Debug.Log("Mapa borrado correctamente");
            StartCoroutine(ObtenerListadoMapas());
        }
    }

}