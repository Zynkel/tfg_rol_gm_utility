using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CargaMapa : MonoBehaviour
{
    public RawImage mapaVisor;

    public string rutaMapa = "map.png";
    // M�todo Start: Se ejecuta una vez cuando el objeto est� activo, similar a un constructor "inicializado" en Java o un m�todo que llamas al inicio en Python.
    void Start()
    {
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

    // M�todo Update: Se ejecuta en cada frame (similar al bucle principal, pero Unity lo gestiona de forma automatizada).
    private void Update()
    {
        
    }
}
