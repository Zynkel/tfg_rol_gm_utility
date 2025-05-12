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

app = FastAPI()

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
TEMP_DIR = os.path.join(BASE_DIR, "temp")
SALIDA_DIR = os.path.join(BASE_DIR, "salida")

os.makedirs(TEMP_DIR, exist_ok=True)
os.makedirs(SALIDA_DIR, exist_ok=True)

client = MongoClient("mongodb://localhost:27017/")
db = client["rollgrid_db"]
fs = gridfs.GridFS(db)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.post("/upload")
async def upload_image(file: UploadFile = File(...)):
    try:
        temp_path = os.path.join(TEMP_DIR, file.filename)
        with open(temp_path, "wb") as buffer:
            shutil.copyfileobj(file.file, buffer)

        with open(temp_path, "rb") as f:
            image_id = fs.put(f, filename=file.filename, content_type=file.content_type)

        result = detectar_cuadricula(temp_path, SALIDA_DIR)

        os.remove(temp_path)

        json_data = {"grid": result} if result else {}

        db["imagenes_json"].insert_one({
            "name": file.filename,
            "image_id": image_id,
            "data": json_data
        })

        if result:
            return JSONResponse(status_code=200, content={"message": "Image saved and grid detected.", "grid": result})
        else:
            return JSONResponse(status_code=202, content={"message": "Image saved, but grid not detected.", "grid": None})

    except Exception as e:
        import traceback
        traceback.print_exc()
        return JSONResponse(status_code=500, content={"error": str(e)})

@app.get("/images")
def list_images():
    names = db["imagenes_json"].distinct("name")
    return {"images": names}

@app.get("/image/{name}")
def get_image_and_data(name: str):
    entry = db["imagenes_json"].find_one({"name": name})
    if not entry:
        raise HTTPException(status_code=404, detail="Image not found.")

    image_id = entry["image_id"]
    data = entry.get("data", {})

    try:
        binary = fs.get(image_id).read()
        image_base64 = base64.b64encode(binary).decode("utf-8")
        return {
            "name": name,
            "data": data,
            "image_base64": image_base64
        }
    except Exception:
        raise HTTPException(status_code=500, detail="Error retrieving image.")

@app.get("/data/{name}")
def get_data(name: str):
    entry = db["imagenes_json"].find_one({"name": name})
    if not entry:
        raise HTTPException(status_code=404, detail="Image not found.")

    return entry.get("data", {})

@app.delete("/image/{name}")
def delete_image(name: str):
    found = False

    entry = db["imagenes_json"].find_one({"name": name})
    if entry:
        image_id = entry.get("image_id")
        if image_id:
            try:
                fs.delete(image_id)
                found = True
            except Exception:
                pass
        db["imagenes_json"].delete_one({"name": name})
        found = True

    if found:
        return {"message": f"'{name}' deleted if it existed."}
    else:
        return JSONResponse(status_code=404, content={"message": f"'{name}' not found in database."})