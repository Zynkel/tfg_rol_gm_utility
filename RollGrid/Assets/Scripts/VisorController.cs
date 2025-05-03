using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ListaMarcasUI;

public class VisorController : MonoBehaviour, IDropHandler
{
    public NavegadorManager navegadorManager; //Script que gestiona el navegador.
    public Transform contenedorMarcas;
    public RawImage mapaCargado;

    public void OnDrop(PointerEventData eventData)
    {
        if (mapaCargado.texture != null)
        {
            GameObject marca = eventData.pointerDrag;

            if (marca != null
                && marca.GetComponent<MarcaItemUI>() != null)
            {
                MarcaItemUI marcaItemUIDropped = marca.GetComponent<MarcaItemUI>();                
                crearMarca(marcaItemUIDropped);
            }
        }        
    }

    public void crearMarca(MarcaItemUI marcaDropped)
    {
        GameObject nuevaMarca = new GameObject("MarcaColocada");
        nuevaMarca.transform.SetParent(contenedorMarcas, false);
        Image imagenMarca = marcaDropped.icono;

        Image img = nuevaMarca.AddComponent<Image>();
        img.sprite = imagenMarca.sprite;
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

        string textoMarca = marcaDropped.GetTextoMarca();
        navegadorManager.AnyadirMarca(img.sprite, textoMarca, nuevaMarca);
    }

    public void CargarMapa()
    {
        string rutaRelativa = "Maps/map.png";
        string rutaMapa = Path.Combine(Application.dataPath, rutaRelativa);

        if (File.Exists(rutaMapa))
        {
            byte[] datosMapa = File.ReadAllBytes(rutaMapa);
            Texture2D mapa = new Texture2D(2, 2);
            if (mapa.LoadImage(datosMapa))
            {
                mapaCargado.texture = mapa;
            }
            else
            {
                Debug.LogError("Error al cargar mapa: " + rutaMapa);
            }
        }
        else
        {
            Debug.LogError("Archivo no encontrado: " + rutaMapa);
        }
    }
}
