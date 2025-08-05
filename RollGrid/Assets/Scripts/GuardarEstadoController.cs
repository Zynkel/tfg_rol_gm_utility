using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GuardarEstadoController : MonoBehaviour
{
    public GameObject panel;
    public TMP_InputField nombreEstadoInput;
    public Button guardarBtn;
    public Button cancelarBtn;

    private VisorController visorController;
    private MapaAPI mapaEnCurso;

    public void Inicializar(VisorController controlador, MapaAPI mapa)
    {
        panel.SetActive(true);
        visorController = controlador;
        mapaEnCurso = mapa;

        nombreEstadoInput.text = "";

        guardarBtn.onClick.RemoveAllListeners();
        guardarBtn.onClick.AddListener(() =>
        {
            string nombre = nombreEstadoInput.text.Trim();
            if (!string.IsNullOrEmpty(nombre)) //Si no se ha introduccido nombre no se guarda.
            {
                panel.SetActive(false);
                visorController.GuardarEstadoDesdeUI(mapaEnCurso, nombre);
            }
        });

        cancelarBtn.onClick.RemoveAllListeners();
        cancelarBtn.onClick.AddListener(() =>
        {
            panel.SetActive(false);
        });
    }
}
