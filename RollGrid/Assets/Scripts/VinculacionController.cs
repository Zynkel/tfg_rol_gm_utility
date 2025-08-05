using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class VinculacionController : MonoBehaviour
{
    [Header("Controladores")]
    public SeleccionarEstadoController seleccionarEstadoController;

    [Header("Elementos UI")]
    public GameObject panel;
    public TMP_Dropdown listadoMapas;
    public Button botonAceptar;
    public string apiUrlBase = "http://localhost:8000/"; // URL de la APi

    private MarcaUI marcaActual;
    private List<MapasNombres> mapasDisponibles;

    public class ListaEstados
    {
        public List<EstadoMapa> estados;
    }

    public void Mostrar(MarcaUI marca, List<MapasNombres> mapasDisponibles)
    {
        marcaActual = marca;
        this.mapasDisponibles = mapasDisponibles;

        listadoMapas.ClearOptions();
        listadoMapas.AddOptions(listarNombreMapas(mapasDisponibles));

        panel.GetComponent<CanvasGroup>().alpha = 1;
        panel.GetComponent<CanvasGroup>().interactable = true;
        panel.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    public void Aceptar()
    {
        //Guardamos en la marca el ID del mapa.
        marcaActual.mapaVinculado = mapasDisponibles[listadoMapas.value].id;
        StartCoroutine(ObtenerEstadosMapa(marcaActual.mapaVinculado));

        panel.GetComponent<CanvasGroup>().alpha = 0;
        panel.GetComponent<CanvasGroup>().interactable = false;
        panel.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    private List<string> listarNombreMapas(List<MapasNombres> mapasDisponibles)
    {
        List<string> nombresMapasDisponibles = new List<string>();

        foreach (MapasNombres mapaActual in mapasDisponibles)
        {
            nombresMapasDisponibles.Add(mapaActual.nombre);
        }

        return nombresMapasDisponibles;
    }

    private void aplicarEstadoIdAMarca(List<EstadoMapa> estados)
    {
        if (estados.Count == 1 )
        {
            marcaActual.estadoVinculado = estados[0].id;
        } 
        else if (estados.Count > 1)
        {
            seleccionarEstadoController.Mostrar(estados, (indiceSeleccionado) =>
            {
                if (indiceSeleccionado.HasValue)
                {
                    EstadoMapa estado = estados[indiceSeleccionado.Value];

                    marcaActual.estadoVinculado = estado.id;
                }
            });
        }    
    }

    public IEnumerator ObtenerEstadosMapa(string idMapa)
    {
        string urlEstados = $"{apiUrlBase}mapas/{idMapa}/estados";

        using (UnityWebRequest www = UnityWebRequest.Get(urlEstados))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error cargando imagen: " + www.error);
                yield break;
            }

            string json = www.downloadHandler.text;
            string jsonWrapped = "{\"estados\":" + json + "}";
            ListaEstados listaEstados = JsonUtility.FromJson<ListaEstados>(jsonWrapped);

            aplicarEstadoIdAMarca(listaEstados.estados); //Vamos a aplicar el id del estado a la referencia de la marca.
        }
    }
}
