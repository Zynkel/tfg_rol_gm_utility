import cv2
import numpy as np

def _cluster_1d(vals, tol_px):
    if len(vals) == 0:
        return []
    vals = np.sort(np.asarray(vals))
    clusters = [[vals[0]]]
    for v in vals[1:]:
        if abs(v - clusters[-1][-1]) <= tol_px:
            clusters[-1].append(v)
        else:
            clusters.append([v])
    return [int(np.median(c)) for c in clusters]

def _grid_lines(gray):
    h, w = gray.shape[:2]
    clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8,8))
    eq = clahe.apply(gray)
    thr = cv2.adaptiveThreshold(eq, 255, cv2.ADAPTIVE_THRESH_MEAN_C, cv2.THRESH_BINARY_INV, 15, 2)
    vk = max(10, h // 100)
    hk = max(10, w // 100)
    vert_kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (1, vk))
    horz_kernel = cv2.getStructuringElement(cv2.MORPH_RECT, (hk, 1))
    vert_lines = cv2.morphologyEx(thr, cv2.MORPH_OPEN, vert_kernel, iterations=1)
    horz_lines = cv2.morphologyEx(thr, cv2.MORPH_OPEN, horz_kernel, iterations=1)
    proj_v = vert_lines.sum(axis=0)
    proj_h = horz_lines.sum(axis=1)
    xs = list(np.where(proj_v > 0.6 * proj_v.max())[0])
    ys = list(np.where(proj_h > 0.6 * proj_h.max())[0])
    tol = int(0.01 * max(w, h)) + 3
    xs = _cluster_1d(xs, tol)
    ys = _cluster_1d(ys, tol)
    return sorted(xs), sorted(ys)

def _draw_overlay(gray, xs, ys, color=(0,165,255), thickness=1):
    h, w = gray.shape[:2]
    imgc = cv2.cvtColor(gray, cv2.COLOR_GRAY2BGR)
    for x in xs:
        cv2.line(imgc, (x, 0), (x, h), color, thickness)
    for y in ys:
        cv2.line(imgc, (0, y), (w, y), color, thickness)
    ok, buf = cv2.imencode(".png", imgc)
    return buf.tobytes() if ok else None

def extraer_info_cuadricula(imagen_path: str, return_overlay: bool=False):
    gray = cv2.imread(imagen_path, cv2.IMREAD_GRAYSCALE)
    if gray is None:
        return None
    h, w = gray.shape[:2]
    xs, ys = _grid_lines(gray)
    if len(xs) < 2 or len(ys) < 2:
        return None
    dx = np.diff(np.array(xs))
    dy = np.diff(np.array(ys))
    cell_w = int(np.median(dx))
    cell_h = int(np.median(dy))
    offx = int(xs[0])
    offy = int(ys[0])
    cols = max(int(round((w - offx) / max(1, cell_w))), 1)
    rows = max(int(round((h - offy) / max(1, cell_h))), 1)
    res = {
        "rows": rows,
        "cols": cols,
        "cellWidth": cell_w,
        "cellHeight": cell_h,
        "offsetX": offx,
        "offsetY": offy,
        "linesX": xs,
        "linesY": ys,
    }
    if return_overlay:
        res["overlay_png"] = _draw_overlay(gray, xs, ys)
    return res
