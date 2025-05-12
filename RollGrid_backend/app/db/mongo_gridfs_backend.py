from pymongo import MongoClient
import gridfs
import os
import json

# Configuración de la base de datos
client = MongoClient("mongodb://localhost:27017/")
db = client["rollgrid_db"]
fs = gridfs.GridFS(db)
coleccion_json = db["imagenes_json"]

def guardar_imagen_y_json(nombre_imagen: str, ruta_imagen: str, datos_json: dict):
    with open(ruta_imagen, "rb") as f:
        imagen_id = fs.put(f, filename=nombre_imagen, content_type="image/jpeg")

    entrada = {
        "nombre": nombre_imagen,
        "imagen_id": imagen_id,
        "datos": datos_json
    }

    coleccion_json.insert_one(entrada)
    print(f"Imagen '{nombre_imagen}' y JSON asociado guardados en MongoDB.")

def recuperar_imagen(nombre: str, ruta_destino: str) -> bool:
    entrada = coleccion_json.find_one({"nombre": nombre})
    if not entrada:
        return False

    imagen_id = entrada["imagen_id"]
    try:
        with open(ruta_destino, "wb") as f:
            f.write(fs.get(imagen_id).read())
        return True
    except Exception as e:
        print(f"Error al recuperar imagen '{nombre}': {e}")
        return False

def obtener_datos_json(nombre: str):
    entrada = coleccion_json.find_one({"nombre": nombre})
    if not entrada:
        return None
    return entrada.get("datos", {})

def listar_nombres_imagenes():
    return list(coleccion_json.distinct("nombre"))
