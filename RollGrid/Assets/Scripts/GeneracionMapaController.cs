using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GeneracionMapaController : MonoBehaviour
{
    public GameObject panel;
    public TMP_InputField promtGeneracionText;
    public Button guardarBtn;
    public Button cancelarBtn;

    private VisorController visorController;

    public void Inicializar()
    {
        panel.SetActive(true);

        promtGeneracionText.text = "";

        cancelarBtn.onClick.RemoveAllListeners();
        cancelarBtn.onClick.AddListener(() =>
        {
            panel.SetActive(false);
        });
    }
}
