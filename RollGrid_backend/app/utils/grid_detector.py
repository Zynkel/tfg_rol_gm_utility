import cv2
import numpy as np
import os
import json

def median_spacing(vals):
    if len(vals) < 2:
        return None
    v = sorted(set(vals))
    diffs = np.diff(v)
    return int(np.median(diffs)) if len(diffs) > 0 else None

def agrupar_lineas(l, tolerancia=10):
    if not l:
        return []
    l = sorted(set(l))
    agrupadas = []
    grupo = [l[0]]
    for val in l[1:]:
        if abs(val - grupo[-1]) <= tolerancia:
            grupo.append(val)
        else:
            agrupadas.append(int(np.median(grupo)))
            grupo = [val]
    agrupadas.append(int(np.median(grupo)))
    return agrupadas

def detectar_cuadricula(image_path, results_dir, line_gap=10):
    img = cv2.imread(image_path)
    if img is None:
        print(f"Error: no se pudo cargar la imagen en '{image_path}'")
        return

    original = img.copy()
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    gray = cv2.equalizeHist(gray)
    blur = cv2.GaussianBlur(gray, (5, 5), 0)
    edged = cv2.Canny(blur, 50, 150)

    lines = cv2.HoughLinesP(edged, 1, np.pi / 180, threshold=80, minLineLength=50, maxLineGap=line_gap)
    horiz_raw, vert_raw = [], []

    if lines is not None:
        for l in lines:
            x1, y1, x2, y2 = l[0]
            angle = np.arctan2(y2 - y1, x2 - x1) * 180 / np.pi
            if -10 < angle < 10:
                horiz_raw.append(y1)
            elif 80 < abs(angle) < 100:
                vert_raw.append(x1)

    horiz = agrupar_lineas(horiz_raw)
    vert = agrupar_lineas(vert_raw)
    sx = median_spacing(vert)
    sy = median_spacing(horiz)
    origen = {
        "x": min(vert) if vert else None,
        "y": min(horiz) if horiz else None
    }

    base = os.path.splitext(os.path.basename(image_path))[0]
    json_data = {
        "imagen": os.path.basename(image_path),
        "modelo": "OpenCV",
        "origen_cuadricula": origen,
        "espaciado_rejilla": {"x": sx if sx else None, "y": sy if sy else None},
        "lineas_verticales": vert,
        "lineas_horizontales": horiz
    }

    os.makedirs(results_dir, exist_ok=True)
    json_path = os.path.join(results_dir, f"{base}_cuadricula.json")
    with open(json_path, 'w') as jf:
        json.dump(json_data, jf, indent=4)
    print(f"JSON de cuadrícula guardado en: {json_path}")

if __name__ == "__main__":
    detectar_cuadricula("ejemplos/dungeon_pb1.jpg", "salida")
