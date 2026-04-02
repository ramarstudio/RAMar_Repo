"""
Contratos abstractos del dominio.
Ningún adaptador concreto se importa aquí — solo tipos puros.
Cualquier motor de ML que implemente estas interfaces es intercambiable.
"""

from abc import ABC, abstractmethod
from dataclasses import dataclass
import numpy as np


@dataclass(frozen=True, slots=True)
class BoundingBox:
    """Rectángulo de detección normalizado [0,1] — independiente de resolución."""
    x1: float
    y1: float
    x2: float
    y2: float
    confidence: float


@dataclass(frozen=True, slots=True)
class FaceData:
    """Resultado de procesar un rostro: ubicación + embedding."""
    bbox: BoundingBox
    embedding: np.ndarray  # vector normalizado L2, shape (d,)
    age: int | None = None
    gender: str | None = None


class FaceDetector(ABC):
    """Detecta rostros en una imagen y devuelve bounding boxes."""

    @abstractmethod
    def detect(self, image: np.ndarray) -> list[BoundingBox]:
        ...


class FaceEncoder(ABC):
    """Genera embedding vectorial de un rostro alineado."""

    @abstractmethod
    def encode(self, image: np.ndarray) -> list[FaceData]:
        ...


class FaceEngine(FaceDetector, FaceEncoder):
    """
    Motor completo: detección + encoding en un solo componente.
    La mayoría de frameworks (InsightFace, FaceNet) unifican ambos.
    """

    @abstractmethod
    def is_ready(self) -> bool:
        ...
