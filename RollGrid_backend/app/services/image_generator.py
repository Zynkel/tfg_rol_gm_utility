import os
import time
import base64
import requests
import anyio
from typing import Tuple
from deep_translator import GoogleTranslator

HF_API_TOKEN = os.getenv("HF_API_TOKEN")
HF_MODEL_ID = "stabilityai/stable-diffusion-xl-base-1.0"

INTERIOR_KW = {"mazmorra", "cueva", "interior", "castillo", "casa", "taberna", "tumba", "cripta", "calabozo"}
EXTERIOR_KW = {"bosque", "campo", "llanura", "pueblo", "ciudad", "río", "rio", "lago", "costa", "playa", "montaña", "montanas", "montañas", "pradera"}

DEFAULT_NEGATIVE = (
    "isometric, 3d perspective, angled view, oblique view, tilt, depth perspective, "
    "foreshortening, shadows from angled lighting, camera angle, vanishing point, "
    "skewed projection, first person, side view, front view"
)

def classify_tipo_es(texto: str) -> str:
    t = texto.lower()
    if any(k in t for k in INTERIOR_KW):
        return "interior"
    if any(k in t for k in EXTERIOR_KW):
        return "exterior"
    return "interior"

def build_prompt_final(tipo: str, prompt_en: str, estilo: str) -> str:
    if tipo == "exterior":
        base = (
            "strictly top-down (orthographic) fantasy overworld map, flat 2D, overhead view, "
            "grid-based layout, clear paths and terrain, clean grid lines, like a tabletop RPG map, "
        )
    else:
        base = (
            "strictly top-down (orthographic) fantasy dungeon/indoor map, flat 2D, overhead view, "
            "grid-based layout, clear rooms and corridors, clean grid lines, like a tabletop RPG map, "
        )

    style_suffix = f", {estilo} style" if estilo and estilo != "default" else ""
    suffix = ", minimal shading, highly detailed, consistent top-down perspective"
    return f"{base}{prompt_en}{style_suffix}{suffix}"

def translate_to_en(text_es: str) -> str:
    return GoogleTranslator(source="auto", target="en").translate(text_es)

def _call_hf_text2img_sync(
    prompt_final: str,
    negative_prompt: str = DEFAULT_NEGATIVE,
    model_id: str = HF_MODEL_ID,
    width: int = 768,
    height: int = 768,
    steps: int = 30,
    guidance: float = 7.5,
    timeout: int = 90,
    retries: int = 3,
    retry_wait: float = 2.5,
) -> str:
    if not HF_API_TOKEN:
        raise RuntimeError("Falta HF_API_TOKEN en el entorno (.env).")

    url = f"https://api-inference.huggingface.co/models/{model_id}"
    headers = {
        "Authorization": f"Bearer {HF_API_TOKEN}",
        "Accept": "image/png",
    }
    payload = {
        "inputs": prompt_final,
        "parameters": {
            "width": width,
            "height": height,
            "num_inference_steps": steps,
            "guidance_scale": guidance,
            "negative_prompt": negative_prompt,
        },
        "options": {"wait_for_model": True},
    }

    last_err = None
    for _ in range(retries):
        try:
            resp = requests.post(url, headers=headers, json=payload, timeout=timeout)

            if resp.status_code == 503:
                time.sleep(retry_wait)
                continue

            if resp.status_code != 200:
                try:
                    err_txt = resp.json()
                except Exception:
                    err_txt = resp.text
                raise RuntimeError(f"HF API error {resp.status_code}: {err_txt}")

            image_bytes = resp.content
            return base64.b64encode(image_bytes).decode("utf-8")

        except Exception as e:
            last_err = e
            time.sleep(retry_wait)

    raise RuntimeError(f"No se pudo generar imagen tras reintentos. Último error: {last_err}")

async def generate_map_image(prompt_es: str, tipo: str = "auto", estilo: str = "default") -> Tuple[str, str]:
    resolved_tipo = classify_tipo_es(prompt_es) if tipo == "auto" else tipo
    prompt_en = translate_to_en(prompt_es)
    prompt_final = build_prompt_final(resolved_tipo, prompt_en, estilo)

    image_b64 = await anyio.to_thread.run_sync(
        _call_hf_text2img_sync,
        prompt_final,
        DEFAULT_NEGATIVE,
    )

    return image_b64, prompt_final

def _call_hf_img2img_sync(
    prompt_final: str,
    init_image_bytes: bytes,
    model_id: str = HF_MODEL_ID,
    width: int = 768,
    height: int = 768,
    steps: int = 30,
    guidance: float = 7.5,
    timeout: int = 90,
    retries: int = 3,
    retry_wait: float = 2.5,
) -> str:
    if not HF_API_TOKEN:
        raise RuntimeError("Falta HF_API_TOKEN en el entorno (.env).")

    url = f"https://api-inference.huggingface.co/models/{model_id}"
    headers = {
        "Authorization": f"Bearer {HF_API_TOKEN}",
        "Accept": "image/png",
        "Content-Type": "application/json",
    }

    payload = {
        "inputs": prompt_final,
        "parameters": {
            "width": width,
            "height": height,
            "num_inference_steps": steps,
            "guidance_scale": guidance,
        },
        "options": {"wait_for_model": True},
        # Imagen codificada en base64
        "image": base64.b64encode(init_image_bytes).decode("utf-8"),
    }

    last_err = None
    for _ in range(retries):
        try:
            resp = requests.post(url, headers=headers, json=payload, timeout=timeout)

            if resp.status_code == 503:
                time.sleep(retry_wait)
                continue

            if resp.status_code != 200:
                try:
                    err_txt = resp.json()
                except Exception:
                    err_txt = resp.text
                raise RuntimeError(f"HF API error {resp.status_code}: {err_txt}")

            image_bytes = resp.content
            return base64.b64encode(image_bytes).decode("utf-8")

        except Exception as e:
            last_err = e
            time.sleep(retry_wait)

    raise RuntimeError(f"No se pudo generar imagen tras reintentos. Último error: {last_err}")
