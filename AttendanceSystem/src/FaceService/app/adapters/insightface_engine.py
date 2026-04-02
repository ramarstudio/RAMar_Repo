"""
Adaptador concreto: InsightFace (ArcFace + RetinaFace).
Implementa FaceEngine — es intercambiable por cualquier otro motor
que cumpla la misma interfaz.

Modelo: buffalo_l
  - Detección: RetinaFace (mAP 96.5% en WiderFace)
  - Reconocimiento: ArcFace (99.8% en LFW benchmark)
  - Embedding: 512 dimensiones, normalizado L2
"""

import logging
import threading
import numpy as np

from app.core.interfaces import FaceEngine, FaceData, BoundingBox

logger = logging.getLogger(__name__)


class InsightFaceEngine(FaceEngine):
    """
    Singleton thread-safe: el modelo se carga UNA sola vez en memoria.
    Subsequent calls reutilizan la instancia.
    """

    _instance: "InsightFaceEngine | None" = None
    _lock = threading.Lock()

    def __new__(cls, *args, **kwargs):
        if cls._instance is None:
            with cls._lock:
                if cls._instance is None:
                    cls._instance = super().__new__(cls)
        return cls._instance

    def __init__(
        self,
        model_name: str = "buffalo_l",
        det_size: tuple[int, int] = (640, 640),
        gpu_id: int = -1,
    ):
        if hasattr(self, "_initialized"):
            return
        self._initialized = True
        self._model_name = model_name
        self._det_size = det_size
        self._gpu_id = gpu_id
        self._app = None
        self._load_lock = threading.Lock()

    def _ensure_loaded(self) -> None:
        """Carga lazy del modelo — solo cuando se necesita por primera vez."""
        if self._app is not None:
            return

        with self._load_lock:
            if self._app is not None:
                return

            import insightface

            logger.info(
                "Cargando modelo %s (det_size=%s, gpu=%d)...",
                self._model_name, self._det_size, self._gpu_id,
            )
            self._app = insightface.app.FaceAnalysis(
                name=self._model_name,
                providers=self._get_providers(),
            )
            self._app.prepare(ctx_id=self._gpu_id, det_size=self._det_size)
            logger.info("Modelo cargado correctamente.")

    def _get_providers(self) -> list[str]:
        """Selecciona provider ONNX según disponibilidad de GPU."""
        if self._gpu_id >= 0:
            try:
                import onnxruntime
                available = onnxruntime.get_available_providers()
                if "CUDAExecutionProvider" in available:
                    return ["CUDAExecutionProvider", "CPUExecutionProvider"]
            except ImportError:
                pass
        return ["CPUExecutionProvider"]

    def is_ready(self) -> bool:
        return self._app is not None

    def detect(self, image: np.ndarray) -> list[BoundingBox]:
        self._ensure_loaded()
        faces = self._app.get(image)

        h, w = image.shape[:2]
        results = []
        for face in faces:
            x1, y1, x2, y2 = face.bbox
            results.append(BoundingBox(
                x1=float(x1 / w),
                y1=float(y1 / h),
                x2=float(x2 / w),
                y2=float(y2 / h),
                confidence=float(face.det_score),
            ))
        return results

    def encode(self, image: np.ndarray) -> list[FaceData]:
        """
        Pipeline completo: detectar → alinear → generar embedding.
        InsightFace hace las 3 fases internamente en app.get().

        Returns:
            Lista de FaceData con embedding normalizado L2 (512-d).
        """
        self._ensure_loaded()
        faces = self._app.get(image)

        h, w = image.shape[:2]
        results = []

        for face in faces:
            if face.embedding is None:
                continue

            # Normalizar L2 — garantiza que cosine_similarity sea un dot product
            embedding = face.embedding.astype(np.float32)
            norm = np.linalg.norm(embedding)
            if norm > 1e-10:
                embedding = embedding / norm

            x1, y1, x2, y2 = face.bbox
            bbox = BoundingBox(
                x1=float(x1 / w),
                y1=float(y1 / h),
                x2=float(x2 / w),
                y2=float(y2 / h),
                confidence=float(face.det_score),
            )

            results.append(FaceData(
                bbox=bbox,
                embedding=embedding,
                age=int(face.age) if hasattr(face, "age") else None,
                gender=("M" if face.gender == 1 else "F") if hasattr(face, "gender") else None,
            ))

        return results
