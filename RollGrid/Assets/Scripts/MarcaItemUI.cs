using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarcaItemUI : MonoBehaviour
{
    public Image fondo;
    public TextMeshProUGUI texto;
    public Image icono;

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
}