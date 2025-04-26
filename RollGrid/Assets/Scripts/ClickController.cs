using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickController : MonoBehaviour
{
    public GameObject panelMarcas;
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;
    public Image imagenBoton;

    private Color defaultColor = new Color32(0x1F, 0x1F, 0x1F, 0xFF); //1F1F1F

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(eventSystem);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> resultados = new List<RaycastResult>();
            raycaster.Raycast(pointerData, resultados);

            bool clicDentroDelPanel = false;
            foreach (var r in resultados)
            {
                if (r.gameObject == panelMarcas || r.gameObject.transform.IsChildOf(panelMarcas.transform))
                {
                    imagenBoton.color = defaultColor;
                    clicDentroDelPanel = true;
                    break;
                }
            }

            if (!clicDentroDelPanel)
            {
                panelMarcas.GetComponent<CanvasGroup>().alpha = 0f;
            }
            else
            {
            }
        }
    }

    private bool resultadoEsMarca(List<RaycastResult> resultados)
    {
        foreach (var r in resultados)
        {
            if (r.gameObject.CompareTag("Marca")) // Usa tag "Marca" en prefabs
                return true;
        }
        return false;
    }
}
