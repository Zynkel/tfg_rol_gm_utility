from pymongo import MongoClient
import gridfs
import os
from bson import ObjectId
from app.models.mapa import EstadoMapa, ObjetoEnMapa

client = MongoClient("mongodb://localhost:27017/")
db = client["rollgrid_db"]
fs = gridfs.GridFS(db)

def get_mongo_db():
    return db

def get_mongo_fs():
    return fs

def guardar_imagen_en_gridfs(file):
    contenido = file.file.read()
    imagen_id = fs.put(contenido, filename=file.filename, content_type=file.content_type)
    return imagen_id

def guardar_mapa_con_datos(nombre: str, imagen_bytes: bytes, objetos: list, celdas: list):
    imagen_id = fs.put(imagen_bytes, filename=nombre, content_type="image/jpeg")

    estado_inicial = EstadoMapa(
        nombre="Estado inicial",
        objetos=[ObjetoEnMapa(**obj) for obj in objetos]
    )

    mapa_doc = {
        "nombre": nombre,
        "imagen_id": imagen_id,
        "celdas": celdas,
        "estados": [estado_inicial.dict(by_alias=True)]
    }

    resultado = db.mapas.insert_one(mapa_doc)
    return resultado.inserted_id

def recuperar_imagen_mapa(mapa_id: str, ruta_salida: str):
    entrada = db.mapas.find_one({"_id": ObjectId(mapa_id)})
    if not entrada:
        return None

    imagen_binaria = fs.get(entrada["imagen_id"]).read()

    os.makedirs(os.path.dirname(ruta_salida), exist_ok=True)
    with open(ruta_salida, "wb") as f:
        f.write(imagen_binaria)

    return ruta_salida

def obtener_imagen_binaria(mapa_id: str) -> bytes:
    entrada = db.mapas.find_one({"_id": ObjectId(mapa_id)})
    if not entrada:
        return None

    imagen_binaria = fs.get(entrada["imagen_id"]).read()
    return imagen_binaria

def actualizar_mapa(mapa_id: str, nuevo_nombre: str = None, nueva_imagen: bytes = None):
    update_fields = {}

    if nuevo_nombre:
        update_fields["nombre"] = nuevo_nombre

    if nueva_imagen:
        nueva_imagen_id = fs.put(nueva_imagen, filename=f"updated_{mapa_id}.jpg", content_type="image/jpeg")
        update_fields["imagen_id"] = nueva_imagen_id

    if not update_fields:
        return False

    resultado = db.mapas.update_one(
        {"_id": ObjectId(mapa_id)},
        {"$set": update_fields}
    )

    return resultado.modified_count > 0
