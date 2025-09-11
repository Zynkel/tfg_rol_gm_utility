using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static FiltroNavegadorController;

public class NavegadorController : MonoBehaviour
{
    public RectTransform content; // El content del ScrollView
    public GameObject filaPrefab; // El prefab de la fila
    public Button botonLimpiarFiltro;
    
    public DetalleMarcaController panelDetalles;
    public List<MarcaFilaUI> listaFilasNavegador = new List<MarcaFilaUI>();

    private MarcaFilaUI filaSeleccionada;

    private void Start()
    {
        if (ModoAplicacionController.Instancia != null)
        {
            ModoAplicacionController.Instancia.OnModoCambiado += OnModoCambiado;
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

        foreach (var fila in listaFilasNavegador)
        {
            if (fila == null || fila.marcaUIAsociada == null) continue;

            var estado = fila.marcaUIAsociada.estado;

            if (esJuego)
            {
                // En juego, ocultar marcas con estado Oculto
                fila.gameObject.SetActive(estado != EstadoMarca.Oculto);
            }
            else
            {
                // En edición, siempre se muestran todas
                fila.gameObject.SetActive(true);
            }
        }
    }

    public void AnyadirMarca(Sprite icono, string nombre, GameObject marcaMapa)
    {
        GameObject nuevaFila = Instantiate(filaPrefab, content); //Objeto fila
        MarcaUI marcaUI = marcaMapa.GetComponent<MarcaUI>(); 
        MarcaFilaUI filaUI = nuevaFila.GetComponent<MarcaFilaUI>();

        filaUI.Configurar(icono, nombre, marcaMapa, panelDetalles, this, marcaUI);
        marcaUI.filaAsociada = filaUI;

        listaFilasNavegador.Add(filaUI);

        forzarRefrescoNavegador();
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
        if (filtro.filtroAplicado)
        {
            botonLimpiarFiltro.gameObject.SetActive(true);
        } 
        else
        {
            botonLimpiarFiltro.gameObject.SetActive(false);
        }

        string tipoSeleccionado = filtro.tipo.ToString().ToLower();
        string estadoSeleccionado = filtro.estado.ToString().ToLower();
        string nombreTexto = filtro.nombre.ToLower();

        foreach (var fila in listaFilasNavegador)
        {
            MarcaUI marcaFilaActual = fila.marcaUIAsociada;

            bool coincideNombre = string.IsNullOrEmpty(nombreTexto) || marcaFilaActual.nombre.ToLower().Contains(nombreTexto);

            bool coincideEstado = estadoSeleccionado == "todos" || marcaFilaActual.estado.ToString() == estadoSeleccionado;

            bool coincideTipo = tipoSeleccionado == "todos" || marcaFilaActual.tipo.ToString() == tipoSeleccionado;

            marcaFilaActual.gameObject.SetActive(coincideNombre && coincideEstado && coincideTipo);
            fila.gameObject.SetActive(coincideNombre && coincideEstado && coincideTipo);            
        }
    }

    public void forzarRefrescoNavegador()
    {
        /*
         * Esto fuerza a Unity a recalcular el layout de la tabla para evitar que la primera fila salga 
         * entrecortada se recalcula el layout para colocarla correctamente. 
         */
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }
}
