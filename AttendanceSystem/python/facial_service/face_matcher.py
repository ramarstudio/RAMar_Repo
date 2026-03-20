"""
face_matcher.py – Comparación de embeddings faciales.

El microservicio NO accede a la BD. La aplicación C# envía los candidatos
pre-filtrados (ej. empleados activos del turno) junto con el embedding capturado.

Pipeline:
    target_embedding (128-D)
    candidates [{empleado_id, embedding}]
        ──► distancia euclidiana contra cada candidato  (numpy vectorizado)
        ──► selección del mínimo
        ──► comparación contra threshold
        ──► MatchResult {match, empleado_id, confidence, distance}
"""

from __future__ import annotations

import logging
from dataclasses import asdict, dataclass

import numpy as np

from facial_service.config import settings

logger = logging.getLogger(__name__)

EMBEDDING_DIM = 128


# ── Tipos de entrada ──────────────────────────────────────────────────────────

@dataclass(frozen=True, slots=True)
class Candidate:
    """Representa un empleado con su embedding registrado."""
    empleado_id: int
    embedding: list[float]

    def to_array(self) -> np.ndarray:
        arr = np.asarray(self.embedding, dtype=np.float64)
        if arr.shape != (EMBEDDING_DIM,):
            raise ValueError(
                f"empleado_id={self.empleado_id}: embedding con dimensión "
                f"inválida {arr.shape}, se esperaban ({EMBEDDING_DIM},)."
            )
        return arr


# ── Tipo de salida ────────────────────────────────────────────────────────────

@dataclass(frozen=True, slots=True)
class MatchResult:
    """Resultado de la comparación facial."""
    match: bool
    empleado_id: int | None  # None si ningún candidato supera el umbral
    confidence: float        # 1 - distance  ∈ [0.0, 1.0]
    distance: float          # distancia euclidiana al mejor candidato

    def to_dict(self) -> dict:
        return asdict(self)


# ── Excepciones ───────────────────────────────────────────────────────────────

class InvalidEmbeddingError(ValueError):
    """El embedding recibido no tiene el formato o dimensión esperados."""


class NoCandidatesError(ValueError):
    """La lista de candidatos está vacía; no hay con quién comparar."""


# ── Validación ────────────────────────────────────────────────────────────────

def _parse_target(target_embedding: list[float]) -> np.ndarray:
    """Convierte y valida el embedding objetivo a ndarray (64-bit)."""
    if not target_embedding:
        raise InvalidEmbeddingError("target_embedding está vacío.")

    arr = np.asarray(target_embedding, dtype=np.float64)

    if arr.ndim != 1 or arr.shape[0] != EMBEDDING_DIM:
        raise InvalidEmbeddingError(
            f"target_embedding tiene forma {arr.shape}; "
            f"se esperaba ({EMBEDDING_DIM},)."
        )

    if not np.isfinite(arr).all():
        raise InvalidEmbeddingError(
            "target_embedding contiene valores NaN o infinitos."
        )

    return arr


def _parse_candidates(raw: list[dict]) -> list[Candidate]:
    """
    Convierte los dicts del request HTTP en objetos Candidate validados.
    Descarta silenciosamente (con log de advertencia) los que tengan
    estructura o dimensiones incorrectas para no abortar toda la búsqueda.
    """
    if not raw:
        raise NoCandidatesError(
            "La lista de candidatos está vacía. "
            "El cliente debe enviar al menos un candidato."
        )

    valid: list[Candidate] = []
    for item in raw:
        try:
            candidate = Candidate(
                empleado_id=int(item["empleado_id"]),
                embedding=list(item["embedding"]),
            )
            # Fuerza la validación de dimensión dentro del dataclass
            candidate.to_array()
            valid.append(candidate)
        except (KeyError, TypeError, ValueError) as exc:
            logger.warning(
                "Candidato descartado por datos inválidos — %s: %s",
                item.get("empleado_id", "id_desconocido"),
                exc,
            )

    if not valid:
        raise NoCandidatesError(
            "Ningún candidato superó la validación de formato. "
            "Revisa que cada entrada tenga 'empleado_id' y 'embedding' de 128 floats."
        )

    return valid


# ── Núcleo de comparación ─────────────────────────────────────────────────────

def _compute_distances(
    target: np.ndarray,
    candidates: list[Candidate],
) -> np.ndarray:
    """
    Calcula la distancia euclidiana entre `target` y cada candidato
    de forma vectorizada (una sola operación matricial).

    face_recognition.face_distance() hace exactamente esto internamente,
    pero al construir la matriz aquí evitamos el overhead de llamar a la
    librería en un loop y mantenemos independencia del import pesado en
    el módulo de matching (que no necesita dlib para comparar).

        dist[i] = ‖target − candidate[i]‖₂

    Los embeddings de face_recognition están normalizados a longitud ~1,
    por lo que la distancia euclidiana y la distancia coseno son equivalentes.
    """
    matrix = np.stack([c.to_array() for c in candidates])  # shape: (N, 128)
    diff   = matrix - target                                 # broadcasting
    return np.sqrt((diff ** 2).sum(axis=1))                  # shape: (N,)


# ── API pública ───────────────────────────────────────────────────────────────

def match_face(
    target_embedding: list[float],
    candidates: list[dict],
    threshold: float | None = None,
) -> dict:
    """
    Compara `target_embedding` contra los candidatos y retorna el mejor match.

    Args:
        target_embedding: Vector 128-D del rostro capturado (lista de floats).
        candidates:       Lista de dicts ``{'empleado_id': int, 'embedding': list[float]}``.
                          Proviene directamente del body del request HTTP.
        threshold:        Umbral de distancia euclidiana. Distancias menores a este
                          valor se consideran match. Si es None usa settings.tolerance.

    Returns:
        Diccionario serializable::

            {
                "match":       bool,
                "empleado_id": int | None,
                "confidence":  float,   # 1 - distance, clipped a [0.0, 1.0]
                "distance":    float,
            }

    Raises:
        InvalidEmbeddingError: target_embedding inválido.
        NoCandidatesError:     Lista vacía o todos los candidatos con formato incorrecto.
    """
    _threshold = threshold if threshold is not None else settings.tolerance

    # ── Validar entradas ──────────────────────────────────────────────────────
    target     = _parse_target(target_embedding)
    valid_cands = _parse_candidates(candidates)

    # ── Calcular distancias vectorizado ───────────────────────────────────────
    distances = _compute_distances(target, valid_cands)

    best_idx      = int(np.argmin(distances))
    best_distance = float(distances[best_idx])
    best_candidate = valid_cands[best_idx]

    is_match   = best_distance < _threshold
    # Clamp a [0, 1]: con threshold=0.6 una distancia=0 → confidence=1.0
    confidence = float(np.clip(1.0 - best_distance, 0.0, 1.0))

    result = MatchResult(
        match=is_match,
        empleado_id=best_candidate.empleado_id if is_match else None,
        confidence=confidence,
        distance=round(best_distance, 6),
    )

    logger.debug(
        "match_face: mejor candidato id=%d | dist=%.4f | threshold=%.2f | match=%s",
        best_candidate.empleado_id,
        best_distance,
        _threshold,
        is_match,
    )

    return result.to_dict()
