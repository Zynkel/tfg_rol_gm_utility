from dotenv import load_dotenv
load_dotenv()

from fastapi import FastAPI
from app.routers import mapas, generator
from fastapi.staticfiles import StaticFiles

app = FastAPI()
app.mount("/static", StaticFiles(directory="static"), name="static")

app.include_router(mapas.router)
app.include_router(generator.router)
