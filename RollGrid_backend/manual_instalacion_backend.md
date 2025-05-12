**Manual de Instalación y Uso del Backend de Análisis de Mapas con Cuadrícula**

---

### 🚀 Requisitos del sistema

- Python 3.11 o superior
- MongoDB instalado localmente (localhost:27017)
- Git (opcional, para clonar el repositorio)

### 🔧 Instalación paso a paso

1. **Clona o descarga el proyecto:**
   ```bash
   git clone https://github.com/tu_usuario/tfg_rol_gm_utility.git
   cd tfg_rol_gm_utility/RollGrid_backend
   ```

2. **Crea un entorno virtual (opcional pero recomendado):**
   ```bash
   python -m venv env
   env\Scripts\activate  # En Windows
   source env/bin/activate  # En Linux/macOS
   ```

3. **Coloca el archivo `requirements.txt` en la carpeta `RollGrid_backend`**
   - Si no lo tienes, descárgalo desde el proyecto o pide a tu compañero de equipo.

4. **Instala las dependencias necesarias:**
   ```bash
   pip install -r requirements.txt
   ```

5. **Asegúrate de que MongoDB esté corriendo**:
   - Si está instalado localmente, solo abre MongoDB Compass o asegúrate de que el servicio esté activo.

6. **Inicia el backend:**
   ```bash
   uvicorn main:app --reload
   ```
   Accede a la interfaz de prueba en:
   ```
   http://localhost:8000/docs
   ```

---

### 📂 Estructura de carpetas relevante

```
RollGrid_backend/
├── app/
│   ├── db/                  # Lógica para guardar en MongoDB con GridFS
│   └── utils/               # Análisis de cuadrícula y herramientas
├── ejemplos/                # Imágenes de prueba
├── salida/                  # Archivos JSON y visualizaciones de depuración
├── temp/                    # Archivos temporales (se eliminan automáticamente)
├── main.py                  # Archivo principal de FastAPI
├── requirements.txt         # Archivo con dependencias necesarias
```

---

### 🔹 Uso de los endpoints principales

1. **`POST /upload`**
   - Sube una imagen para ser almacenada y analizada.
   - Se detecta la cuadrícula (si existe) y se guarda en MongoDB.

2. **`GET /imagenes`**
   - Devuelve una lista con los nombres de las imágenes almacenadas.

3. **`GET /imagen/{nombre}`**
   - Devuelve un objeto JSON con:
     - nombre
     - metadatos (cuadrícula)
     - imagen codificada en base64

4. **`GET /datos/{nombre}`**
   - Devuelve solo el JSON con los datos de la cuadrícula detectada.

---

### 📊 Visualización de datos con MongoDB Compass

1. **Abre MongoDB Compass**
2. Conéctate a:
   ```
   mongodb://localhost:27017
   ```
3. Ve a la base de datos `rollgrid_db`
4. Explora:
   - `imagenes_json`: contiene metadatos + nombre de la imagen
   - `fs.files`: archivos subidos (nombre, tamaño, tipo)
   - `fs.chunks`: datos binarios reales

---

### 📘 Notas finales

- Las imágenes temporales se eliminan automáticamente.
- El JSON generado por la detección de cuadrícula puede ser usado directamente por Unity u otro sistema.

---

Si tienes dudas o errores, asegúrate de revisar la consola del servidor donde se ejecuta `uvicorn`, o consulta el archivo `main.py`.
