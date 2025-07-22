import os
import torch
import cv2
from detectron2.config import get_cfg
from detectron2.engine import DefaultPredictor
from detectron2 import model_zoo


MODEL_PATH = os.path.join("models", "mask_rcnn_model.pth")


CLASSES = [
    "mesa", "puerta", "escalera", "armario", "casa",
    "cama", "libreria", "barriles", "barril", "cofre",
    "escaleras", "caja", "tienda_de_tela", "saco"
]

def cargar_predictor():
    cfg = get_cfg()
    cfg.merge_from_file(
        model_zoo.get_config_file("COCO-InstanceSegmentation/mask_rcnn_R_50_FPN_3x.yaml")
    )
    cfg.MODEL.ROI_HEADS.NUM_CLASSES = len(CLASSES)
    cfg.MODEL.WEIGHTS = MODEL_PATH
    cfg.MODEL.ROI_HEADS.SCORE_THRESH_TEST = 0.5
    cfg.MODEL.DEVICE = "cuda" if torch.cuda.is_available() else "cpu"
    return DefaultPredictor(cfg)

predictor = cargar_predictor()  

def analizar_objetos(imagen_path: str) -> list:
    image = cv2.imread(imagen_path)
    if image is None:
        raise ValueError(f"No se pudo cargar la imagen desde {imagen_path}")
    
    outputs = predictor(image)

    resultados = []
    pred_boxes = outputs["instances"].pred_boxes.tensor.cpu().numpy()
    pred_classes = outputs["instances"].pred_classes.cpu().numpy()

    for i in range(len(pred_classes)):
        box = pred_boxes[i]
        class_id = int(pred_classes[i])
        label = CLASSES[class_id] if class_id < len(CLASSES) else "objeto"

        # Coordenadas del centro
        x = int((box[0] + box[2]) / 2)
        y = int((box[1] + box[3]) / 2)

        resultados.append({
            "tipo": label,
            "posicion": {"x": x, "y": y}
        })

    return resultados
