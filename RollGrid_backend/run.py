from pymongo import MongoClient
import gridfs
import os

# Base del proyecto
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
EJEMPLOS_DIR = os.path.join(BASE_DIR, "ejemplos")
SALIDA_DIR = os.path.join(BASE_DIR, "salida")

# Conexión a MongoDB
client = MongoClient("mongodb://localhost:27017/")
db = client["rollgrid_db"]
fs = gridfs.GridFS(db)

# Crear carpetas si no existen
def asegurar_directorio(ruta):
    if not os.path.exists(ruta):
        os.makedirs(ruta)

def guardar_imagen_y_json(ruta_imagen, nombre_imagen, datos_json):
    with open(ruta_imagen, "rb") as f:
        imagen_id = fs.put(f, filename=nombre_imagen, content_type="image/jpeg")

    resultado = db["imagenes_json"].insert_one({
        "nombre": nombre_imagen,
        "imagen_id": imagen_id,
        "datos": datos_json
    })

    return resultado.inserted_id

def recuperar_imagen_y_json(nombre_imagen, ruta_salida_imagen):
    entrada = db["imagenes_json"].find_one({"nombre": nombre_imagen})
    if entrada:
        imagen_binaria = fs.get(entrada["imagen_id"]).read()

        carpeta_salida = os.path.dirname(ruta_salida_imagen)
        asegurar_directorio(carpeta_salida)

        with open(ruta_salida_imagen, "wb") as f:
            f.write(imagen_binaria)

        return entrada["datos"]
    else:
        return None

if __name__ == "__main__":
    # Asegurar carpetas locales
    asegurar_directorio(EJEMPLOS_DIR)
    asegurar_directorio(SALIDA_DIR)

    datos_ejemplo = {
        "descripcion": "Mapa de dungeon_pb1 para partida inicial",
        "elementos_detectados": ["dungeon_pb1", "sendero", "rocas"]
    }

    ruta_imagen = os.path.join(EJEMPLOS_DIR, "dungeon_pb1.jpg")
    ruta_salida = os.path.join(SALIDA_DIR, "dungeon_pb1_recuperada.jpg")

    if os.path.exists(ruta_imagen):
        id_guardado = guardar_imagen_y_json(
            ruta_imagen=ruta_imagen,
            nombre_imagen="dungeon_pb1.jpg",
            datos_json=datos_ejemplo
        )

        print("ID insertado:", id_guardado)

        datos_recuperados = recuperar_imagen_y_json(
            nombre_imagen="dungeon_pb1.jpg",
            ruta_salida_imagen=ruta_salida
        )

        print("Datos recuperados:", datos_recuperados)
    else:
        print(f"No se encontró la imagen en {ruta_imagen}. Agrega una imagen para probar.")