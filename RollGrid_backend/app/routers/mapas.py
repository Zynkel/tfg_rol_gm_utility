from fastapi import APIRouter, File, UploadFile, HTTPException, Form
from fastapi.responses import StreamingResponse
import tempfile
import os
from bson import ObjectId
from typing import List

from app.services.procesador_mapa import analizar_objetos
from app.services.grid_utils import extraer_celdas_desde_imagen
from app.db.mongo_gridfs_backend import (
    guardar_mapa_con_datos,
    obtener_imagen_binaria,
    recuperar_imagen_mapa,
    actualizar_mapa,
    get_mongo_db,
)
from app.models.mapa import EstadoMapa, ObjetoEnMapa, Mapa

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
        celdas = extraer_celdas_desde_imagen(ruta_temporal)
        mapa_id = guardar_mapa_con_datos(nombre, contenido, objetos_detectados, celdas)

        return {
            "mapa_id": str(mapa_id),
            "mensaje": "✅ Mapa creado y analizado correctamente"
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
        "celdas": mapa["celdas"],
        "estados": mapa["estados"]
    }


@router.get("/mapas/{mapa_id}/imagen")
def obtener_imagen_mapa(mapa_id: str):
    binario = obtener_imagen_binaria(mapa_id)
    if not binario:
        raise HTTPException(status_code=404, detail="Imagen no encontrada")
    return StreamingResponse(content=binario, media_type="image/jpeg")


@router.get("/mapas/{mapa_id}")
def obtener_mapa_completo(mapa_id: str):
    db = get_mongo_db()
    mapa = db.mapas.find_one({"_id": ObjectId(mapa_id)})
    if not mapa:
        raise HTTPException(status_code=404, detail="Mapa no encontrado")

    imagen_binaria = obtener_imagen_binaria(mapa_id)
    if not imagen_binaria:
        raise HTTPException(status_code=404, detail="Imagen no encontrada")

    import base64
    imagen_base64 = base64.b64encode(imagen_binaria).decode("utf-8")

    return {
        "id": str(mapa["_id"]),
        "nombre": mapa["nombre"],
        "imagen_base64": imagen_base64,
        "celdas": mapa["celdas"],
        "estados": mapa["estados"]
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

    return {"mensaje": "🛠️ Mapa actualizado correctamente"}


@router.post("/mapas/{mapa_id}/estados")
def agregar_estado(mapa_id: str, estado: EstadoMapa):
    db = get_mongo_db()
    resultado = db.mapas.update_one(
        {"_id": ObjectId(mapa_id)},
        {"$push": {"estados": estado.dict(by_alias=True)}}
    )
    if resultado.modified_count == 0:
        raise HTTPException(status_code=404, detail="Mapa no encontrado")
    return {"mensaje": "🗺️ Estado agregado correctamente"}


@router.get("/mapas/{mapa_id}/estados")
def listar_estados(mapa_id: str):
    db = get_mongo_db()
    mapa = db.mapas.find_one({"_id": ObjectId(mapa_id)}, {"estados": 1})
    if not mapa:
        raise HTTPException(status_code=404, detail="Mapa no encontrado")
    return mapa["estados"]


@router.get("/mapas/{mapa_id}/estados/{estado_id}")
def obtener_estado_individual(mapa_id: str, estado_id: int):
    db = get_mongo_db()
    mapa = db.mapas.find_one({"_id": ObjectId(mapa_id)}, {"estados": 1})
    if not mapa or estado_id < 0 or estado_id >= len(mapa["estados"]):
        raise HTTPException(status_code=404, detail="Estado no encontrado")
    return mapa["estados"][estado_id]


@router.delete("/mapas/{mapa_id}")
def eliminar_mapa(mapa_id: str):
    db = get_mongo_db()
    eliminado = db.mapas.delete_one({"_id": ObjectId(mapa_id)})
    if eliminado.deleted_count == 0:
        raise HTTPException(status_code=404, detail="Mapa no encontrado")
    return {"mensaje": "🗑️ Mapa eliminado correctamente"}
