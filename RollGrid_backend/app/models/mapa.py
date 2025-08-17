from typing import List, Optional
from pydantic import BaseModel, Field
from uuid import uuid4

from app.models.personaje_en_mapa import PersonajeEnMapa

class ObjetoEnMapa(BaseModel):
    tipo: str
    descripcion: Optional[str] = None
    contenido: Optional[List[str]] = None  
    estado: Optional[str] = None  
    posicion: dict  
    bounding_box: Optional[dict] = None  
    destino_mapa_id: Optional[str] = None 
    destino_estado_id: Optional[str] = None
    visible: bool = True

class EstadoMapa(BaseModel):
    id: str = Field(default_factory=lambda: str(uuid4()))
    nombre: str
    descripcion: Optional[str] = None
    fecha_creacion: Optional[str] = None
    objetos: List[ObjetoEnMapa] = []
    personajes: List[PersonajeEnMapa] = []


class Mapa(BaseModel):
    id: Optional[str] = Field(default_factory=str)
    nombre: str
    fecha_subida: Optional[str] = None
    imagen_id: Optional[str] = None
    grid: Optional[dict] = None
    estados: List[EstadoMapa] = []
