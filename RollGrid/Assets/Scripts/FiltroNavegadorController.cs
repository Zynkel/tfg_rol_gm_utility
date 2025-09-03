using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FiltroNavegadorController : MonoBehaviour
{
    public GameObject panel; // Asigna el panel en el Inspector    

    public class FiltroNavegador
    {
        public string nombre;        
        public string tipo;
        public string estado;
        public bool filtroAplicado;

        public FiltroNavegador(string nombre, string tipo, string estado, bool filtrado)
        {
            this.nombre = nombre;
            this.tipo = tipo;
            this.estado = estado;
            filtroAplicado = filtrado;
        }
    }

    [Header("Referencias UI")]
    public TMP_InputField inputNombre;
    public TMP_Dropdown dropdownTipo;
    public TMP_Dropdown dropdownEstado;
    public Button botonAplicar;

    [SerializeField] private MarcasManager marcasManager;
    [SerializeField] private NavegadorController navegadorController;
    private bool dropdownsInicializados = false;

    public void CargarDatos()
    {
        InicializarDropdowns();
        panel.GetComponent<CanvasGroup>().alpha = 1;
        panel.GetComponent<CanvasGroup>().interactable = true;
        panel.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
    
    public void Aplicar()
    {
        string nombre = inputNombre.text;
        string tipoFiltro = dropdownTipo.options[dropdownTipo.value].text;
        string estadoFiltro = dropdownEstado.options[dropdownEstado.value].text;
        bool filtroAplicado = false;

        /**
         * Se comprueba si se aplica algún filtro, en ese caso se indica para mostrar el botón de limpiar.
         */
        if (!string.IsNullOrEmpty(nombre) || !string.IsNullOrEmpty(tipoFiltro) || !string.IsNullOrEmpty(estadoFiltro))
        {
            filtroAplicado = true;
        }

        FiltroNavegador filtro = new FiltroNavegador(nombre, tipoFiltro, estadoFiltro, filtroAplicado);
        navegadorController.AplicarFiltro(filtro);
        SalirFiltros();
    }

    public void LimpiarFiltro()
    {
        string nombre = "";
        string tipoFiltro = "Todos";
        string estadoFiltro = "Todos";

        inputNombre.text = "";
        dropdownTipo.value = 0;
        dropdownTipo.RefreshShownValue();
        dropdownEstado.value = 0;
        dropdownEstado.RefreshShownValue();

        FiltroNavegador filtro = new FiltroNavegador(nombre, tipoFiltro, estadoFiltro, false);
        navegadorController.AplicarFiltro(filtro);
    }

    public void SalirFiltros()
    {
        panel.GetComponent<CanvasGroup>().alpha = 0;
        panel.GetComponent<CanvasGroup>().interactable = false;
        panel.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
    private void InicializarDropdowns()
    {
        if (!dropdownsInicializados)
        {
            var opcionesTipo = new List<string> { "Todos", "Enemigo", "Tesoro", "Puerta", "Objeto" };
            dropdownTipo.ClearOptions();
            dropdownTipo.AddOptions(opcionesTipo);

            var opcionesEstado = new List<string> { "Todos", "Activo", "Inactivo", "Oculto" };
            dropdownEstado.ClearOptions();
            dropdownEstado.AddOptions(opcionesEstado);

            dropdownsInicializados = true;
        }        
    }
}
