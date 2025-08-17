using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MarcasManager : MonoBehaviour
{
    public Sprite iconoEnemigo;
    public Sprite iconoTesoro;
    public Sprite iconoPuerta;
    public Sprite iconoObjeto;

    public List<TipoMarca> tiposDisponibles;

    private Dictionary<TipoMarca, Sprite> iconosPorTipo;

    

    void Awake()
    {
        iconosPorTipo = new Dictionary<TipoMarca, Sprite>
        {
            { TipoMarca.objeto, iconoObjeto },
            { TipoMarca.enemigo, iconoEnemigo },
            { TipoMarca.tesoro, iconoTesoro },
            { TipoMarca.puerta, iconoPuerta }            
        };
    }

    public Sprite GetIconoPorTipo(TipoMarca tipo)
    {
        return iconosPorTipo[tipo];
    }

    public Sprite GetIconoPorNombreTipo(string tipoNombre)
    {
        if (Enum.TryParse<TipoMarca>(tipoNombre.ToLower(), out TipoMarca tipo))
        {
            if (iconosPorTipo.TryGetValue(tipo, out Sprite sprite))
                return sprite;
            else
                Debug.LogWarning($"No hay sprite asignado al tipo '{tipo}'");
        }
        else
        {
            Debug.LogWarning($"Tipo de marca desconocido: '{tipoNombre}'");
        }

        return iconoObjeto; // sprite genérico (cambiar por un signo de ? u otra cosa)
    }

    public List<TMP_Dropdown.OptionData> ObtenerOpcionesTipoMarca()
    {
        List<TMP_Dropdown.OptionData> opcionesTipo = new List<TMP_Dropdown.OptionData>
        {

            new TMP_Dropdown.OptionData(TipoMarca.objeto.ToString(), GetIconoPorTipo(TipoMarca.objeto), Color.white),
            new TMP_Dropdown.OptionData(TipoMarca.enemigo.ToString(), GetIconoPorTipo(TipoMarca.enemigo), Color.white),
            new TMP_Dropdown.OptionData(TipoMarca.tesoro.ToString(), GetIconoPorTipo(TipoMarca.tesoro), Color.white),
            new TMP_Dropdown.OptionData(TipoMarca.puerta.ToString(), GetIconoPorTipo(TipoMarca.puerta), Color.white)
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

