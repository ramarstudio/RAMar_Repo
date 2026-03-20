"""
face_detector.py – Detección de rostros sobre imágenes NumPy/OpenCV.

Pipeline interno:
    imagen (BGR numpy) ──► validación ──► conversión BGR→RGB
        ──► dlib HOG/CNN (vía face_recognition) ──► lista [{x,y,w,h}]

Uso básico:
    detector = FaceDetector()
    faces = detector.detect(cv2_frame)
"""

from __future__ import annotations

import logging
from dataclasses import dataclass
from typing import Sequence

import cv2
import face_recognition
import numpy as np

from facial_service.config import settings

logger = logging.getLogger(__name__)


# ── Tipos ─────────────────────────────────────────────────────────────────────

@dataclass(frozen=True, slots=True)
class FaceLocation:
    """Bounding box de un rostro detectado en la imagen."""
    x: int   # columna del borde izquierdo
    y: int   # fila del borde superior
    w: int   # ancho  (x + w = borde derecho)
    h: int   # alto   (y + h = borde inferior)

    def to_dict(self) -> dict[str, int]:
        return {"x": self.x, "y": self.y, "w": self.w, "h": self.h}

    @property
    def area(self) -> int:
        return self.w * self.h


# ── Excepciones ───────────────────────────────────────────────────────────────

class InvalidImageError(ValueError):
    """La imagen recibida es None, está vacía o tiene un formato inesperado."""


# ── Validación ────────────────────────────────────────────────────────────────

# Tamaño mínimo en píxeles para que la detección tenga sentido
_MIN_DIM = 32
# Tamaño máximo antes de redimensionar para acelerar HOG en CPU
_MAX_LONG_SIDE = 1280


def _validate(image: np.ndarray) -> None:
    """Lanza InvalidImageError si la imagen no es utilizable."""
    if image is None:
        raise InvalidImageError("La imagen recibida es None.")

    if not isinstance(image, np.ndarray):
        raise InvalidImageError(
            f"Se esperaba numpy.ndarray, se recibió {type(image).__name__}."
        )

    if image.size == 0:
        raise InvalidImageError("La imagen está vacía (0 elementos).")

    if image.ndim not in (2, 3):
        raise InvalidImageError(
            f"Dimensiones inesperadas: {image.ndim}D. Se esperan 2 (gris) o 3 (color)."
        )

    h, w = image.shape[:2]
    if h < _MIN_DIM or w < _MIN_DIM:
        raise InvalidImageError(
            f"Imagen demasiado pequeña ({w}×{h}). Mínimo requerido: {_MIN_DIM}px."
        )


# ── Preprocesado ──────────────────────────────────────────────────────────────

def _to_rgb(image: np.ndarray) -> np.ndarray:
    """
    Convierte la imagen al espacio RGB que espera face_recognition.

    - BGR  (3 canales, OpenCV por defecto) → RGB
    - BGRA (4 canales, e.g. PNG con alfa)  → RGB
    - Gris (1 canal o 2D)                  → RGB
    - RGB  (ya correcto)                   → devuelve como está
    """
    if image.ndim == 2 or (image.ndim == 3 and image.shape[2] == 1):
        return cv2.cvtColor(image, cv2.COLOR_GRAY2RGB)

    if image.shape[2] == 4:
        return cv2.cvtColor(image, cv2.COLOR_BGRA2RGB)

    # Heurística: si viene de OpenCV asumimos BGR
    return cv2.cvtColor(image, cv2.COLOR_BGR2RGB)


def _maybe_downscale(image: np.ndarray) -> tuple[np.ndarray, float]:
    """
    Reduce la imagen si el lado largo supera _MAX_LONG_SIDE.

    Retorna (imagen_redimensionada, escala) donde escala ∈ (0, 1].
    Las coordenadas de detección deben dividirse por escala después.
    """
    h, w = image.shape[:2]
    long_side = max(h, w)

    if long_side <= _MAX_LONG_SIDE:
        return image, 1.0

    scale = _MAX_LONG_SIDE / long_side
    new_w = int(w * scale)
    new_h = int(h * scale)
    resized = cv2.resize(image, (new_w, new_h), interpolation=cv2.INTER_AREA)
    logger.debug("Imagen redimensionada %dx%d → %dx%d (escala %.2f)", w, h, new_w, new_h, scale)
    return resized, scale


# ── Clase principal ───────────────────────────────────────────────────────────

class FaceDetector:
    """
    Detecta rostros en imágenes NumPy usando dlib HOG o CNN.

    Args:
        model:         'hog' (CPU, rápido) o 'cnn' (GPU, preciso).
                       Por defecto toma el valor de settings.model.
        upsample_times: Número de veces que dlib aumenta la resolución
                        antes de buscar rostros. Más alto detecta caras
                        pequeñas, pero es más lento. Default: 1.
    """

    def __init__(
        self,
        model: str | None = None,
        upsample_times: int = 1,
    ) -> None:
        self._model = (model or settings.model).lower()
        self._upsample = upsample_times

        if self._model not in {"hog", "cnn"}:
            raise ValueError(f"model inválido: '{self._model}'. Use 'hog' o 'cnn'.")

        logger.info(
            "FaceDetector inicializado — model=%s, upsample=%d",
            self._model, self._upsample,
        )

    # ── API pública ───────────────────────────────────────────────────────────

    def detect(
        self,
        image: np.ndarray,
        *,
        bgr: bool = True,
    ) -> list[dict[str, int]]:
        """
        Detecta todos los rostros en `image`.

        Args:
            image: Array NumPy con la imagen. OpenCV produce BGR por defecto;
                   pasa bgr=False si ya está en RGB.
            bgr:   Indica si los canales están en orden BGR (True, default)
                   o RGB (False).

        Returns:
            Lista de diccionarios ``{'x': int, 'y': int, 'w': int, 'h': int}``,
            uno por rostro detectado. Lista vacía si no se detecta ninguno.

        Raises:
            InvalidImageError: Si la imagen es None, está vacía o tiene
                               un formato incompatible.
        """
        _validate(image)

        rgb_image = _to_rgb(image) if bgr else image.copy()
        rgb_image, scale = _maybe_downscale(rgb_image)

        try:
            # face_recognition retorna [(top, right, bottom, left), ...]
            raw_locations: Sequence[tuple[int, int, int, int]] = (
                face_recognition.face_locations(
                    rgb_image,
                    model=self._model,
                    number_of_times_to_upsample=self._upsample,
                )
            )
        except Exception as exc:
            logger.exception("Error interno durante la detección de rostros.")
            raise RuntimeError(f"Fallo en la detección de rostros: {exc}") from exc

        faces = [
            self._to_face_location(top, right, bottom, left, scale).to_dict()
            for top, right, bottom, left in raw_locations
        ]

        logger.debug(
            "Detectados %d rostro(s) — model=%s", len(faces), self._model
        )
        return faces

    def detect_locations(
        self,
        image: np.ndarray,
        *,
        bgr: bool = True,
    ) -> list[FaceLocation]:
        """
        Igual que detect() pero retorna objetos FaceLocation tipados
        en lugar de dicts. Útil para uso interno (face_encoder.py).
        """
        raw = self.detect(image, bgr=bgr)
        return [FaceLocation(**d) for d in raw]

    # ── Helpers privados ──────────────────────────────────────────────────────

    @staticmethod
    def _to_face_location(
        top: int, right: int, bottom: int, left: int, scale: float
    ) -> FaceLocation:
        """
        Convierte el formato (top, right, bottom, left) de dlib al formato
        {x, y, w, h} estándar, reescalando si la imagen fue reducida.
        """
        if scale != 1.0:
            top    = int(top    / scale)
            right  = int(right  / scale)
            bottom = int(bottom / scale)
            left   = int(left   / scale)

        return FaceLocation(
            x=left,
            y=top,
            w=right - left,
            h=bottom - top,
        )


# ── Instancia de conveniencia (opcional) ─────────────────────────────────────

# Permite uso directo:  from facial_service.face_detector import detector
# El objeto se crea una sola vez al importar el módulo (patrón singleton ligero).
detector = FaceDetector()
