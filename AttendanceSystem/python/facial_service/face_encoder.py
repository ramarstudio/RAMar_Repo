"""
face_encoder.py – Extracción del embedding facial (vector 128-D).

Pipeline:
    imagen RGB ──► detección de ubicaciones (dlib)
               ──► selección del rostro más grande
               ──► cálculo del encoding (face_recognition)
               ──► list[float] serializable a JSON
"""

from __future__ import annotations

import logging

import face_recognition
import numpy as np

from facial_service.config import settings
from facial_service.face_detector import FaceLocation, InvalidImageError, _validate

logger = logging.getLogger(__name__)

# Dimensión garantizada por face_recognition / dlib
EMBEDDING_DIM = 128


# ── Excepciones ───────────────────────────────────────────────────────────────

class NoFaceDetectedError(ValueError):
    """No se encontró ningún rostro en la imagen proporcionada."""


class EncodingError(RuntimeError):
    """Fallo interno al calcular el embedding facial."""


# ── Helpers privados ──────────────────────────────────────────────────────────

def _largest_face(
    locations: list[tuple[int, int, int, int]],
) -> tuple[int, int, int, int]:
    """
    De una lista de ubicaciones (top, right, bottom, left) devuelve
    la que tiene mayor área. Criterio más robusto que tomar el índice 0,
    especialmente en capturas grupales donde el sujeto real no siempre
    aparece primero en la lista de dlib.
    """
    return max(locations, key=lambda loc: (loc[1] - loc[3]) * (loc[2] - loc[0]))


def _location_to_face_location(loc: tuple[int, int, int, int]) -> FaceLocation:
    top, right, bottom, left = loc
    return FaceLocation(x=left, y=top, w=right - left, h=bottom - top)


# ── API pública ───────────────────────────────────────────────────────────────

def encode_face(
    image: np.ndarray,
    *,
    model: str | None = None,
    num_jitters: int | None = None,
) -> list[float]:
    """
    Extrae el embedding 128-D del rostro más grande presente en `image`.

    Args:
        image:       Array NumPy en formato RGB, shape (H, W, 3), dtype uint8.
        model:       Modelo de detección: 'hog' (default, CPU) o 'cnn' (GPU).
                     Si es None usa settings.model.
        num_jitters: Veces que se recalcula el encoding para mayor precisión.
                     Si es None usa settings.num_jitters.

    Returns:
        Lista de 128 floats (Python nativos), lista directamente serializable
        con json.dumps() o como campo Pydantic.

    Raises:
        InvalidImageError:   La imagen es None, vacía o tiene formato incorrecto.
        NoFaceDetectedError: No se detectó ningún rostro.
        EncodingError:       Fallo interno de dlib al calcular el embedding.
    """
    _validate(image)

    _model       = (model or settings.model).lower()
    _num_jitters = num_jitters if num_jitters is not None else settings.num_jitters

    # ── 1. Detectar ubicaciones ───────────────────────────────────────────────
    try:
        locations: list[tuple[int, int, int, int]] = face_recognition.face_locations(
            image,
            model=_model,
            number_of_times_to_upsample=1,
        )
    except Exception as exc:
        raise EncodingError(f"Error en la detección de ubicaciones: {exc}") from exc

    if not locations:
        logger.debug("encode_face: sin rostros detectados en la imagen.")
        raise NoFaceDetectedError(
            "No se detectó ningún rostro en la imagen. "
            "Verifique iluminación, orientación y resolución."
        )

    # ── 2. Seleccionar el rostro más grande ───────────────────────────────────
    target_location = _largest_face(locations)

    if len(locations) > 1:
        selected = _location_to_face_location(target_location)
        logger.debug(
            "encode_face: %d rostros detectados; se usa el más grande (%dx%d px).",
            len(locations),
            selected.w,
            selected.h,
        )

    # ── 3. Calcular encoding ──────────────────────────────────────────────────
    try:
        encodings: list[np.ndarray] = face_recognition.face_encodings(
            image,
            known_face_locations=[target_location],
            num_jitters=_num_jitters,
            model="large",   # "large" = red ResNet de 128-D (más precisa que "small")
        )
    except Exception as exc:
        raise EncodingError(f"Error al calcular el encoding facial: {exc}") from exc

    # face_encodings puede devolver lista vacía aunque se le pase una ubicación
    # válida (muy raro, pero ocurre con imágenes de baja calidad o recortes extremos)
    if not encodings:
        raise NoFaceDetectedError(
            "La ubicación del rostro fue detectada pero el encoding no pudo "
            "calcularse. La región facial puede estar demasiado borrosa o recortada."
        )

    embedding: np.ndarray = encodings[0]

    assert embedding.shape == (EMBEDDING_DIM,), (
        f"Dimensión inesperada del embedding: {embedding.shape}"
    )

    logger.debug("encode_face: embedding calculado (dim=%d).", EMBEDDING_DIM)

    # ── 4. Serializar a lista Python ──────────────────────────────────────────
    return embedding.tolist()
