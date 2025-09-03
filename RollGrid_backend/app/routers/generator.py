from fastapi import APIRouter, HTTPException, Request
from pydantic import BaseModel
from typing import Optional
import base64
import uuid
import os

from app.services.image_generator import generate_map_image
from app.services.hybrid_generator import generate_hybrid_map
from app.db.mongo_gridfs_backend import guardar_mapa_con_datos

router = APIRouter(prefix="/maps", tags=["Generador"])

class GenerateImageRequest(BaseModel):
    prompt: str
    tipo: Optional[str] = "auto"
    estilo: Optional[str] = "default"
    modo: Optional[str] = "text2img"
    save: Optional[bool] = False

class GenerateImageResponse(BaseModel):
    image_base64: Optional[str] = None
    image_url: Optional[str] = None
    guide_url: Optional[str] = None
    prompt_final: str
    layout: Optional[dict] = None

@router.post("/generate-image", response_model=GenerateImageResponse)
async def generate_image(request: Request, body: GenerateImageRequest):
    try:
        layout = None
        image_b64 = None
        prompt_final = None
        guide_url = None

        if body.modo == "hybrid":
            layout, image_b64, prompt_final, guide_url = await generate_hybrid_map(
                prompt_es=body.prompt,
                tipo=body.tipo,
                estilo=body.estilo
            )
        else:
            image_b64, prompt_final = await generate_map_image(
                prompt_es=body.prompt,
                tipo=body.tipo,
                estilo=body.estilo
            )

        image_url = None
        if body.save:
            os.makedirs("static/generated", exist_ok=True)
            filename = f"{uuid.uuid4().hex}.png"
            filepath = os.path.join("static", "generated", filename)
            with open(filepath, "wb") as f:
                f.write(base64.b64decode(image_b64))
            base = str(request.base_url).rstrip("/")
            image_url = f"{base}/static/generated/{filename}"

        return GenerateImageResponse(
            image_base64=None if body.save else image_b64,
            image_url=image_url,
            guide_url=guide_url,
            prompt_final=prompt_final,
            layout=layout
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error generando el mapa: {str(e)}")

class SaveGeneratedRequest(BaseModel):
    nombre: str
    image_base64: str
    layout: Optional[dict] = None

class SaveGeneratedResponse(BaseModel):
    mapa_id: str
    mensaje: str

@router.post("/confirm-save", response_model=SaveGeneratedResponse)
async def confirm_save(body: SaveGeneratedRequest):
    try:
        imagen_bytes = base64.b64decode(body.image_base64)
        mapa_id = guardar_mapa_con_datos(
            nombre=body.nombre,
            imagen_bytes=imagen_bytes,
            grid=body.layout or {},
            objetos=[],
            personajes=[]
        )
        return SaveGeneratedResponse(mapa_id=str(mapa_id), mensaje="Mapa guardado correctamente")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"No se pudo guardar el mapa: {str(e)}")
