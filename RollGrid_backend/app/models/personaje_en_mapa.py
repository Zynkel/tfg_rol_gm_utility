from typing import Optional, List, Dict
from pydantic import BaseModel, Field
from uuid import uuid4

class PersonajeEnMapa(BaseModel):
    id: str = Field(default_factory=lambda: str(uuid4()))
    tipo: str  # "npc" o "enemigo"
    nombre: Optional[str] = None
    descripcion: Optional[str] = None
    estado: Optional[str] = None  # ej: "hostil", "neutral"
    posicion: dict  # en píxeles
    objetos: Optional[List[str]] = []  # ej: ["espada", "llave"]
    stats: Optional[Dict[str, int]] = {}  # campo libre: fuerza, vida, etc.
    notas: Optional[str] = None  # notas narrativas opcionales
    destino_mapa_id: Optional[str] = None
    destino_estado_id: Optional[str] = None
