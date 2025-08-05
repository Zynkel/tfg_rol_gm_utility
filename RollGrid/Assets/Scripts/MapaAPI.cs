using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapaAPI
{
    public string id;
    public string nombre;
    public string imagen_base64;
    public GridInfo grid;
    public List<EstadoMapa> estados;
}

[Serializable]
public class GridInfo
{
    public int rows;
    public int cols;
    public float cellWidth;
    public float cellHeight;
    public float offsetX;
    public float offsetY;
}

[Serializable]
public class EstadoMapa
{
    public string id;
    public string nombre;
    public string descripcion;
    public string fecha_creacion;
    public List<ObjetoMapa> objetos;
    public List<PersonajeMapa> personajes;
}

[Serializable]
public class ObjetoMapa
{
    public string tipo;
    public string descripcion;
    public List<ContenidoObjeto> contenido;
    public EstadoObjeto estado;
    public Posicion posicion;
    public BoundingBox bounding_box;
    public string destino_mapa_id;
    public string destino_estado_id;
}

[Serializable]
public class Posicion
{
    public int x;
    public int y;
}

[Serializable]
public class BoundingBox
{
    public int x;
    public int y;
    public int width;
    public int height;
}

[Serializable]
public class EstadoObjeto
{
    public bool visible;
    public bool bloqueado;
}

[Serializable]
public class ContenidoObjeto
{
    public string tipo;
    public string descripcion;
}

[Serializable]
public class PersonajeMapa
{
    public string id;
    public string nombre;
    public Posicion posicion;
}
