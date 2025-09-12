
#  Manual de Instalación y Uso del Backend de Análisis de Mapas con Cuadrícula – RollGrid

Este backend permite subir mapas de rol en imagen, analizarlos con inteligencia artificial (Detectron2 + Mask R-CNN), detectar objetos como cofres, puertas, escaleras, mesas y más, y almacenar todo en MongoDB. Está pensado para integrarse con una aplicación como Unity para gestionar sesiones de partidas.

---

## Requisitos del sistema

- Python 3.11 o superior
- MongoDB instalado localmente y ejecutándose en `localhost:27017`
- Git (opcional, para clonar el repositorio)
- Una tarjeta gráfica NVIDIA (opcional, para usar CUDA)
- Visual Studio Code o cualquier editor compatible con Python

---

## Instalación paso a paso

### 1. Clona o descarga el proyecto (recomendado con Git LFS)

Este proyecto utiliza [Git LFS](https://git-lfs.com/) para gestionar archivos grandes como el modelo entrenado (`mask_rcnn_model.pth`).

```bash
# Instala Git LFS (una sola vez en tu sistema)
# En Windows con Chocolatey
choco install git-lfs

# En Linux
sudo apt install git-lfs

# En macOS
brew install git-lfs
```

```bash
# Clona el repositorio correctamente
git lfs install
git clone https://github.com/tu_usuario/tfg_rol_gm_utility.git
cd tfg_rol_gm_utility/RollGrid_backend
```

Verifica que el archivo `models/mask_rcnn_model.pth` esté descargado. Si no se descarga, ejecuta:

```bash
git lfs pull
```

> Si descargas el proyecto como `.zip`, el modelo **no estará incluido**. Debes usar `git clone` con Git LFS.

---

### 2. Crea un entorno virtual (opcional pero recomendado)

```bash
python -m venv env
env\Scripts\activate      # En Windows
source env/bin/activate     # En Linux o macOS
```

---

### 3. Desinstala versiones antiguas de PyTorch si es necesario

```bash
pip uninstall torch torchvision torchaudio -y
```

---

### 4. Instala PyTorch compatible con Python 3.11

#### Versión CPU (si no tienes GPU NVIDIA)

```bash
pip install torch==2.1.0 torchvision==0.16.0 torchaudio==2.1.0 --index-url https://download.pytorch.org/whl/cpu
```

#### Versión con CUDA (si tienes GPU NVIDIA compatible)

```bash
pip install torch==2.1.0 torchvision==0.16.0 torchaudio==2.1.0 --index-url https://download.pytorch.org/whl/cu118
```

> Verifica si tienes CUDA disponible con este código:

```python
import torch
print(torch.cuda.is_available())
```

---

### 5. Instala el resto de dependencias del proyecto

```bash
pip install -r requirements.txt
```

---

### 6. Asegúrate de que MongoDB esté corriendo

- Puedes usar MongoDB Compass para conectarte a:
  ```
  mongodb://localhost:27017
  ```

---

## Ejecutar el servidor backend

Desde la carpeta `RollGrid_backend`:

```bash
uvicorn main:app --reload
```

Accede a la interfaz Swagger en:

```
http://localhost:8000/docs
```

---

## Estructura de carpetas relevante

```
RollGrid_backend/
├── app/
│   ├── db/                  # Conexión y lógica con MongoDB y GridFS
│   ├── models/              # Modelos Pydantic
│   ├── routers/             # Rutas de la API
│   ├── services/            # IA y análisis de cuadrículas
├── models/                  # Modelos IA (ej. mask_rcnn_model.pth)
├── temp/                    # Imágenes temporales
├── main.py                  # Entrada de FastAPI
├── requirements.txt         # Dependencias
```

---

## Endpoints disponibles (en `/docs`)

- `POST /mapas`: Sube una imagen y analiza con IA
- `GET /mapas`: Lista de mapas
- `GET /mapas/{mapa_id}`: Mapa completo (incluye imagen en base64)
- `GET /mapas/{mapa_id}/datos`: Datos sin imagen
- `GET /mapas/{mapa_id}/imagen`: Imagen binaria
- `POST /mapas/{mapa_id}/estados`: Añadir estado/sesión
- `GET /mapas/{mapa_id}/estados`: Lista de estados
- `GET /mapas/{mapa_id}/estados/{estado_id}`: Obtener estado individual
- `PUT /mapas/{mapa_id}`: Actualizar mapa (nombre o imagen)
- `DELETE /mapas/{mapa_id}`: Eliminar mapa

---

## Estructura de un JSON de mapa (ejemplo)

```json
{
  "nombre": "Mapa del orfanato",
  "imagen_id": "66b1f6...",
  "estados": [
    {
      "nombre": "Estado inicial",
      "objetos": [
        {
          "tipo": "cofre",
          "estado": "cerrado",
          "grid_pos": { "x": 12, "y": 7 }
        },
        {
          "tipo": "puerta",
          "estado": "bloqueada",
          "destino": {
            "mapa_id": "66c9f...",
            "estado_id": "inicio"
          },
          "grid_pos": { "x": 5, "y": 0 }
        }
      ]
    }
  ]
}
```

---

## Notas finales

- Las imágenes se eliminan de la carpeta `temp/` tras procesarlas.
- Toda la información se almacena en MongoDB (colección `mapas` y `fs.*`).
- El sistema permite gestionar múltiples estados para una misma imagen.
- Es compatible con expansión para nuevas sesiones analizadas con IA.

---

¿Dudas? Consulta la consola de FastAPI (`uvicorn`) o el archivo `main.py`.
