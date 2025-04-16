
🧪 Tutorial de instalación y prueba del backend de análisis de mapas

Este proyecto permite subir imágenes de mapas de rol de mesa y simula un análisis automático, devolviendo un JSON con elementos típicos (como casas, puertas, cofres, etc.) y la cuadrícula del mapa.

---

🔧 Requisitos previos

🖥️ Instalaciones necesarias:

1. Python 3.11 o superior
2. MongoDB Community Server
   Descargar e instalar desde: https://www.mongodb.com/try/download/community
   Durante la instalación:
   - Marca ✅ Install MongoDB as a service
   - Usa el usuario por defecto: Network Service

3. (Opcional pero útil) MongoDB Compass para ver la base de datos visualmente.

---

⚙️ Preparación del entorno

1. Clonar el proyecto

git clone <repositorio>
cd RollGrid_backend

2. Crear y activar entorno virtual

python -m venv env
env\Scripts\activate     # En Windows
# o
source env/bin/activate  # En Mac/Linux

3. Instalar dependencias

pip install -r requirements.txt

Si no tienes el requirements.txt, instala manualmente:

pip install fastapi uvicorn motor pymongo python-multipart

---

🗄️ Verificar que MongoDB está corriendo

1. Abre una terminal y ejecuta:

mongosh

Si conecta correctamente a mongodb://127.0.0.1:27017, ya está todo listo.

Si no está corriendo, inicia el servicio de MongoDB:

net start MongoDB      # En Windows con Mongo instalado como servicio

O bien ejecuta manualmente:

cd "C:\Program Files\MongoDB\Server\8.0\bin"
.\mongod.exe

---

🚀 Ejecutar el backend

Desde la raíz del proyecto:

uvicorn main:app --reload

---

🧪 Probar el servicio

1. Abre el navegador en:  
   http://127.0.0.1:8000/docs

2. Haz scroll hasta el endpoint POST /upload
   - Pulsa "Try it out"
   - Sube una imagen cualquiera
   - Pulsa "Execute"

3. Verás una respuesta con:
   - ID del mapa
   - Ruta de la imagen
   - JSON simulado del análisis

4. Puedes probar el endpoint GET /map/{id} con el ID recibido para recuperar el JSON y los datos de ese mapa.

---

📂 Estructura de carpetas

RollGrid_backend/
├── main.py              # Código de FastAPI
├── data/maps/           # Carpeta donde se guardan las imágenes subidas
├── env/                 # Entorno virtual (no subir al repo)
├── model/               # (Futuro) Carpeta para el modelo TFLite
└── requirements.txt     # Dependencias del proyecto
