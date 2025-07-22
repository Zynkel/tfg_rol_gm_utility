import cv2
import numpy as np
from fastapi import UploadFile
from app.db.mongo_gridfs_backend import get_mongo_fs

def guardar_imagen_en_gridfs(file: UploadFile):
    fs = get_mongo_fs()
    contenido = file.file.read()
    imagen_id = fs.put(contenido, filename=file.filename, content_type=file.content_type)
    return imagen_id

def extraer_celdas_desde_imagen(imagen_path: str) -> list | None:
    imagen = cv2.imread(imagen_path, cv2.IMREAD_GRAYSCALE)
    if imagen is None:
        return None

    blur = cv2.GaussianBlur(imagen, (5, 5), 0)
    bordes = cv2.Canny(blur, 50, 150, apertureSize=3)

    lineas = cv2.HoughLinesP(bordes, 1, np.pi / 180, threshold=100, minLineLength=50, maxLineGap=10)
    if lineas is None or len(lineas) < 20:
        return None 

    horizontales = []
    verticales = []

    for linea in lineas:
        x1, y1, x2, y2 = linea[0]
        if abs(y2 - y1) < 10:  
            horizontales.append(y1)
        elif abs(x2 - x1) < 10:  
            verticales.append(x1)

    if len(horizontales) < 2 or len(verticales) < 2:
        return None  

    horizontales = sorted(list(set(horizontales)))
    verticales = sorted(list(set(verticales)))

    celdas = []
    for i in range(len(verticales) - 1):
        for j in range(len(horizontales) - 1):
            celdas.append({"x": i, "y": j})

    return celdas
