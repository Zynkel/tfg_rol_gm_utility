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

        public FiltroNavegador(string nombre, string tipo, string estado)
        {
            this.nombre = nombre;
            this.tipo = tipo;
            this.estado = estado;
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
        FiltroNavegador filtro = new FiltroNavegador(nombre, tipoFiltro, estadoFiltro);
        navegadorController.AplicarFiltro(filtro);
        SalirFiltros();
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
