from fastapi import FastAPI, UploadFile, File
from fastapi.responses import JSONResponse
from fastapi.staticfiles import StaticFiles
from pydantic import BaseModel
from datetime import datetime
from bson import ObjectId
import motor.motor_asyncio
import os
import shutil
import uuid

app = FastAPI()

# Conexión a MongoDB
client = motor.motor_asyncio.AsyncIOMotorClient("mongodb://localhost:27017")
db = client.map_data
collection = db.maps

# Carpeta para guardar imágenes y JSONs
DATA_DIR = "data/maps"
os.makedirs(DATA_DIR, exist_ok=True)

# Modelo de documento para MongoDB
class MapEntry(BaseModel):
    name: str
    image_path: str
    json_data: dict
    created_at: datetime

# Montar carpeta estática para servir imágenes
app.mount("/static", StaticFiles(directory=DATA_DIR), name="static")

# Endpoint para subir imagen y analizar
@app.post("/upload")
async def upload_map(file: UploadFile = File(...)):
    # Guardar imagen con nombre único
    map_id = str(uuid.uuid4())
    folder_path = os.path.join(DATA_DIR, map_id)
    os.makedirs(folder_path, exist_ok=True)

    image_path = os.path.join(folder_path, file.filename)
    with open(image_path, "wb") as buffer:
        shutil.copyfileobj(file.file, buffer)

    # Simulación de análisis de imagen
    simulated_json = {
        "grid": [[0, 0], [1, 0], [1, 1]],
        "elements": [
            {"type": "casa", "position": [2, 3]},
            {"type": "puerta", "position": [4, 1]},
            {"type": "cofre", "position": [5, 5]}
        ]
    }

    # Guardar en MongoDB
    document = {
        "name": file.filename,
        "image_path": f"/static/{map_id}/{file.filename}",
        "json_data": simulated_json,
        "created_at": datetime.utcnow()
    }
    result = await collection.insert_one(document)

    return JSONResponse({
        "id": str(result.inserted_id),
        "image_url": document["image_path"],
        "json_data": simulated_json
    })

# Endpoint para recuperar JSON por ID
@app.get("/map/{map_id}")
async def get_map(map_id: str):
    doc = await collection.find_one({"_id": ObjectId(map_id)})
    if not doc:
        return JSONResponse(status_code=404, content={"message": "Mapa no encontrado"})
    return {
        "name": doc["name"],
        "image_url": doc["image_path"],
        "json_data": doc["json_data"],
        "created_at": doc["created_at"]
    }
