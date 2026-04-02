"""
Configuración centralizada del servicio.
Todas las variables son inyectables por entorno (.env) o por defecto razonables.
"""

from pydantic_settings import BaseSettings
from functools import lru_cache


class Settings(BaseSettings):
    # ── Servidor ──────────────────────────────────────────────────────────
    host: str = "0.0.0.0"
    port: int = 5001
    workers: int = 1  # 1 worker para evitar cargar modelo N veces en RAM

    # ── Modelo ────────────────────────────────────────────────────────────
    detection_model: str = "buffalo_l"  # InsightFace model pack
    detection_size: tuple[int, int] = (640, 640)
    gpu_id: int = -1  # -1 = CPU, 0+ = GPU index

    # ── Umbrales ──────────────────────────────────────────────────────────
    similarity_threshold: float = 0.60
    min_face_size: int = 40  # px — descarta rostros demasiado pequeños
    max_faces_per_image: int = 1  # para marcaje: esperamos 1 persona

    # ── Seguridad ─────────────────────────────────────────────────────────
    api_key: str = ""  # Vacío = sin autenticación (desarrollo)
    allowed_origins: list[str] = ["*"]

    # ── Logging ───────────────────────────────────────────────────────────
    log_level: str = "INFO"

    model_config = {"env_prefix": "FACE_", "env_file": ".env"}


@lru_cache(maxsize=1)
def get_settings() -> Settings:
    return Settings()
