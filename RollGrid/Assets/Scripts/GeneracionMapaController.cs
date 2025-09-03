using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GeneracionMapaController : MonoBehaviour
{
    [Header("Controladores")]
    public VisorController visorController;

    [Header("Elementos UI")]
    public GameObject panel;
    public TMP_InputField promtGeneracionText;
    public Button guardarBtn;
    public Button cancelarBtn;


    [System.Serializable]
    public class MapaGenerador
    {
        public string prompt;
        public string tipo = "auto";
        public string estilo = "default";
        public string modo = "text2img";
        public bool save = false;
    }

    [System.Serializable]
    public class RespuestaGeneracionMapa
    {
        public string image_base64;
        public string image_url;
        public string guide_url;
        public string prompt_final;
        public string layout;
    }

    public void Inicializar()
    {
        panel.SetActive(true);


        guardarBtn.onClick.RemoveAllListeners();
        guardarBtn.onClick.AddListener(() =>
        {
            OnAceptarPrompt();
        });

        cancelarBtn.onClick.RemoveAllListeners();
        cancelarBtn.onClick.AddListener(() =>
        {
            CerrarPanel();
        });
    }

    public void CerrarPanel()
    {
        guardarBtn.interactable = true;
        promtGeneracionText.text = "";
        panel.SetActive(false);
    }

    public void OnAceptarPrompt()
    {
        string promptUsuario = promtGeneracionText.text.Trim();

        if (string.IsNullOrEmpty(promptUsuario))
        {
            Debug.LogWarning("Debe introducir un prompt válido.");
            return;
        }

        // Crear el body de la petición
        MapaGenerador datos = new MapaGenerador();
        datos.prompt = promptUsuario;

        guardarBtn.interactable = false;
        StartCoroutine(visorController.GenerarMapaDesdePrompt(datos));
    }

}
