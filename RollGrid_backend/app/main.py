from fastapi import FastAPI
from app.routers import mapas

app = FastAPI()

app.include_router(mapas.router)
