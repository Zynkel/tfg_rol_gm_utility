using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System;

public class SeleccionarEstadoController : MonoBehaviour
{
    public GameObject panel;
    public TMP_Dropdown estadosDropdown;
    public Button seleccionarBtn;
    public Button cancelarBtn;

    private List<string> nombreEstados = new List<string>();
    private Action<int?> callbackSeleccion;

    public void Mostrar(List<EstadoMapa> estados, Action<int?> callback)
    {
        panel.SetActive(true);
        callbackSeleccion = callback;

        estadosDropdown.ClearOptions();
        estadosDropdown.AddOptions(listarNombresEstados(estados));

        seleccionarBtn.onClick.RemoveAllListeners();
        seleccionarBtn.onClick.AddListener(() =>
        {
            int indice = estadosDropdown.value;
            nombreEstados.Clear();
            panel.SetActive(false);
            callbackSeleccion?.Invoke(indice);
        });

        cancelarBtn.onClick.RemoveAllListeners();
        cancelarBtn.onClick.AddListener(() =>
        {
            panel.SetActive(false);
            nombreEstados.Clear();
            callbackSeleccion?.Invoke(null); // cargar solo imagen
        });
    }

    private List<string> listarNombresEstados (List<EstadoMapa> estados)
    {
        foreach (EstadoMapa estadoActual in estados)
        {
            nombreEstados.Add(estadoActual.nombre);
        }

        return nombreEstados;
    }
}
