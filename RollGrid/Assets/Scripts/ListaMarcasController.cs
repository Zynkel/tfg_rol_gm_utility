using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ListaMarcasUI : MonoBehaviour
{
    [System.Serializable]
    public class Marca
    {
        public string nombre;
        public Sprite icono;

        public Marca(string nombre, Sprite icono)
        {
            this.nombre = nombre;
            this.icono = icono;
        }
    }

    public Transform contenedor;
    public GameObject prefabMarca;

    public Sprite iconoEnemigo;
    public Sprite iconoTesoro;
    public Sprite iconoPuerta;

    private MarcaItemUI marcaSeleccionada;

    void Start()
    {
        // Crear la lista de marcas
        Marca[] marcas = new Marca[]
        {
            new Marca("Enemigo", iconoEnemigo),
            new Marca("Tesoro", iconoTesoro),
            new Marca("Puerta", iconoPuerta),
        };

        // Por cada marca se crea el objeto MarcaItemUI para asignar texto e imagen.
        foreach (Marca marca in marcas)
        {
            CrearItem(marca);
        }
    }

    void CrearItem(Marca marca)
    {
        GameObject item = Instantiate(prefabMarca, contenedor);
        MarcaItemUI ui = item.GetComponent<MarcaItemUI>();
        ui.Configurar(marca.nombre, marca.icono);
        item.GetComponent<Button>().onClick.AddListener(() => SeleccionarMarca(ui));
    }

    void SeleccionarMarca(MarcaItemUI marca)
    {
        Debug.Log("Recibo evento y seleccionarMarca: " + marcaSeleccionada);
        if (marcaSeleccionada != null)
            marcaSeleccionada.Desmarcar();

        marcaSeleccionada = marca;
        marcaSeleccionada.Seleccionar();
    }
}