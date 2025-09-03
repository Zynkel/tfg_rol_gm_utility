import os
from pathlib import Path
import torch
import cv2
import numpy as np
from detectron2.config import get_cfg
from detectron2.engine import DefaultPredictor
from detectron2 import model_zoo

ROOT = Path(__file__).resolve().parents[2]  
MODEL_PATH = os.getenv("RG_MODEL_WEIGHTS", str(ROOT / "models" / "mask_rcnn_model.pth"))

CLASSES = [
    "mesa", "puerta", "escalera", "armario", "casa",
    "cama", "libreria", "barriles", "barril", "cofre",
    "escaleras", "caja", "tienda_de_tela", "saco"
]

def cargar_predictor():
    if not os.path.isfile(MODEL_PATH):
        raise FileNotFoundError(f"No se encontró el checkpoint del modelo: {MODEL_PATH}")

    cfg = get_cfg()
  
    cfg.merge_from_file(model_zoo.get_config_file("COCO-InstanceSegmentation/mask_rcnn_R_50_FPN_3x.yaml"))


    cfg.MODEL.ROI_HEADS.NUM_CLASSES = len(CLASSES)


    cfg.MODEL.WEIGHTS = MODEL_PATH

    cfg.MODEL.ROI_HEADS.SCORE_THRESH_TEST = 0.3



    cfg.MODEL.DEVICE = "cuda" if torch.cuda.is_available() else "cpu"
    print(f"[OBJ] Cargando pesos: {cfg.MODEL.WEIGHTS} | device={cfg.MODEL.DEVICE}")
    return DefaultPredictor(cfg)


_predictor = None
def _get_predictor():
    global _predictor
    if _predictor is None:
        _predictor = cargar_predictor()
    return _predictor

def analizar_objetos(imagen_path: str) -> list:
    image = cv2.imread(imagen_path)
    if image is None:
        raise ValueError(f"No se pudo cargar la imagen desde {imagen_path}")

    predictor = _get_predictor()
    outputs = predictor(image)

    inst = outputs["instances"].to("cpu")
    boxes = inst.pred_boxes.tensor.numpy() if inst.has("pred_boxes") else np.zeros((0, 4))
    classes = inst.pred_classes.numpy() if inst.has("pred_classes") else np.zeros((0,), dtype=int)
    scores = inst.scores.numpy() if inst.has("scores") else np.zeros((len(classes),), dtype=float)

    masks = []
    if inst.has("pred_masks"):
        pm = inst.pred_masks.numpy()  # [N, H, W] bool
      
        for m in pm:
            masks.append(m.astype(np.uint8).tolist())
    else:
        masks = [None] * len(classes)

    resultados = []
    for i in range(len(classes)):
        x1, y1, x2, y2 = boxes[i].tolist()
        cx = int((x1 + x2) / 2)
        cy = int((y1 + y2) / 2)
        class_id = int(classes[i])
        label = CLASSES[class_id] if 0 <= class_id < len(CLASSES) else f"cls_{class_id}"
        score = float(scores[i])

        
        resultados.append({
            
            "tipo": label,
            "posicion": {"x": cx, "y": cy},
            "visible": True,

            
            "label": label,
            "score": score,
            "bbox": [int(x1), int(y1), int(x2), int(y2)],
            "mask": masks[i]  
        })

    return resultados
