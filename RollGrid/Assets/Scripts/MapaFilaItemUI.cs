using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class MapaFilaItemUI : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI textoNombreMapa;

    private ListaMapasController mapasController;
    private string nombreMapa;
    private float ultimoClickTime;
    private const float tiempoDobleClick = 0.3f;

    public void Inicializar(string nombre, ListaMapasController listaMapasController)
    {
        nombreMapa = nombre;
        textoNombreMapa.text = nombre;
        mapasController = listaMapasController;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        float tiempoDesdeUltimoClick = Time.time - ultimoClickTime;

        if (tiempoDesdeUltimoClick < tiempoDobleClick)
        {
            //Llamar al controlador para cargar mapa
            StartCoroutine(mapasController.CargarMapaDesdeAPI(nombreMapa));
        } else
        {
            mapasController.SeleccionarFila(this);
        }

        ultimoClickTime = Time.time;
    }

    public void EstablecerSeleccionado(bool seleccionado)
    {
        GetComponent<Image>().color = seleccionado ? new Color(0.7f, 0.7f, 1f) : Color.white;
    }

    public string GetNombreMapa() => nombreMapa;
}
