import cv2
import json
import os
import numpy as np

def dibujar_cuadricula_desde_json(ruta_imagen, ruta_json, ruta_salida):
    imagen = cv2.imread(ruta_imagen)
    if imagen is None:
        print(f"No se pudo cargar la imagen: {ruta_imagen}")
        return

    with open(ruta_json, "r") as f:
        datos = json.load(f)

    origen = datos.get("origen_cuadricula")
    espaciado = datos.get("espaciado_rejilla")
    horizontales = datos.get("lineas_horizontales", [])
    verticales = datos.get("lineas_verticales", [])

    print("Horizontales detectadas:", horizontales)
    print("Verticales detectadas:", verticales)

    debug = imagen.copy()

    # Dibujar líneas horizontales
    for y in horizontales:
        cv2.line(debug, (0, y), (debug.shape[1], y), (0, 255, 0), 1)

    # Dibujar líneas verticales
    for x in verticales:
        cv2.line(debug, (x, 0), (x, debug.shape[0]), (0, 255, 0), 1)

    # Combinar original y debug lado a lado
    combinado = np.hstack((imagen, debug))
    os.makedirs(os.path.dirname(ruta_salida), exist_ok=True)
    cv2.imwrite(ruta_salida, combinado)
    print(f"Imagen comparativa guardada en: {ruta_salida}")

if __name__ == "__main__":
    dibujar_cuadricula_desde_json(
        "ejemplos/dungeon_pb1.jpg",
        "salida/dungeon_pb1_cuadricula.json",
        "salida/dungeon_pb1_comparacion.jpg"
    )
