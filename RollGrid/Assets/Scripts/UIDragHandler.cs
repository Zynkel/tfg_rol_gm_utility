using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [Header("Zona desde donde se puede arrastrar")]
    public RectTransform barraTitulo; // Asignar en el inspector

    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 offset;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Verificar si el click está dentro de la barra de título
        if (!RectTransformUtility.RectangleContainsScreenPoint(barraTitulo, eventData.position, eventData.pressEventCamera))
            return;

        // Calcular offset
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out offset
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Si no empezó el drag desde la barra, no mover
        if (!RectTransformUtility.RectangleContainsScreenPoint(barraTitulo, eventData.position, eventData.pressEventCamera))
            return;

        if (canvas == null) return;

        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos))
        {
            rectTransform.localPosition = pos - offset;
        }
    }
}
