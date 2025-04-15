using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CargaMapa : MonoBehaviour
{
    public RawImage mapaVisor;

    public void CargarMapa()
    {
        string rutaRelativa = "Maps/map.png";
        string rutaMapa = Path.Combine(Application.dataPath, rutaRelativa);

        if(File.Exists(rutaMapa))
        {
            byte[] datosMapa = File.ReadAllBytes(rutaMapa);
            Texture2D mapa = new Texture2D(2, 2);
            if (mapa.LoadImage(datosMapa))
            {
                mapaVisor.texture = mapa;
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
