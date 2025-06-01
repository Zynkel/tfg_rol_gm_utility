using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MarcasManager : MonoBehaviour
{
    public Sprite iconoEnemigo;
    public Sprite iconoTesoro;
    public Sprite iconoPuerta;

    public List<TipoMarca> tiposDisponibles;

    private Dictionary<TipoMarca, Sprite> iconosPorTipo;

    

    void Awake()
    {
        iconosPorTipo = new Dictionary<TipoMarca, Sprite>
        {
            { TipoMarca.Enemigo, iconoEnemigo },
            { TipoMarca.Tesoro, iconoTesoro },
            { TipoMarca.Puerta, iconoPuerta }
        };
    }

    public Sprite GetIconoPorTipo(TipoMarca tipo)
    {
        return iconosPorTipo[tipo];
    }

    public List<TMP_Dropdown.OptionData> ObtenerOpcionesTipoMarca()
    {
        List<TMP_Dropdown.OptionData> opcionesTipo = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData(TipoMarca.Enemigo.ToString(), GetIconoPorTipo(TipoMarca.Enemigo), Color.white),
            new TMP_Dropdown.OptionData(TipoMarca.Tesoro.ToString(), GetIconoPorTipo(TipoMarca.Tesoro), Color.white),
            new TMP_Dropdown.OptionData(TipoMarca.Puerta.ToString(), GetIconoPorTipo(TipoMarca.Puerta), Color.white)
        };

        return opcionesTipo;
    }

    public List<TMP_Dropdown.OptionData> ObtenerOpcionesEstadoMarca()
    {
        List<TMP_Dropdown.OptionData> opcionesEstado = new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData(EstadoMarca.Activo.ToString()),
            new TMP_Dropdown.OptionData(EstadoMarca.Inactivo.ToString()),
            new TMP_Dropdown.OptionData(EstadoMarca.Oculto.ToString())
        };

        return opcionesEstado;
    }
}

