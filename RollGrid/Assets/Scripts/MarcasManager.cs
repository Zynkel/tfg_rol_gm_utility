using System.Collections.Generic;
using UnityEngine;

public class MarcasManager : MonoBehaviour
{
    public Sprite iconoEnemigo;
    public Sprite iconoTesoro;
    public Sprite iconoPuerta;

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
}

