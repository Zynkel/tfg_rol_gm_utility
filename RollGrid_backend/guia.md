# 🧭 Manual de Instalación y Uso del Backend de Análisis de Mapas con Cuadrícula – RollGrid

Este backend permite subir mapas de rol en imagen, analizarlos con inteligencia artificial (Detectron2 + Mask R-CNN), detectar objetos como cofres, puertas, escaleras, mesas y más, y almacenar todo en MongoDB. Está pensado para integrarse con una aplicación como Unity para gestionar sesiones de partidas.

---

## ✅ Requisitos del sistema

- Python 3.11 o superior
- MongoDB instalado localmente y ejecutándose en `localhost:27017`
- Git (opcional, para clonar el repositorio)
- Una tarjeta gráfica NVIDIA (opcional, para usar CUDA)
- Visual Studio Code o cualquier editor compatible con Python

---

## 🔧 Instalación paso a paso

### 1. Clona o descarga el proyecto

```bash
git clone https://github.com/tu_usuario/tfg_rol_gm_utility.git
cd tfg_rol_gm_utility/RollGrid_backend
```

### 2. Crea un entorno virtual (opcional pero recomendado)

```bash
python -m venv env
env\Scripts\activate      # En Windows
source env/bin/activate   # En Linux o macOS
```

### 3. Asegúrate de que el archivo `requirements.txt` esté presente

Si no lo tienes, solicita el archivo o vuelve a generarlo con las dependencias necesarias.

### 4. Instala las dependencias del proyecto

```bash
pip install -r requirements.txt
```

### 5. Verifica que Detectron2 y Torch estén correctamente instalados

```python
import torch
print(torch.cuda.is_available())  # Debe ser True si tienes una GPU compatible
```

### 6. Inicia MongoDB

- Asegúrate de que el servicio esté corriendo.
- Si usas MongoDB Compass, simplemente ábrelo y conéctate a:
  ```
  mongodb://localhost:27017
  ```

---

## ▶️ Ejecutar el servidor backend

Desde la carpeta `RollGrid_backend`:

```bash
uvicorn main:app --reload
```

Accede a la interfaz interactiva de prueba en:

```
http://localhost:8000/docs
```

---

## 📂 Estructura de carpetas relevante

```
RollGrid_backend/
├── app/
│   ├── db/                  # Lógica para conexión y almacenamiento en MongoDB
│   ├── models/              # Modelos Pydantic: Mapa, EstadoMapa, ObjetoEnMapa
│   ├── routers/             # Rutas de API FastAPI (endpoints)
│   ├── services/            # Lógica de procesamiento con IA y cuadrículas
│   └── __init__.py
├── models/                  # Modelos IA entrenados (ej. mask_rcnn_model.pth)
├── temp/                    # Imágenes temporales (eliminadas tras procesar)
├── main.py                  # Archivo principal del backend FastAPI
├── requirements.txt         # Lista de dependencias
```

---

## 🔌 Endpoints disponibles (vía `/docs`)

### `POST /mapas`
- Sube un nuevo mapa (imagen)
- La imagen se guarda en MongoDB con GridFS
- Se analiza con IA y se crean los objetos detectados
- Se almacena un estado inicial con todos los objetos

### `GET /mapas`
- Devuelve la lista de mapas disponibles
- Muestra su nombre e ID

### `GET /mapas/{mapa_id}`
- Devuelve un mapa completo por su ID
- Incluye los estados asociados (sesiones)

### `POST /mapas/{mapa_id}/estados`
- Añade un nuevo estado/sesión a un mapa
- Puedes incluir nuevos objetos, trampas, cambios de estado (ej. trampas activas, cofres vacíos, etc.)

---

## 🗃️ Visualizar los datos en MongoDB Compass

1. Abre MongoDB Compass
2. Conéctate a:

```
mongodb://localhost:27017
```

3. Explora la base de datos `rollgrid_db`
   - `mapas`: contiene los mapas con sus estados y referencias de imagen
   - `fs.files` y `fs.chunks`: contiene los binarios de las imágenes guardadas en GridFS

---

## 🧱 Estructura de un JSON de mapa (ejemplo)

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
          "descripcion": "Cofre polvoriento con runas",
          "contenido": ["pociones", "oro"],
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

## 📘 Notas finales

- Las imágenes se procesan y se eliminan automáticamente de la carpeta `temp/`.
- El análisis IA está basado en un modelo entrenado con Detectron2 y Mask R-CNN.
- Toda la información puede ser recuperada desde MongoDB para usarla en una app como Unity.

---

¿Dudas? Verifica la consola donde ejecutas `uvicorn` o consulta el archivo `main.py`.
