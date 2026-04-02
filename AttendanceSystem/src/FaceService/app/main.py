"""
Entry point FastAPI — composición de dependencias.
El modelo se carga en background al arrancar (no bloquea el health check).
"""

import logging
from contextlib import asynccontextmanager
from concurrent.futures import ThreadPoolExecutor

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.config import get_settings
from app.api.routes import router, set_engine
from app.adapters.insightface_engine import InsightFaceEngine
from app.middleware.security import ApiKeyMiddleware, RateLimitMiddleware

_executor = ThreadPoolExecutor(max_workers=1)


def _load_engine() -> InsightFaceEngine:
    """Carga el motor en un thread separado para no bloquear el event loop."""
    settings = get_settings()
    engine = InsightFaceEngine(
        model_name=settings.detection_model,
        det_size=settings.detection_size,
        gpu_id=settings.gpu_id,
    )
    # Forzar carga inmediata del modelo
    engine._ensure_loaded()
    return engine


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Startup: carga modelo. Shutdown: limpieza."""
    settings = get_settings()

    logging.basicConfig(
        level=getattr(logging, settings.log_level.upper(), logging.INFO),
        format="%(asctime)s | %(levelname)-7s | %(name)s | %(message)s",
        datefmt="%H:%M:%S",
    )
    logger = logging.getLogger("face_service")
    logger.info("Iniciando FaceService en :%d...", settings.port)

    # Cargar modelo en background thread
    import asyncio
    loop = asyncio.get_event_loop()
    engine = await loop.run_in_executor(_executor, _load_engine)
    set_engine(engine)

    logger.info("FaceService listo.")
    yield
    logger.info("FaceService detenido.")


def create_app() -> FastAPI:
    settings = get_settings()

    app = FastAPI(
        title="FaceService — Reconocimiento Facial",
        version="1.0.0",
        lifespan=lifespan,
    )

    # Middlewares (orden inverso de ejecución)
    app.add_middleware(RateLimitMiddleware, max_requests=120, window_seconds=60)
    app.add_middleware(ApiKeyMiddleware)
    app.add_middleware(
        CORSMiddleware,
        allow_origins=settings.allowed_origins,
        allow_methods=["POST", "GET"],
        allow_headers=["*"],
    )

    app.include_router(router)

    return app


app = create_app()
