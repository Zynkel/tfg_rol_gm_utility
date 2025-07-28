using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static FiltroNavegadorController;

public class NavegadorController : MonoBehaviour
{
    public RectTransform content; // El content del ScrollView
    public GameObject filaPrefab; // El prefab de la fila
    public DetalleMarcaController panelDetalles;    

    private MarcaFilaUI filaSeleccionada;
    private List<MarcaFilaUI> listaFilasNavegador = new List<MarcaFilaUI>();


    public void AnyadirMarca(Sprite icono, string nombre, GameObject marcaMapa)
    {
        GameObject nuevaFila = Instantiate(filaPrefab, content); //Objeto fila
        MarcaUI marcaUI = marcaMapa.GetComponent<MarcaUI>(); 
        MarcaFilaUI filaUI = nuevaFila.GetComponent<MarcaFilaUI>();

        filaUI.Configurar(icono, nombre, marcaMapa, panelDetalles, this, marcaUI);
        marcaUI.filaAsociada = filaUI;

        listaFilasNavegador.Add(filaUI);

        /*
         * Esto fuerza a Unity a recalcular el layout de la tabla para evitar que la primera fila salga 
         * entrecortada se recalcula el layout para colocarla correctamente. 
         */
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    public void FilaSeleccionada(MarcaFilaUI nuevaFila)
    {
        if (filaSeleccionada != null && filaSeleccionada != nuevaFila)
        {
            filaSeleccionada.Deseleccionar();
        }

        filaSeleccionada = nuevaFila;
        filaSeleccionada.Seleccionar();
    }

    public void AplicarFiltro(FiltroNavegador filtro) 
    {
        string tipoSeleccionado = filtro.tipo.ToString();
        string estadoSeleccionado = filtro.estado.ToString();
        string nombreTexto = filtro.nombre.ToLower();

        foreach (var fila in listaFilasNavegador)
        {
            MarcaUI marcaFilaActual = fila.marcaUIAsociada;

            bool coincideNombre = string.IsNullOrEmpty(nombreTexto) || marcaFilaActual.nombre.ToLower().Contains(nombreTexto);

            bool coincideEstado = estadoSeleccionado == "Todos" || marcaFilaActual.estado.ToString() == estadoSeleccionado;

            bool coincideTipo = tipoSeleccionado == "Todos" || marcaFilaActual.tipo.ToString() == tipoSeleccionado;

            marcaFilaActual.gameObject.SetActive(coincideNombre && coincideEstado && coincideTipo);
            fila.gameObject.SetActive(coincideNombre && coincideEstado && coincideTipo);            
        }
    }
}
