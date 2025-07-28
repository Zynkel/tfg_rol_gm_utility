using UnityEngine;
using UnityEngine.UI;

public class MarcaColocada : MonoBehaviour
{
    public string nombre;
    public TipoMarca tipo;
    public Sprite icono;
    public MarcaUI marcaUI;

    private RectTransform rectTransform;

    public void Inicializar(string nombre, TipoMarca tipo, Sprite icono, MarcasManager marcasManager, MenuContextualController menuContextual, Vector2 posicionPantalla, Transform canvasTransform)
    {
        this.nombre = nombre;
        this.tipo = tipo;
        this.icono = icono;

        // Imagen de la marca
        Image img = gameObject.GetComponent<Image>();
        img.sprite = icono;
        img.raycastTarget = true;

        // Tama�o y posici�n
        rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(40, 40);

        // Convertir posici�n pantalla a local en el canvas
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasTransform as RectTransform,
            posicionPantalla,
            null,
            out localPos);
        rectTransform.localPosition = localPos;

        // A�adir y configurar MarcaUI
        marcaUI = gameObject.GetComponent<MarcaUI>();
        marcaUI.Inicializar(nombre, tipo, icono, marcasManager, menuContextual);
    }
}
