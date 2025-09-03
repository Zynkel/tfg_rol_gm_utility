using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.IO;
using SFB;
using System;

public class SubirMapaController : MonoBehaviour
{
    [Header("Controladores")]
    public ListaMapasController listadoMapasController;
    public VisorController visorController;

    [Header("Componentes UI")]
    public GameObject panelNombreMapa;
    public TMP_InputField inputNombreMapa;
    public Button botonConfirmarNombre;
    public Button botonCancelar;

    [Header("URL API")]
    public string apiUrl = "http://localhost:8000/mapas"; // URL de la APi

    private string rutaImagenSeleccionada;
    private bool subidaMapaLocal = true;

    void Start()
    {
        botonConfirmarNombre.onClick.RemoveAllListeners();
        botonConfirmarNombre.onClick.AddListener(OnConfirmarNombre);
        botonCancelar.onClick.RemoveAllListeners();
        botonCancelar.onClick.AddListener(OnCancelar);
    }

    // Llamado desde el botón en la UI
    public void OnBotonCargarMapaLocal()
    {
        subidaMapaLocal = true;

        var extensiones = new[] {
            new ExtensionFilter("Imagenes", "png", "jpg", "jpeg")
        };

        // Abre el diálogo de selección de archivo
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Selecciona un mapa", "", extensiones, false);

        // Abrir selector de archivos
        if (paths.Length > 0 && File.Exists(paths[0]))
        {
            rutaImagenSeleccionada = paths[0];
            panelNombreMapa.SetActive(true); // Mostrar panel para poner el nombre
            inputNombreMapa.text = "";
        }
    }

    // Llamado desde el botón del visor
    public void OnBotonSubirMapaGenerado()
    {
        subidaMapaLocal = false;
        panelNombreMapa.SetActive(true); // Mostrar panel para poner el nombre
        inputNombreMapa.text = "";
    }

    private void OnConfirmarNombre()
    {
        string nombreMapa = inputNombreMapa.text.Trim();
        if (string.IsNullOrEmpty(nombreMapa))
        {
            Debug.LogWarning("Debe introducir un nombre para el mapa.");
            return;
        }

        //La subida de un mapa desde la máquina local
        if (subidaMapaLocal)
        {
            StartCoroutine(SubirMapa(nombreMapa, rutaImagenSeleccionada));
        }
        else //Subida de mapa generado
        {
            StartCoroutine(visorController.SubirMapaGenerado(nombreMapa));
        }
    }

    private void OnCancelar()
    {
        inputNombreMapa.text = "";
        panelNombreMapa.SetActive(false);
    }

    private IEnumerator SubirMapa(string nombreMapa, string rutaImagen)
    {
        // Leer imagen como bytes
        byte[] imagenBytes = File.ReadAllBytes(rutaImagen);
        // Convertir bytes a string binario
        string imagenBinaria = System.Text.Encoding.UTF8.GetString(imagenBytes);

        // Preparar POST
        // Detectar tipo MIME según extensión
        string extension = Path.GetExtension(rutaImagen).ToLower();
        string mimeType;
        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                mimeType = "image/jpeg";
                break;
            case ".png":
                mimeType = "image/png";
                break;
            default:
                mimeType = "application/octet-stream"; // genérico
                break;
        }
        // Crear formulario multipart
        WWWForm form = new WWWForm();
        form.AddField("nombre", nombreMapa); // Campo texto
        form.AddBinaryData("file", imagenBytes, Path.GetFileName(rutaImagen), mimeType);

        // Enviar POST
        UnityWebRequest request = UnityWebRequest.Post(apiUrl, form);

        // Enviar y esperar respuesta
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al subir el mapa: " + request.error);
        }
        else
        {
            Debug.Log("Mapa subido correctamente: " + request.downloadHandler.text);
            yield return StartCoroutine(listadoMapasController.ObtenerListadoMapas());
            panelNombreMapa.SetActive(false);

        }
    }
}
