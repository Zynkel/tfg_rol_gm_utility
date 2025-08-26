# app/services/procedural_generator.py
from __future__ import annotations
import random
from typing import Dict, List, Tuple, Set, Optional
from PIL import Image, ImageDraw

# ===============================
# Tipos
# ===============================
Point = Tuple[int, int]

# ===============================
# Utilidades
# ===============================
def rect_intersects(a: Dict, b: Dict, padding: int = 1) -> bool:
    ax0, ay0 = a["x"] - padding, a["y"] - padding
    ax1, ay1 = a["x"] + a["width"] + padding - 1, a["y"] + a["height"] + padding - 1

    bx0, by0 = b["x"] - padding, b["y"] - padding
    bx1, by1 = b["x"] + b["width"] + padding - 1, b["y"] + b["height"] + padding - 1

    return not (ax1 < bx0 or bx1 < ax0 or ay1 < by0 or by1 < ay0)


def manhattan_path(a: Point, b: Point, first: str = "h") -> List[Point]:
    (x0, y0), (x1, y1) = a, b
    path: List[Point] = []

    if first == "h":
        step = 1 if x1 >= x0 else -1
        for x in range(x0, x1 + step, step):
            path.append((x, y0))
        step = 1 if y1 >= y0 else -1
        for y in range(y0 + step, y1 + step, step):
            path.append((x1, y))
    else:
        step = 1 if y1 >= y0 else -1
        for y in range(y0, y1 + step, step):
            path.append((x0, y))
        step = 1 if x1 >= x0 else -1
        for x in range(x0 + step, x1 + step, step):
            path.append((x, y1))

    return path


def clamp(v: int, lo: int, hi: int) -> int:
    return max(lo, min(hi, v))

def generate_basic_layout(
    width: int = 40,
    height: int = 30,
    rooms_target: int = 5,
    room_w_range: Tuple[int, int] = (5, 9),
    room_h_range: Tuple[int, int] = (4, 8),
    seed: Optional[int] = None,
) -> Dict:
    if seed is not None:
        random.seed(seed)

    layout = {
        "width": width,
        "height": height,
        "rooms": [],       # {x,y,width,height}
        "corridors": [],   # {"cells": [(x,y),...]}
        "doors": []        # {"x": int, "y": int}
    }

    attempts = rooms_target * 20
    while len(layout["rooms"]) < rooms_target and attempts > 0:
        attempts -= 1
        rw = random.randint(*room_w_range)
        rh = random.randint(*room_h_range)

        x = random.randint(2, width - rw - 3) if width - rw - 3 >= 2 else 1
        y = random.randint(2, height - rh - 3) if height - rh - 3 >= 2 else 1

        candidate = {"x": x, "y": y, "width": rw, "height": rh}

        if all(not rect_intersects(candidate, r, padding=1) for r in layout["rooms"]):
            layout["rooms"].append(candidate)


    if not layout["rooms"]:
        rw = clamp(room_w_range[0], 3, width - 4)
        rh = clamp(room_h_range[0], 3, height - 4)
        layout["rooms"].append({"x": (width - rw)//2, "y": (height - rh)//2, "width": rw, "height": rh})

    rooms_sorted = sorted(layout["rooms"], key=lambda r: (r["x"], r["y"]))
    corridor_cells: Set[Point] = set()

    def room_center(r) -> Point:
        cx = r["x"] + r["width"] // 2
        cy = r["y"] + r["height"] // 2
        return (cx, cy)

    for i in range(len(rooms_sorted) - 1):
        r1, r2 = rooms_sorted[i], rooms_sorted[i+1]
        c1, c2 = room_center(r1), room_center(r2)
        first = "h" if random.random() < 0.5 else "v"
        path = manhattan_path(c1, c2, first=first)

        path = [(clamp(x, 1, width - 2), clamp(y, 1, height - 2)) for (x, y) in path]

        layout["corridors"].append({"cells": path})
        corridor_cells.update(path)

        entry = _door_for_room_edge(r1, path)
        exit_ = _door_for_room_edge(r2, list(reversed(path)))
        if entry:
            layout["doors"].append({"x": entry[0], "y": entry[1]})
        if exit_:
            layout["doors"].append({"x": exit_[0], "y": exit_[1]})

    return layout


def _door_for_room_edge(room: Dict, path: List[Point]) -> Optional[Point]:

    x0, y0 = room["x"], room["y"]
    x1, y1 = x0 + room["width"] - 1, y0 + room["height"] - 1

    for (px, py) in path:
        touching_left   = (px == x0 - 1) and (y0 <= py <= y1)
        touching_right  = (px == x1 + 1) and (y0 <= py <= y1)
        touching_top    = (py == y0 - 1) and (x0 <= px <= x1)
        touching_bottom = (py == y1 + 1) and (x0 <= px <= x1)

        if touching_left:
            return (x0, py)
        if touching_right:
            return (x1, py)
        if touching_top:
            return (px, y0)
        if touching_bottom:
            return (px, y1)
    return None


def render_layout_to_image(
    layout: Dict,
    cell_size: int = 16,
    grid: bool = False,
    corridor_width_px: Optional[int] = None
) -> Image.Image:
    """
    Render simple:
      - Fondo blanco
      - Habitaciones: gris claro con borde negro
      - Pasillos: gris medio (líneas gruesas)
      - Puertas: líneas cortas negras en el borde
      - (opcional) Grid
    """
    W, H = layout["width"], layout["height"]
    width_px, height_px = W * cell_size, H * cell_size

    img = Image.new("RGB", (width_px, height_px), "white")
    draw = ImageDraw.Draw(img)

    if grid:
        for x in range(0, width_px + 1, cell_size):
            draw.line([(x, 0), (x, height_px)], fill=(230, 230, 230))
        for y in range(0, height_px + 1, cell_size):
            draw.line([(0, y), (width_px, y)], fill=(230, 230, 230))

    for r in layout["rooms"]:
        x0, y0 = r["x"] * cell_size, r["y"] * cell_size
        x1, y1 = x0 + r["width"] * cell_size, y0 + r["height"] * cell_size
        draw.rectangle([x0, y0, x1, y1], fill=(220, 220, 220), outline=(0, 0, 0), width=2)

    cw = corridor_width_px if corridor_width_px is not None else max(2, cell_size // 2)
    for cor in layout["corridors"]:
        cells: List[Point] = cor["cells"]
        if not cells:
            continue
        for i in range(len(cells) - 1):
            (x0, y0), (x1, y1) = cells[i], cells[i + 1]
            x0p = x0 * cell_size + cell_size // 2
            y0p = y0 * cell_size + cell_size // 2
            x1p = x1 * cell_size + cell_size // 2
            y1p = y1 * cell_size + cell_size // 2
            draw.line([(x0p, y0p), (x1p, y1p)], fill=(160, 160, 160), width=cw)

    door_len = max(2, cell_size // 2)
    for d in layout["doors"]:
        dx, dy = d["x"], d["y"]
        cx, cy = dx * cell_size + cell_size // 2, dy * cell_size + cell_size // 2
        draw.line([(cx - door_len // 2, cy), (cx + door_len // 2, cy)], fill=(0, 0, 0), width=2)

    return img
