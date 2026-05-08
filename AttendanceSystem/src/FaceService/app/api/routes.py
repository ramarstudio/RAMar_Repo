"""
Endpoints REST — capa delgada que orquesta core + adaptadores.
No contiene lógica de negocio, solo traduce HTTP ↔ dominio.
"""

import logging
import numpy as np
from fastapi import APIRouter, HTTPException

from app.api.schemas import (
    EncodeRequest, EncodeResponse,
    VerifyRequest, VerifyResponse,
    HealthResponse,
)
from app.core.interfaces import FaceEngine
from app.core.image_utils import decode_base64_image
from app.core.similarity import cosine_similarity
from app.config import get_settings

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/api")

# Inyectado en main.py al levantar la app
_engine: FaceEngine | None = None


def set_engine(engine: FaceEngine) -> None:
    global _engine
    _engine = engine


def _get_engine() -> FaceEngine:
    if _engine is None:
        raise HTTPException(503, detail="Motor de reconocimiento facial no inicializado.")
    return _engine


# ── POST /api/encode ──────────────────────────────────────────────────────

@router.post("/encode", response_model=EncodeResponse)
async def encode_face(req: EncodeRequest) -> EncodeResponse:
    """
    Recibe imagen → detecta rostro → devuelve embedding 512-d.
    Usado para REGISTRAR un empleado nuevo.
    """
    engine = _get_engine()

    try:
        image = decode_base64_image(req.image_base64)
    except ValueError as exc:
        raise HTTPException(400, detail=str(exc))

    faces = engine.encode(image)

    if not faces:
        raise HTTPException(
            422,
            detail="No se detectó ningún rostro en la imagen. "
                   "Asegúrese de estar frente a la cámara con buena iluminación.",
        )

    settings = get_settings()
    if len(faces) > settings.max_faces_per_image:
        raise HTTPException(
            422,
            detail=f"Se detectaron {len(faces)} rostros. Solo debe haber 1 persona frente a la cámara.",
        )

    # Tomar el rostro con mayor confianza
    best = max(faces, key=lambda f: f.bbox.confidence)

    if best.bbox.confidence < 0.5:
        raise HTTPException(
            422,
            detail="El rostro detectado tiene baja confianza. Mejore la iluminación o acérquese a la cámara.",
        )

    logger.info(
        "Encode exitoso: confidence=%.3f, embedding_dim=%d",
        best.bbox.confidence, len(best.embedding),
    )

    return EncodeResponse(Embedding=best.embedding.tolist())


# ── POST /api/verify ──────────────────────────────────────────────────────

@router.post("/verify", response_model=VerifyResponse)
async def verify_face(req: VerifyRequest) -> VerifyResponse:
    """
    Recibe imagen + embedding conocido → compara → devuelve Match + Confidence.
    Usado para VERIFICAR identidad en cada marcaje.

    Complejidad: O(d) — una sola comparación coseno.
    """
    engine = _get_engine()
    settings = get_settings()

    try:
        image = decode_base64_image(req.image_base64)
    except ValueError as exc:
        raise HTTPException(400, detail=str(exc))

    faces = engine.encode(image)

    if not faces:
        logger.debug("Verify: no se detectó rostro.")
        return VerifyResponse(Match=False, Confidence=0.0)

    # Tomar el rostro con mayor confianza
    best = max(faces, key=lambda f: f.bbox.confidence)
    known = np.array(req.known_embedding, dtype=np.float32)

    similarity = cosine_similarity(best.embedding, known)

    # Clamp a [0, 1] — coseno puede ser negativo en vectores muy distintos
    confidence = max(0.0, min(1.0, similarity))
    is_match = confidence >= settings.similarity_threshold

    logger.info(
        "Verify: match=%s, confidence=%.4f, threshold=%.2f",
        is_match, confidence, settings.similarity_threshold,
    )

    return VerifyResponse(Match=is_match, Confidence=round(confidence, 6))


# ── GET /api/health ───────────────────────────────────────────────────────

@router.get("/health", response_model=HealthResponse)
async def health() -> HealthResponse:
    # Siempre devuelve 200 — el campo model_loaded indica si el modelo ya cargo.
    # Esto permite que el cliente C# detecte el arranque del servidor
    # desde el primer segundo, sin esperar a que el modelo termine de cargarse.
    ready = _engine is not None and _engine.is_ready()
    settings = get_settings()
    return HealthResponse(
        status="ok" if ready else "loading",
        model_loaded=ready,
        model_name=settings.detection_model,
        embedding_dim=512,
    )
