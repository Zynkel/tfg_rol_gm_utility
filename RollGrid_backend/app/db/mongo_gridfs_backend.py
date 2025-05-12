from pymongo import MongoClient
import gridfs
import os

# Conexión a MongoDB
client = MongoClient("mongodb://localhost:27017/")
db = client["rollgrid_db"]
fs = gridfs.GridFS(db)

def guardar_imagen_y_json(nombre_imagen, ruta_imagen, datos_json):
    with open(ruta_imagen, "rb") as f:
        imagen_id = fs.put(f, filename=nombre_imagen, content_type="image/jpeg")

    db["imagenes_json"].insert_one({
        "nombre": nombre_imagen,
        "imagen_id": imagen_id,
        "datos": datos_json
    })

    return imagen_id

def recuperar_imagen(nombre_imagen, ruta_salida):
    entrada = db["imagenes_json"].find_one({"nombre": nombre_imagen})
    if not entrada:
        return None

    imagen_binaria = fs.get(entrada["imagen_id"]).read()

    os.makedirs(os.path.dirname(ruta_salida), exist_ok=True)
    with open(ruta_salida, "wb") as f:
        f.write(imagen_binaria)

    return ruta_salida

def obtener_datos_json(nombre_imagen):
    entrada = db["imagenes_json"].find_one({"nombre": nombre_imagen}, {"_id": 0, "datos": 1})
    return entrada["datos"] if entrada else None

def listar_nombres_imagenes():
    resultados = db["imagenes_json"].find({}, {"_id": 0, "nombre": 1})
    return [r["nombre"] for r in resultados]
