using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuContextualController : MonoBehaviour
{
    [Header("Elementos UI")]
    public GameObject panel;
    public Button botonVerDetalles;
    public Button botonViajar;
    public Button botonEliminar;
    public Button botonCerrar;

    public ListaMapasController listaMapasController;
    public VisorController visorController;
    public DetalleMarcaController detalleController;
    private MarcaUI marcaActual;
    private ModoAplicacion modo;

    private void Start()
    {
        if (ModoAplicacionController.Instancia != null)
        {
            ModoAplicacionController.Instancia.OnModoCambiado += OnModoCambiado;
            // Aplicar el modo actual por si ya estaba en uno concreto
            OnModoCambiado(ModoAplicacionController.Instancia.ModoActual);
        }            
    }

    private void OnDestroy()
    {
        if (ModoAplicacionController.Instancia != null)
            ModoAplicacionController.Instancia.OnModoCambiado -= OnModoCambiado;
    }

    private void OnModoCambiado(ModoAplicacion nuevoModo)
    {
        if (marcaActual != null)
            modo = nuevoModo;
    }

    public void Mostrar(Vector2 posicionPantalla, GameObject marca)
    {
        marcaActual = marca.GetComponent<MarcaUI>();

        panel.GetComponent<CanvasGroup>().alpha = 1;
        panel.GetComponent<CanvasGroup>().interactable = true;
        panel.GetComponent<CanvasGroup>().blocksRaycasts = true;
        panel.transform.position = posicionPantalla;

        if (modo == ModoAplicacion.Edicion) //Modo edición, se puede abrir editar y eliminar.
        {
            // Mostrar opciones normales
            if (botonVerDetalles != null) botonVerDetalles.gameObject.SetActive(true);
            if (botonEliminar != null) botonEliminar.gameObject.SetActive(true);

            //Mostrar detalle de la marca.
            botonVerDetalles.onClick.RemoveAllListeners();
            botonVerDetalles.onClick.AddListener(() =>
            {
                OnBotonDetalles(marca);
            });

            //Eliminar marca desde el menú contextual.
            botonEliminar.onClick.RemoveAllListeners();
            botonEliminar.onClick.AddListener(() =>
            {
                MarcaUI marcaUI = marca.GetComponent<MarcaUI>();
                marcaUI.filaAsociada.EliminarMarca();
                Cerrar();
            });
        }
        else // Modo juego, detalles y eliminar desactivados, solo botón viajar si se cumple la condición.
        {
            if (marcaActual.tipo == TipoMarca.puerta && !string.IsNullOrEmpty(marcaActual.mapaVinculado))
            {
                // Solo mostrar viajar
                if (botonVerDetalles != null) botonVerDetalles.gameObject.SetActive(false);
                if (botonEliminar != null) botonEliminar.gameObject.SetActive(false);
            }
            else
            {
                // Ninguna acción disponible → cerrar menú
                Cerrar();
            }
        }

        

        // Habilitar o deshabilitar el botón de viajar."
        bool puedeCambiar = marcaActual.tipo == TipoMarca.puerta && !string.IsNullOrEmpty(marcaActual.mapaVinculado);
        botonViajar.interactable = puedeCambiar;

        if (puedeCambiar)
        {
            botonViajar.onClick.RemoveAllListeners();
            botonViajar.onClick.AddListener(() =>
            {
                //Llamar al controlador para cargar mapa
                CargarMapaDesdePuerta(marcaActual.mapaVinculado, marcaActual.estadoVinculado);
                Cerrar();
            });
        }       

        //Funcionalidad botón cerrar del menú.
        botonCerrar.onClick.RemoveAllListeners();
        botonCerrar.onClick.AddListener(Cerrar);
    }

    public void OnBotonDetalles(GameObject marca)
    {
        if (marca != null)
        {
            var modo = ModoAplicacionController.Instancia.ModoActual;
            detalleController.MostrarDetalles(marca, modo);
        }
    }

    public void CargarMapaDesdePuerta(string mapaId, string estadoId)
    {
        // 1. Comprobar que los IDs son válidos
        if (string.IsNullOrEmpty(mapaId))
        {
            Debug.LogError("No se ha especificado un mapa vinculado para esta puerta.");
            return;
        }

        // 2. Iniciar la carga del mapa desde la API
        StartCoroutine(visorController.CargarMapaYEstado(mapaId, estadoId));
    }

    public void Cerrar()
    {
        panel.GetComponent<CanvasGroup>().alpha = 0;
        panel.GetComponent<CanvasGroup>().interactable = false;
        panel.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
}
