from typing import Optional, List, Dict
from pydantic import BaseModel, Field
from uuid import uuid4

class PersonajeEnMapa(BaseModel):
    id: str = Field(default_factory=lambda: str(uuid4()))
    tipo: str 
    nombre: Optional[str] = None
    descripcion: Optional[str] = None
    estado: Optional[str] = None  
    posicion: dict 
    objetos: Optional[List[str]] = []
    stats: Optional[Dict[str, int]] = {}  
    notas: Optional[str] = None  
    destino_mapa_id: Optional[str] = None
    destino_estado_id: Optional[str] = None
