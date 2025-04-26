using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MarcaItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image fondo;
    public TextMeshProUGUI texto;
    public Image icono;

    //Variables privadas para arrastar la marca.
    private Transform originalParent;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Color colorNormal = new Color32(0xEF, 0xEA, 0xBB, 0x00); //EFEAB8
    private Color colorSeleccionado = new Color32(0x93, 0x93, 0x93, 0xFF); //939393

    public void Configurar(string nombre, Sprite sprite)
    {
        texto.text = nombre;
        icono.sprite = sprite;
        Desmarcar();
    }

    public void Seleccionar()
    {
        fondo.color = colorSeleccionado;
    }

    public void Desmarcar()
    {
        fondo.color = colorNormal;
    }

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(canvas.transform); // Saca el ítem del layout para arrastrarlo libremente
        canvasGroup.blocksRaycasts = false; // Permite detectar el drop en otras zonas
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pos);

        transform.localPosition = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(originalParent); // Vuelve a la lista si no se soltó en un receptor válido
        canvasGroup.blocksRaycasts = true;
        transform.localPosition = Vector3.zero;
    }
}