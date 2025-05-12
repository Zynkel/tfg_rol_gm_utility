from fastapi import FastAPI, File, UploadFile, HTTPException
from fastapi.responses import JSONResponse
from fastapi.middleware.cors import CORSMiddleware
from pymongo import MongoClient
import gridfs
import os
import json
import shutil
import base64

from app.utils.grid_detector import detectar_cuadricula

# --- Configuración base ---
app = FastAPI()
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
TEMP_DIR = os.path.join(BASE_DIR, "temp")
SALIDA_DIR = os.path.join(BASE_DIR, "salida")

os.makedirs(TEMP_DIR, exist_ok=True)
os.makedirs(SALIDA_DIR, exist_ok=True)

# --- Conexión a MongoDB ---
client = MongoClient("mongodb://localhost:27017/")
db = client["rollgrid_db"]
fs = gridfs.GridFS(db)

# --- Middleware CORS (si lo usas desde Unity o frontend externo) ---
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# --- POST /upload ---
@app.post("/upload")
async def subir_imagen(archivo: UploadFile = File(...)):
    try:
        # Guardar archivo temporalmente
        ruta_temporal = os.path.join(TEMP_DIR, archivo.filename)
        with open(ruta_temporal, "wb") as buffer:
            shutil.copyfileobj(archivo.file, buffer)

        # Guardar en MongoDB
        with open(ruta_temporal, "rb") as f:
            imagen_id = fs.put(f, filename=archivo.filename, content_type=archivo.content_type)

        # Detectar cuadrícula
        resultado = detectar_cuadricula(ruta_temporal, SALIDA_DIR)

        # Eliminar archivo temporal
        os.remove(ruta_temporal)

        # Armar JSON de metadatos
        datos_json = {
            "cuadricula": resultado if resultado else None
        }

        db["imagenes_json"].insert_one({
            "nombre": archivo.filename,
            "imagen_id": imagen_id,
            "datos": datos_json
        })

        if resultado:
            return JSONResponse(status_code=200, content={"mensaje": "Imagen guardada y analizada correctamente.", "cuadricula": resultado})
        else:
            return JSONResponse(status_code=202, content={"mensaje": "Imagen guardada, pero no se detectó cuadrícula.", "cuadricula": None})

    except Exception as e:
        import traceback
        traceback.print_exc()
        return JSONResponse(status_code=500, content={"error": str(e)})

# --- GET /imagenes ---
@app.get("/imagenes")
def listar_imagenes():
    nombres = db["imagenes_json"].distinct("nombre")
    return {"imagenes": nombres}

# --- GET /imagen/{nombre} ---
@app.get("/imagen/{nombre}")
def obtener_imagen_y_datos(nombre: str):
    entrada = db["imagenes_json"].find_one({"nombre": nombre})
    if not entrada:
        raise HTTPException(status_code=404, detail="Imagen no encontrada.")

    imagen_id = entrada["imagen_id"]
    datos = entrada.get("datos", {})

    try:
        binario = fs.get(imagen_id).read()
        imagen_base64 = base64.b64encode(binario).decode("utf-8")
        return {
            "nombre": nombre,
            "datos": datos,
            "imagen_base64": imagen_base64
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail="Error al recuperar la imagen.")

# --- GET /datos/{nombre} ---
@app.get("/datos/{nombre}")
def obtener_datos(nombre: str):
    entrada = db["imagenes_json"].find_one({"nombre": nombre})
    if not entrada:
        raise HTTPException(status_code=404, detail="Imagen no encontrada.")

    return entrada.get("datos", {})
