using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MarcaFilaUI : MonoBehaviour
{
    public Image icono;
    public TextMeshProUGUI texto;
    public Button botonEditar;
    public Button botonEliminar;
    [HideInInspector] public NavegadorController navegadorController;
    public MarcaUI marcaUIAsociada;

    [SerializeField] private GameObject marcaAsociada;
    private bool estaSeleccionada = false;
    private Color colorSeleccionado = new Color(0.3f, 0.3f, 0.3f);// Seleccionado azulado
    private Color colorNormal = new Color(0.8f, 0.9f, 0f, 0f); //Normal transparente
    private DetalleMarcaController detalleController;

    public void Configurar(Sprite sprite, string nombre, GameObject marca, DetalleMarcaController detalles, NavegadorController navController, MarcaUI marcaUI)
    {
        icono.sprite = sprite;
        texto.text = nombre;
        marcaAsociada = marca;
        marcaUIAsociada = marcaUI;
        detalleController = detalles;
        navegadorController = navController;

        marcaAsociada.AddComponent<MarcaFilaUI>();

        botonEliminar.onClick.AddListener(EliminarMarca);
        botonEditar.onClick.AddListener(() => detalleController.MostrarDetalles(marca));

        Deseleccionar();
    }   

    public void Seleccionar()
    {
        estaSeleccionada = true;
        GetComponent<Image>().color = colorSeleccionado;

        if (marcaAsociada != null)
        {
            // Ejemplo: resaltar marca aumentando su tama�o o cambiando color
            marcaAsociada.transform.localScale = Vector3.one * 1.3f;
        }
    }

    public void Deseleccionar()
    {
        estaSeleccionada = false;
        GetComponent<Image>().color = colorNormal;

        if (marcaAsociada != null)
        {
            marcaAsociada.transform.localScale = Vector3.one;
        }
    }

    public void AlClickear()
    {
        navegadorController.FilaSeleccionada(this);
    }

    public bool isSeleccionada()
    {
        return estaSeleccionada;
    }

    public void EliminarMarca()
    {
        // Limpia el objeto seleccionado si es este mismo
        if (EventSystem.current.currentSelectedGameObject == this.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // Limpia tambi�n cualquier referencia de puntero activa
        PointerEventData pointer = new PointerEventData(EventSystem.current)
        {
            pointerEnter = null
        };

        // Opcional: desactiva el raycast antes de destruir (por seguridad)
        Graphic graphic = GetComponent<Graphic>();
        if (graphic != null)
        {
            graphic.raycastTarget = false;
        }

        // Destruye primero la marca asociada si existe
        if (marcaAsociada != null)
        {
            Destroy(marcaAsociada);
            marcaAsociada = null;
        }

        // Luego destruye esta fila del navegador
        this.gameObject.SetActive(false);

        // Destruir desde un objeto activo
        SafeDestroyer.Instance.DestroySafely(this.gameObject);
    }
}