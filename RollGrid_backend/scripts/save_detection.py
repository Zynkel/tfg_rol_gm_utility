import argparse, json, os
from typing import List, Dict, Any, Optional
import cv2, numpy as np, tkinter as tk
from tkinter import filedialog
import sys, pathlib

ROOT = pathlib.Path(__file__).resolve().parents[1]
os.chdir(ROOT)
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from app.services.grid_utils import extraer_info_cuadricula
from app.services.map_processor import analizar_objetos

def draw_title_strip(img: np.ndarray, title: str, height: int = 36) -> np.ndarray:
    h, w = img.shape[:2]
    strip = np.full((height, w, 3), 245, dtype=np.uint8)
    cv2.putText(strip, title, (12, height - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.8, (50, 50, 50), 2, cv2.LINE_AA)
    return np.vstack([strip, img])

def overlay_grid(image_bgr: np.ndarray, grid: Dict[str, Any], color=(0, 165, 255)) -> np.ndarray:
    img = image_bgr.copy()
    origin = grid.get("origin") or grid.get("origen") or [0, 0]
    cell = grid.get("cell_size") or grid.get("cellSize") or [32, 32]
    rows = int(grid.get("rows") or grid.get("filas") or 0)
    cols = int(grid.get("cols") or grid.get("columnas") or 0)
    ox, oy = int(origin[0]), int(origin[1])
    cw, ch = int(cell[0]), int(cell[1])
    h, w = img.shape[:2]
    for c in range(cols + 1):
        x = ox + c * cw
        if 0 <= x < w:
            cv2.line(img, (x, 0), (x, h), color, 1, cv2.LINE_AA)
    for r in range(rows + 1):
        y = oy + r * ch
        if 0 <= y < h:
            cv2.line(img, (0, y), (w, y), color, 1, cv2.LINE_AA)
    cv2.circle(img, (ox, oy), 4, (0, 0, 255), -1)
    return img

def overlay_detections(image_bgr: np.ndarray, detections: List[Dict[str, Any]]) -> np.ndarray:
    img = image_bgr.copy()
    for det in detections:
        label = det.get("label") or det.get("etiqueta") or "obj"
        score = det.get("score") if isinstance(det.get("score"), (float, int)) else None
        text = f"{label}" + (f" {score:.2f}" if score is not None else "")
        if "mask" in det and isinstance(det["mask"], list):
            mask = np.array(det["mask"], dtype=np.uint8)
            contours, _ = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
            cv2.drawContours(img, contours, -1, (0, 200, 0), 2)
            if len(contours) > 0 and len(contours[0]) > 0:
                x, y = contours[0][0][0]
                cv2.putText(img, text, (x, max(0, y - 5)), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 100, 0), 2, cv2.LINE_AA)
        if "bbox" in det and isinstance(det["bbox"], (list, tuple)) and len(det["bbox"]) == 4:
            x1, y1, x2, y2 = map(int, det["bbox"])
            cv2.rectangle(img, (x1, y1), (x2, y2), (36, 255, 12), 2)
            cv2.putText(img, text, (x1, max(0, y1 - 7)), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (36, 255, 12), 2, cv2.LINE_AA)
    return img

def hstack_equal_height(images: List[np.ndarray], pad: int = 12, pad_color=(255, 255, 255)) -> np.ndarray:
    target_h = max(im.shape[0] for im in images)
    out = None
    for im in images:
        h, w = im.shape[:2]
        scale = target_h / h
        imr = cv2.resize(im, (int(w * scale), target_h), interpolation=cv2.INTER_AREA)
        if out is None:
            out = imr
        else:
            pad_col = np.full((target_h, pad, 3), pad_color, dtype=np.uint8)
            out = np.hstack([out, pad_col, imr])
    return out

def load_image(path: str) -> np.ndarray:
    img = cv2.imread(path, cv2.IMREAD_COLOR)
    if img is None:
        raise FileNotFoundError(f"No se pudo abrir la imagen: {path}")
    return img

def pick_image() -> Optional[str]:
    root = tk.Tk(); root.withdraw()
    p = filedialog.askopenfilename(title="Selecciona la imagen ORIGINAL", filetypes=[("Imágenes", "*.png;*.jpg;*.jpeg;*.webp;*.bmp")])
    root.destroy()
    return p or None

def pick_dir() -> Optional[str]:
    root = tk.Tk(); root.withdraw()
    d = filedialog.askdirectory(title="Selecciona carpeta de salida")
    root.destroy()
    return d or None

def run_pipeline(image_path: str, output_path: str, grid_json: Optional[str], dets_json: Optional[str], grid_img: Optional[str], objects_img: Optional[str]) -> Dict[str, Any]:
    os.makedirs(os.path.dirname(output_path) or ".", exist_ok=True)
    orig = load_image(image_path)
    if grid_img:
        cuads = load_image(grid_img)
        grid_info = {}
    else:
        if grid_json and os.path.exists(grid_json):
            with open(grid_json, "r", encoding="utf-8") as f:
                grid = json.load(f)
        else:
            grid = extraer_info_cuadricula(image_path)
        if not grid:
            raise RuntimeError("No se pudo detectar la cuadrícula")
        if "origin" not in grid and "origen" not in grid:
            grid["origin"] = [int(grid["offsetX"]), int(grid["offsetY"])]
        if "cell_size" not in grid and "cellSize" not in grid:
            grid["cell_size"] = [int(grid["cellWidth"]), int(grid["cellHeight"])]
        cuads = overlay_grid(orig, grid)
        grid_info = grid
    if objects_img:
        objs = load_image(objects_img)
        detections = []
    else:
        if dets_json and os.path.exists(dets_json):
            with open(dets_json, "r", encoding="utf-8") as f:
                detections = json.load(f)
        else:
            detections = analizar_objetos(image_path) or []
        objs = overlay_detections(orig, detections)
    base_name, _ = os.path.splitext(output_path)
    grid_out = f"{base_name}_rejilla.png"
    objects_out = f"{base_name}_objetos.png"
    cv2.imwrite(grid_out, draw_title_strip(cuads, "Rejilla"))
    cv2.imwrite(objects_out, draw_title_strip(objs, "Objetos"))
    return {
        "image": image_path,
        "outputs": {
            "grid_image": grid_out,
            "objects_image": objects_out
        },
        "grid_summary": {
            "origin": (grid_info.get("origin") or grid_info.get("origen")) if grid_info else None,
            "cell_size": (grid_info.get("cell_size") or grid_info.get("cellSize")) if grid_info else None,
            "rows": (grid_info.get("rows") or grid_info.get("filas")) if grid_info else None,
            "cols": (grid_info.get("cols") or grid_info.get("columnas")) if grid_info else None,
        },
        "num_detections": len(detections) if isinstance(detections, list) else 0,
        "used_grid_image": bool(grid_img),
        "used_objects_image": bool(objects_img)
    }

def main():
    parser = argparse.ArgumentParser(description="Original | Cuadrícula | Objetos")
    parser.add_argument("--image", default=None)
    parser.add_argument("--output", default=None)
    parser.add_argument("--grid-json", default=None)
    parser.add_argument("--detections-json", default=None)
    parser.add_argument("--grid-img", default=None)
    parser.add_argument("--objects-img", default=None)
    args = parser.parse_args()
    image_path = args.image or pick_image()
    if not image_path:
        print(json.dumps({"error": "No se seleccionó imagen"}, ensure_ascii=False))
        return
    output_path = args.output
    if not output_path:
        out_dir = pick_dir()
        if not out_dir:
            print(json.dumps({"error": "No se seleccionó carpeta de salida"}, ensure_ascii=False))
            return
        name, _ = os.path.splitext(os.path.basename(image_path))
        output_path = os.path.join(out_dir, f"{name}_resumen.png")
    info = run_pipeline(
        image_path,
        output_path,
        args.grid_json,
        args.detections_json,
        args.grid_img,
        args.objects_img
    )
    print(json.dumps(info, ensure_ascii=False, indent=2))

if __name__ == "__main__":
    main()
