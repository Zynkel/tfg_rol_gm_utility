from fastapi import FastAPI, File, UploadFile, Form
from fastapi.responses import JSONResponse, FileResponse
from app.db.mongo_gridfs_backend import (
    guardar_imagen_y_json,
    recuperar_imagen,
    obtener_datos_json,
    listar_nombres_imagenes
)
from app.utils.grid_detector import detectar_cuadricula
import os
import shutil
import json
import base64
from pymongo import MongoClient
import gridfs

app = FastAPI()

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
TEMP_DIR = os.path.join(BASE_DIR, "temp")
SALIDA_DIR = os.path.join(BASE_DIR, "salida")
os.makedirs(TEMP_DIR, exist_ok=True)
os.makedirs(SALIDA_DIR, exist_ok=True)

@app.post("/upload")
async def subir_imagen(
    archivo: UploadFile = File(...)
):
    try:
        ruta_temporal = os.path.join(TEMP_DIR, archivo.filename)
        with open(ruta_temporal, "wb") as buffer:
            shutil.copyfileobj(archivo.file, buffer)

        # Ejecutar análisis de cuadrícula
        resultado = detectar_cuadricula(ruta_temporal, SALIDA_DIR)

        datos_json = {"cuadricula": resultado} if resultado else {}

        guardar_imagen_y_json(
            nombre_imagen=archivo.filename,
            ruta_imagen=ruta_temporal,
            datos_json=datos_json
        )

        # Eliminar archivo temporal
        if os.path.exists(ruta_temporal):
            os.remove(ruta_temporal)

        if resultado is None:
            return JSONResponse(status_code=202, content={
                "mensaje": "Imagen guardada. Cuadrícula no detectada.",
                "cuadricula": None
            })

        return {
            "mensaje": "Imagen guardada y cuadrícula detectada correctamente.",
            "cuadricula": resultado
        }

    except Exception as e:
        import traceback
        traceback.print_exc()
        return JSONResponse(status_code=500, content={"error": str(e)})

@app.get("/imagen/{nombre}")
def obtener_imagen_y_json(nombre: str):
    ruta_salida = os.path.join(TEMP_DIR, nombre)
    resultado = recuperar_imagen(nombre, ruta_salida)

    if not resultado or not os.path.exists(ruta_salida):
        return JSONResponse(status_code=404, content={"error": "Imagen no encontrada."})

    with open(ruta_salida, "rb") as f:
        imagen_base64 = base64.b64encode(f.read()).decode("utf-8")

    datos = obtener_datos_json(nombre) or {}

    return {
        "nombre": nombre,
        "datos": datos,
        "imagen_base64": imagen_base64
    }

@app.get("/imagenes")
def listar_imagenes():
    nombres = listar_nombres_imagenes()
    return {"imagenes": nombres}

@app.get("/datos/{nombre}")
def obtener_datos(nombre: str):
    datos = obtener_datos_json(nombre)
    if not datos:
        return JSONResponse(status_code=404, content={"error": "Datos no encontrados para esa imagen."})
    return datos
