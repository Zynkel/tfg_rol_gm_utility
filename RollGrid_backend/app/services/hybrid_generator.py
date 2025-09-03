import io
import os
import anyio
from typing import Tuple
from datetime import datetime
from app.services.procedural_generator import generate_basic_layout, render_layout_to_image
from app.services.image_generator import classify_tipo_es, translate_to_en, build_prompt_final, _call_hf_img2img_sync

STATIC_DIR = "static/generated"

async def generate_hybrid_map(prompt_es: str, tipo="auto", estilo="default") -> Tuple[dict, str, str, str]:
    layout = generate_basic_layout()

    buffer = io.BytesIO()
    img = render_layout_to_image(layout)
    img.save(buffer, format="PNG")
    guide_bytes = buffer.getvalue()

    os.makedirs(STATIC_DIR, exist_ok=True)
    guide_filename = f"guide_{datetime.now().strftime('%Y%m%d_%H%M%S')}.png"
    guide_path = os.path.join(STATIC_DIR, guide_filename)
    with open(guide_path, "wb") as f:
        f.write(guide_bytes)

    resolved_tipo = classify_tipo_es(prompt_es) if tipo == "auto" else tipo
    prompt_en = translate_to_en(prompt_es)

    prompt_final = (
        build_prompt_final(resolved_tipo, prompt_en, estilo)
        + ", match the provided black and white layout image exactly for walls, doors, and rooms, "
          "2D blueprint style, clean grid, flat, consistent scale, minimal shading, highly detailed"
    )

    image_b64 = await anyio.to_thread.run_sync(
        _call_hf_img2img_sync,
        prompt_final,
        guide_bytes,
    )

    guide_url = f"/static/generated/{guide_filename}"

    return layout, image_b64, prompt_final, guide_url
