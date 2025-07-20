from typing import List, Optional
from pydantic import BaseModel, Field
from bson import ObjectId

class ObjetoEnMapa(BaseModel):
    tipo: str
    descripcion: Optional[str] = None
    contenido: Optional[List[str]] = None  
    estado: Optional[str] = None  
    posicion: dict  
    bounding_box: Optional[dict] = None  
    destino_mapa_id: Optional[str] = None 
    destino_estado_id: Optional[str] = None

class EstadoMapa(BaseModel):
    nombre: str
    descripcion: Optional[str] = None
    fecha_creacion: Optional[str] = None
    objetos: List[ObjetoEnMapa]

class Mapa(BaseModel):
    id: Optional[str] = Field(default_factory=str)
    nombre: str
    fecha_subida: Optional[str] = None
    imagen_id: Optional[str] = None  
    estados: List[EstadoMapa] = []
