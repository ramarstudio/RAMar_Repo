"""
Funciones de similitud entre embeddings.
Operaciones vectorizadas con NumPy — O(d) por par, O(n·d) para búsqueda 1:N.
Independiente de hardware/compilador: puro álgebra lineal.
"""

import numpy as np


def cosine_similarity(a: np.ndarray, b: np.ndarray) -> float:
    """
    Similitud coseno entre dos vectores.
    Asume vectores ya normalizados L2 (como ArcFace).
    Si no están normalizados, se normaliza sobre la marcha.

    Complejidad: O(d) donde d = dimensión del embedding.
    """
    norm_a = np.linalg.norm(a)
    norm_b = np.linalg.norm(b)

    if norm_a < 1e-10 or norm_b < 1e-10:
        return 0.0

    return float(np.dot(a, b) / (norm_a * norm_b))


def cosine_similarity_batch(query: np.ndarray, gallery: np.ndarray) -> np.ndarray:
    """
    Similitud coseno de un vector query contra N vectores en gallery.
    Vectorizado — una sola operación matricial, no un loop.

    Args:
        query:   shape (d,)
        gallery: shape (n, d)

    Returns:
        shape (n,) — similitudes en rango [-1, 1]

    Complejidad: O(n·d) — escala linealmente con la cantidad de empleados.
    """
    if gallery.ndim == 1:
        gallery = gallery.reshape(1, -1)

    # Normalizar
    query_norm = query / (np.linalg.norm(query) + 1e-10)
    gallery_norms = np.linalg.norm(gallery, axis=1, keepdims=True) + 1e-10
    gallery_normalized = gallery / gallery_norms

    # Producto punto vectorizado: (n, d) @ (d,) → (n,)
    return gallery_normalized @ query_norm


def find_best_match(
    query: np.ndarray,
    gallery: np.ndarray,
    threshold: float = 0.60,
) -> tuple[int, float]:
    """
    Encuentra el embedding más similar en el gallery que supere el umbral.

    Returns:
        (index, similarity) — index=-1 si ninguno supera el umbral.
    """
    if gallery.size == 0:
        return -1, 0.0

    similarities = cosine_similarity_batch(query, gallery)
    best_idx = int(np.argmax(similarities))
    best_sim = float(similarities[best_idx])

    if best_sim < threshold:
        return -1, best_sim

    return best_idx, best_sim
