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
    [HideInInspector] public NavegadorManager manager;

    [SerializeField] private GameObject marcaAsociada;
    private bool estaSeleccionada = false;
    private Color colorSeleccionado = new Color(0.3f, 0.3f, 0.3f);// Seleccionado azulado
    private Color colorNormal = new Color(0.8f, 0.9f, 0f, 0f); //Normal transparente

    public void Configurar(Sprite sprite, string nombre, GameObject marca, NavegadorManager Navmanager)
    {
        icono.sprite = sprite;
        texto.text = nombre;
        marcaAsociada = marca;
        manager = Navmanager;

        botonEliminar.onClick.AddListener(EliminarMarca);
        botonEditar.onClick.AddListener(EditarMarca);

        Deseleccionar();
    }   

    public void Seleccionar()
    {
        estaSeleccionada = true;
        GetComponent<Image>().color = colorSeleccionado;

        if (marcaAsociada != null)
        {
            // Ejemplo: resaltar marca aumentando su tamaño o cambiando color
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
        manager.FilaSeleccionada(this);
    }

    private void EliminarMarca()
    {
        // Limpia el objeto seleccionado si es este mismo
        if (EventSystem.current.currentSelectedGameObject == this.gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // Limpia también cualquier referencia de puntero activa
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

    private IEnumerator DestruirEnSiguienteFrame()
    {
        yield return null; // espera 1 frame
        Destroy(this.gameObject);
    }

    private void EditarMarca()
    {
        Debug.Log("Editar marca: " + texto.text);
        // Aquí puedes abrir un panel de edición en el futuro
    }
}