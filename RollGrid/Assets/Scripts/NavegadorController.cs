using UnityEngine;
using UnityEngine.UI;

public class NavegadorController : MonoBehaviour
{
    public RectTransform content; // El content del ScrollView
    public GameObject filaPrefab; // El prefab de la fila
    public DetalleMarcaController panelDetalles;

    private MarcaFilaUI filaSeleccionada;

    public void AnyadirMarca(Sprite icono, string nombre, GameObject marcaMapa)
    {
        GameObject nuevaFila = Instantiate(filaPrefab, content); //Objeto fila
        MarcaUI marcaUI = marcaMapa.GetComponent<MarcaUI>(); 
        MarcaFilaUI filaUI = nuevaFila.GetComponent<MarcaFilaUI>();

        filaUI.Configurar(icono, nombre, marcaMapa, panelDetalles, this, marcaUI);
        marcaUI.filaAsociada = filaUI;

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
}
