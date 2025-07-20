using System.Collections.Generic;
using System.Windows.Forms;
using TMPro;
using UnityEngine;

public class VinculacionController : MonoBehaviour
{
    [Header("Elementos UI")]
    public GameObject panel;
    public TMP_Dropdown listadoMapas;
    public Button botonAceptar;    

    private MarcaUI marcaActual;

    public void Mostrar(MarcaUI marca, List<string> nombresMapasDisponibles)
    {
        marcaActual = marca;

        listadoMapas.ClearOptions();
        listadoMapas.AddOptions(nombresMapasDisponibles);

        panel.GetComponent<CanvasGroup>().alpha = 1;
        panel.GetComponent<CanvasGroup>().interactable = true;
        panel.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    public void Aceptar()
    {
        string mapaSeleccionado = listadoMapas.options[listadoMapas.value].text;
        marcaActual.mapaVinculado = mapaSeleccionado;

        panel.GetComponent<CanvasGroup>().alpha = 0;
        panel.GetComponent<CanvasGroup>().interactable = false;
        panel.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
}
