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
from app.api.routes import router, set_engine, set_load_error
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
    """
    Startup: inicia el servidor inmediatamente y carga el modelo en background.
    Esto permite que el health check responda desde el primer segundo,
    evitando que el cliente C# agote el timeout esperando que el puerto abra.
    """
    settings = get_settings()

    logging.basicConfig(
        level=getattr(logging, settings.log_level.upper(), logging.INFO),
        format="%(asctime)s | %(levelname)-7s | %(name)s | %(message)s",
        datefmt="%H:%M:%S",
    )
    logger = logging.getLogger("face_service")
    logger.info("Iniciando FaceService en :%d...", settings.port)

    import asyncio
    loop = asyncio.get_event_loop()

    async def _load_in_background():
        try:
            engine = await asyncio.wait_for(
                loop.run_in_executor(_executor, _load_engine),
                timeout=300,
            )
            set_engine(engine)
            logger.info("Modelo cargado. FaceService listo.")
        except asyncio.TimeoutError:
            msg = "Timeout cargando el modelo (>5 min)."
            logger.error(msg)
            set_load_error(msg)
        except Exception as exc:
            msg = str(exc)
            logger.error("Error cargando modelo: %s", msg)
            set_load_error(msg)

    # Lanzar carga en background — el servidor ya acepta conexiones
    asyncio.create_task(_load_in_background())
    logger.info("FaceService aceptando conexiones (modelo cargando en background)...")

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
