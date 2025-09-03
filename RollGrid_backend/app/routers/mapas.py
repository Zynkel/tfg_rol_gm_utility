from fastapi import APIRouter, File, UploadFile, HTTPException, Form
from fastapi.responses import StreamingResponse
from fastapi.encoders import jsonable_encoder
import tempfile
import os
from bson import ObjectId
from io import BytesIO
import base64

from app.models.mapa import EstadoMapa
from app.services.map_processor import analizar_objetos
from app.services.grid_utils import extraer_info_cuadricula
from app.db.mongo_gridfs_backend import (
    guardar_mapa_con_datos,
    obtener_imagen_binaria,
    actualizar_mapa,
    get_mongo_db,
)

router = APIRouter()


@router.post("/mapas")
async def crear_mapa(nombre: str = Form(...), file: UploadFile = File(...)):
    if not file.filename.lower().endswith((".jpg", ".jpeg", ".png", ".webp")):
        raise HTTPException(status_code=400, detail="Formato de imagen no soportado")

    contenido = await file.read()

    with tempfile.NamedTemporaryFile(delete=False, suffix=".jpg") as tmp:
        tmp.write(contenido)
        ruta_temporal = tmp.name

    try:
        objetos_detectados = analizar_objetos(ruta_temporal)
        grid_info = extraer_info_cuadricula(ruta_temporal)

        if not grid_info:
            raise HTTPException(status_code=400, detail="No se pudo detectar la cuadrícula en la imagen.")

        mapa_id = guardar_mapa_con_datos(
            nombre=nombre,
            imagen_bytes=contenido,
            grid=grid_info,
            objetos=objetos_detectados,
            personajes=[]
        )

        return {
            "mapa_id": str(mapa_id),
            "mensaje": "Mapa creado y analizado correctamente"
        }

    finally:
        os.remove(ruta_temporal)


@router.get("/mapas")
def listar_mapas():
    db = get_mongo_db()
    mapas = db.mapas.find({}, {"_id": 1, "nombre": 1})
    return [{"id": str(m["_id"]), "nombre": m["nombre"]} for m in mapas]


@router.get("/mapas/{mapa_id}/datos")
def obtener_datos_mapa(mapa_id: str):
    db = get_mongo_db()
    mapa = db.mapas.find_one({"_id": ObjectId(mapa_id)})
    if not mapa:
        raise HTTPException(status_code=404, detail="Mapa no encontrado")
    return {
        "nombre": mapa["nombre"],
        "grid": mapa.get("grid", {}),
        "estados": mapa.get("estados", [])
    }


@router.get("/mapas/{mapa_id}/imagen")
def obtener_imagen_mapa(mapa_id: str):
    binario = obtener_imagen_binaria(mapa_id)
    if not binario:
        raise HTTPException(status_code=404, detail="Imagen no encontrada")
    return StreamingResponse(content=BytesIO(binario), media_type="image/jpeg")


@router.get("/mapas/{mapa_id}")
def obtener_mapa_completo(mapa_id: str):
    db = get_mongo_db()
    mapa = db.mapas.find_one({"_id": ObjectId(mapa_id)})
    if not mapa:
        raise HTTPException(status_code=404, detail="Mapa no encontrado")

    imagen_binaria = obtener_imagen_binaria(mapa_id)
    if not imagen_binaria:
        raise HTTPException(status_code=404, detail="Imagen no encontrada")

    imagen_base64 = base64.b64encode(imagen_binaria).decode("utf-8")

    return {
        "id": str(mapa["_id"]),
        "nombre": mapa["nombre"],
        "imagen_base64": imagen_base64,
        "grid": mapa.get("grid", {}),
        "estados": mapa.get("estados", [])
    }


@router.put("/mapas/{mapa_id}")
async def actualizar_datos_mapa(
    mapa_id: str,
    nuevo_nombre: str = Form(None),
    nueva_imagen: UploadFile = File(None)
):
    nueva_imagen_bytes = await nueva_imagen.read() if nueva_imagen else None
    actualizado = actualizar_mapa(mapa_id, nuevo_nombre, nueva_imagen_bytes)

    if not actualizado:
        raise HTTPException(status_code=400, detail="No se pudo actualizar el mapa")

    return {"mensaje": "Mapa actualizado correctamente"}


@router.post("/mapas/{mapa_id}/estados")
def agregar_estado(mapa_id: str, estado: EstadoMapa):
    db = get_mongo_db()
    resultado = db.mapas.update_one(
        {"_id": ObjectId(mapa_id)},
        {"$push": {"estados": estado.dict(by_alias=True)}}
    )
    if resultado.modified_count == 0:
        raise HTTPException(status_code=404, detail="Mapa no encontrado")
    return {"mensaje": "Estado agregado correctamente"}


@router.get("/mapas/{mapa_id}/estados")
def listar_estados(mapa_id: str):
    db = get_mongo_db()
    mapa = db.mapas.find_one({"_id": ObjectId(mapa_id)}, {"estados": 1})
    if not mapa:
        raise HTTPException(status_code=404, detail="Mapa no encontrado")
    return mapa["estados"]


@router.get("/mapas/{mapa_id}/estados/{estado_id}")
def obtener_estado_individual(mapa_id: str, estado_id: str):
    db = get_mongo_db()
    mapa = db.mapas.find_one({"_id": ObjectId(mapa_id)}, {"estados": 1})
    if not mapa:
        raise HTTPException(status_code=404, detail="Mapa no encontrado")

    for estado in mapa.get("estados", []):
        if estado.get("id") == estado_id:
            return jsonable_encoder(estado)

    raise HTTPException(status_code=404, detail="Estado no encontrado")


@router.delete("/mapas/{mapa_id}")
def eliminar_mapa(mapa_id: str):
    db = get_mongo_db()
    eliminado = db.mapas.delete_one({"_id": ObjectId(mapa_id)})
    if eliminado.deleted_count == 0:
        raise HTTPException(status_code=404, detail="Mapa no encontrado")
    return {"mensaje": "Mapa eliminado correctamente"}
