using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VisorDropZone : MonoBehaviour, IDropHandler
{
    public Transform contenedorMarcas;
    public RawImage mapaCargado;

    public void OnDrop(PointerEventData eventData)
    {
        if (mapaCargado.texture != null)
        {
            GameObject marca = eventData.pointerDrag;

            if (marca != null)
            {
                MarcaItemUI marcaItemUIDropped = marca.GetComponent<MarcaItemUI>();
                Image imagenMarca = marcaItemUIDropped.icono;
                crearMarca(imagenMarca.sprite);
            }
        }        
    }

    public void crearMarca(Sprite imagenMarca)
    {
        GameObject nuevaMarca = new GameObject("MarcaColocada");
        nuevaMarca.transform.SetParent(contenedorMarcas, false);

        Image img = nuevaMarca.AddComponent<Image>();
        img.sprite = imagenMarca;
        img.raycastTarget = false;

        RectTransform rt = nuevaMarca.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(40, 40); // Ajusta el tamaño de la marca

        // Posicionar en el punto donde se suelta (centro del cursor)
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            this.transform as RectTransform,
            Input.mousePosition,
            null,
            out localPos);

        rt.localPosition = localPos;
    }
}
