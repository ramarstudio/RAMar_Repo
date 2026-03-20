"""
app.py – Punto de entrada del microservicio de reconocimiento facial.

Expone cuatro endpoints sobre HTTP local para la aplicación C# WPF:

    POST /api/detect   – Detecta bounding-boxes de rostros en una imagen.
    POST /api/encode   – Extrae el embedding 128-D del rostro principal.
    POST /api/match    – Compara un embedding contra candidatos enviados por el cliente.
    GET  /api/health   – Sondeo de disponibilidad del servicio.

Arranque:
    python -m facial_service.app
    uvicorn facial_service.app:app --host 127.0.0.1 --port 5001
"""

from __future__ import annotations

import base64
import logging
import logging.config
import sys
from contextlib import asynccontextmanager
from typing import Any

import cv2
import numpy as np
import uvicorn
from fastapi import FastAPI, Request, status
from fastapi.responses import JSONResponse
from pydantic import BaseModel, Field, field_validator

from facial_service.config import settings
from facial_service.face_detector import FaceDetector, InvalidImageError
from facial_service.face_encoder import EncodingError, NoFaceDetectedError, encode_face
from facial_service.face_matcher import (
    InvalidEmbeddingError,
    NoCandidatesError,
    match_face,
)

# ── Logging ───────────────────────────────────────────────────────────────────

def _configure_logging() -> None:
    logging.config.dictConfig(
        {
            "version": 1,
            "disable_existing_loggers": False,
            "formatters": {
                "default": {
                    "format": "%(asctime)s | %(levelname)-8s | %(name)s | %(message)s",
                    "datefmt": "%Y-%m-%d %H:%M:%S",
                }
            },
            "handlers": {
                "console": {
                    "class": "logging.StreamHandler",
                    "stream": "ext://sys.stdout",
                    "formatter": "default",
                }
            },
            "root": {
                "level": "DEBUG" if settings.debug else "INFO",
                "handlers": ["console"],
            },
        }
    )


_configure_logging()
logger = logging.getLogger(__name__)


# ── Ciclo de vida ─────────────────────────────────────────────────────────────

@asynccontextmanager
async def lifespan(app: FastAPI):
    """Inicializa recursos al arrancar y los libera al apagar."""
    logger.info(
        "Microservicio de reconocimiento facial iniciando en %s (model=%s, tolerance=%.2f)",
        settings.base_url,
        settings.model,
        settings.tolerance,
    )
    # Instancia compartida del detector (carga el modelo dlib una sola vez)
    app.state.detector = FaceDetector()
    yield
    logger.info("Microservicio detenido.")


# ── Aplicación ────────────────────────────────────────────────────────────────

app = FastAPI(
    title="Facial Recognition Service",
    version="1.0.0",
    description="Microservicio local de reconocimiento facial para el Sistema de Asistencia.",
    docs_url="/docs" if settings.debug else None,   # Swagger solo en modo debug
    redoc_url=None,
    lifespan=lifespan,
)


# ── Manejadores de excepción centralizados ────────────────────────────────────
# Mapean las excepciones de dominio a códigos HTTP sin try/except en cada endpoint.

def _error_body(exc: Exception) -> dict[str, str]:
    return {"detail": str(exc), "error": type(exc).__name__}


@app.exception_handler(InvalidImageError)
async def handle_invalid_image(request: Request, exc: InvalidImageError) -> JSONResponse:
    return JSONResponse(status_code=status.HTTP_422_UNPROCESSABLE_ENTITY, content=_error_body(exc))


@app.exception_handler(NoFaceDetectedError)
async def handle_no_face(request: Request, exc: NoFaceDetectedError) -> JSONResponse:
    return JSONResponse(status_code=status.HTTP_422_UNPROCESSABLE_ENTITY, content=_error_body(exc))


@app.exception_handler(InvalidEmbeddingError)
async def handle_invalid_embedding(request: Request, exc: InvalidEmbeddingError) -> JSONResponse:
    return JSONResponse(status_code=status.HTTP_422_UNPROCESSABLE_ENTITY, content=_error_body(exc))


@app.exception_handler(NoCandidatesError)
async def handle_no_candidates(request: Request, exc: NoCandidatesError) -> JSONResponse:
    return JSONResponse(status_code=status.HTTP_422_UNPROCESSABLE_ENTITY, content=_error_body(exc))


@app.exception_handler(EncodingError)
async def handle_encoding_error(request: Request, exc: EncodingError) -> JSONResponse:
    logger.exception("EncodingError no controlado.")
    return JSONResponse(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR, content=_error_body(exc))


@app.exception_handler(RuntimeError)
async def handle_runtime_error(request: Request, exc: RuntimeError) -> JSONResponse:
    logger.exception("RuntimeError no controlado.")
    return JSONResponse(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR, content=_error_body(exc))


# ── Schemas Pydantic ──────────────────────────────────────────────────────────

class ImageRequest(BaseModel):
    """Body común para endpoints que reciben una imagen codificada en base64."""

    image_base64: str = Field(
        ...,
        description="Imagen codificada en Base64 (JPEG o PNG). "
                    f"Tamaño máximo descomprimido: {settings.max_image_size_mb} MB.",
    )

    @field_validator("image_base64")
    @classmethod
    def validate_base64_size(cls, v: str) -> str:
        # Estimación rápida del tamaño sin decodificar: cada 4 chars ≈ 3 bytes
        estimated_bytes = len(v) * 3 // 4
        if estimated_bytes > settings.max_image_bytes:
            raise ValueError(
                f"La imagen supera el tamaño máximo permitido "
                f"({settings.max_image_size_mb} MB)."
            )
        return v


class CandidateSchema(BaseModel):
    empleado_id: int
    embedding: list[float] = Field(..., min_length=128, max_length=128)


class MatchRequest(BaseModel):
    embedding: list[float] = Field(..., min_length=128, max_length=128)
    candidates: list[CandidateSchema] = Field(..., min_length=1)
    threshold: float | None = Field(
        default=None,
        ge=0.0,
        le=1.0,
        description="Override del umbral de tolerancia. Si es null usa el de config.",
    )


# ── Respuestas ────────────────────────────────────────────────────────────────

class DetectResponse(BaseModel):
    faces: list[dict[str, int]]


class EncodeResponse(BaseModel):
    embedding: list[float]


class MatchResponse(BaseModel):
    match: bool
    empleado_id: int | None
    confidence: float
    distance: float


class HealthResponse(BaseModel):
    status: str
    model: str
    tolerance: float
    version: str


# ── Helper: decodificación de imagen ─────────────────────────────────────────

def _decode_image_rgb(image_base64: str) -> np.ndarray:
    """
    Decodifica una cadena base64 a un array NumPy en RGB.

    Raises:
        InvalidImageError: Si el base64 es inválido o no puede decodificarse
                           como imagen por OpenCV.
    """
    try:
        raw_bytes = base64.b64decode(image_base64, validate=True)
    except Exception as exc:
        raise InvalidImageError(f"Base64 inválido: {exc}") from exc

    buffer = np.frombuffer(raw_bytes, dtype=np.uint8)
    bgr_image = cv2.imdecode(buffer, cv2.IMREAD_COLOR)

    if bgr_image is None:
        raise InvalidImageError(
            "No se pudo decodificar la imagen. "
            "Asegúrese de enviar un JPEG o PNG válido en base64."
        )

    # Convertir BGR (OpenCV) → RGB (face_recognition / dlib)
    return cv2.cvtColor(bgr_image, cv2.COLOR_BGR2RGB)


# ── Endpoints ─────────────────────────────────────────────────────────────────

@app.get(
    "/api/health",
    response_model=HealthResponse,
    summary="Sondeo de disponibilidad",
    tags=["Sistema"],
)
async def health() -> dict[str, Any]:
    """Retorna el estado del servicio y los parámetros activos de configuración."""
    return {
        "status": "ok",
        "model": settings.model,
        "tolerance": settings.tolerance,
        "version": app.version,
    }


@app.post(
    "/api/detect",
    response_model=DetectResponse,
    summary="Detecta rostros en una imagen",
    tags=["Visión Computacional"],
)
async def detect(request: Request, body: ImageRequest) -> dict[str, Any]:
    """
    Recibe una imagen en base64 y devuelve las coordenadas de todos los
    rostros detectados.

    **Response:**
    ```json
    {
        "faces": [
            {"x": 120, "y": 45, "w": 180, "h": 200},
            ...
        ]
    }
    ```
    Lista vacía si no se detecta ningún rostro (no es un error).
    """
    image_rgb = _decode_image_rgb(body.image_base64)
    detector: FaceDetector = request.app.state.detector
    faces = detector.detect(image_rgb, bgr=False)
    logger.info("/api/detect: %d rostro(s) detectados.", len(faces))
    return {"faces": faces}


@app.post(
    "/api/encode",
    response_model=EncodeResponse,
    summary="Extrae el embedding facial 128-D",
    tags=["Visión Computacional"],
)
async def encode(body: ImageRequest) -> dict[str, Any]:
    """
    Recibe una imagen en base64 y devuelve el vector de características
    del rostro más grande detectado.

    **Response:**
    ```json
    { "embedding": [0.0723, -0.1204, ..., 0.0891] }
    ```

    **Errores:**
    - `422` si no se detecta ningún rostro o la imagen es inválida.
    """
    image_rgb = _decode_image_rgb(body.image_base64)
    embedding = encode_face(image_rgb)
    logger.info("/api/encode: embedding generado (dim=%d).", len(embedding))
    return {"embedding": embedding}


@app.post(
    "/api/match",
    response_model=MatchResponse,
    summary="Compara un embedding contra candidatos",
    tags=["Reconocimiento Facial"],
)
async def match(body: MatchRequest) -> dict[str, Any]:
    """
    Compara el embedding capturado en vivo contra la lista de candidatos
    pre-filtrados enviada por el cliente C#.

    La aplicación C# es responsable de consultar la BD y enviar solo los
    candidatos relevantes (ej. empleados del turno activo).

    **Response:**
    ```json
    {
        "match": true,
        "empleado_id": 42,
        "confidence": 0.78,
        "distance": 0.2194
    }
    ```
    `empleado_id` es `null` cuando `match` es `false`.
    """
    candidates_raw = [c.model_dump() for c in body.candidates]
    result = match_face(
        target_embedding=body.embedding,
        candidates=candidates_raw,
        threshold=body.threshold,
    )
    logger.info(
        "/api/match: match=%s | empleado_id=%s | confidence=%.4f",
        result["match"],
        result["empleado_id"],
        result["confidence"],
    )
    return result


# ── Arranque ──────────────────────────────────────────────────────────────────

if __name__ == "__main__":
    uvicorn.run(
        "facial_service.app:app",
        host=settings.host,
        port=settings.port,
        reload=settings.debug,
        log_level="debug" if settings.debug else "info",
        # access_log=False reduce ruido en producción;
        # el logger propio registra lo relevante.
        access_log=settings.debug,
    )
