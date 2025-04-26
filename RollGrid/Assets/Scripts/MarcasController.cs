using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MarcasController : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public CanvasGroup ventanaCanvasGroup;
    public Image imagenBoton;

    private RectTransform panelMarcas;
    private Vector2 offset;
    private Color defaultColor = new Color32(0x1F, 0x1F, 0x1F, 0xFF); //1F1F1F
    private Color pressedColor = new Color32(0x3F, 0x3F, 0x3F, 0xFF); //3F3F3F

    public void TogglePanel()
    {
        // Se oculta el panel, el botón aparece sin pulsar.
        if (ventanaCanvasGroup.alpha == 1f)
        {
            OcultarPanel();
        } else // Se activa el panel, el botón se muestra presionado.
        {
            MostrarPanel();
        }
    }

    public void MostrarPanel()
    {
        imagenBoton.color = pressedColor;
        ventanaCanvasGroup.alpha = 1f;
        ventanaCanvasGroup.interactable = true;
        ventanaCanvasGroup.blocksRaycasts = true;
    }

    public void OcultarPanel()
    {
        imagenBoton.color = defaultColor;
        ventanaCanvasGroup.alpha = 0f;
        ventanaCanvasGroup.interactable = false;
        ventanaCanvasGroup.blocksRaycasts = false;
    }

    //En Awake, guardamos la referencia al RectTransform del panel. Esto se hace una vez al iniciar.
    void Awake()
    {
        panelMarcas = GetComponent<RectTransform>();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelMarcas,
            eventData.position,
            eventData.pressEventCamera,
            out offset
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localMousePosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelMarcas.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localMousePosition))
        {
            panelMarcas.localPosition = localMousePosition - offset;
        }
    }
}
