using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class MapaFilaItemUI : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI textoNombreMapa;
    public string nombreMapa;
    public string idMapa;

    private ListaMapasController mapasController;
    private float ultimoClickTime;
    private const float tiempoDobleClick = 0.3f;
    private Color colorSeleccionado = new Color(0.3f, 0.3f, 0.3f);// Seleccionado azulado
    private Color colorDefecto = new Color(0.7f, 0.7f, 0.7f);

    public void Inicializar(string nombre, string id, ListaMapasController listaMapasController)
    {
        nombreMapa = nombre;
        idMapa = id;
        textoNombreMapa.text = nombre;
        mapasController = listaMapasController;

        GetComponent<Image>().color = colorDefecto;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        float tiempoDesdeUltimoClick = Time.time - ultimoClickTime;

        if (tiempoDesdeUltimoClick < tiempoDobleClick)
        {
            //Llamar al controlador para cargar mapa
            StartCoroutine(mapasController.CargarMapaDesdeAPI(idMapa));
        } else
        {
            mapasController.SeleccionarFila(this);
        }

        ultimoClickTime = Time.time;
    }

    public void EstablecerSeleccionado(bool seleccionado)
    {
        GetComponent<Image>().color = seleccionado ? colorSeleccionado : colorDefecto;
    }

    public string GetNombreMapa() => nombreMapa;
}
