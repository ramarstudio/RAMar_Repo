"""
config.py – Configuración central del microservicio de reconocimiento facial.

Todas las variables pueden sobreescribirse mediante variables de entorno
o un archivo .env en la raíz del proyecto (cargado automáticamente por
pydantic-settings).

Ejemplo de .env:
    FACE_SERVICE_PORT=5001
    FACE_SERVICE_TOLERANCE=0.55
"""

from pydantic import Field, field_validator
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Configuración del servidor y del motor de reconocimiento facial."""

    model_config = SettingsConfigDict(
        env_prefix="FACE_SERVICE_",   # todas las vars de entorno usan este prefijo
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=False,
    )

    # ── Servidor ─────────────────────────────────────────────────────────────
    host: str = Field(
        default="127.0.0.1",
        description="Interfaz de red en la que escucha el servidor. "
                    "Usa 127.0.0.1 para comunicación local con el cliente WPF.",
    )
    port: int = Field(
        default=5001,
        ge=1024,
        le=65535,
        description="Puerto TCP del servidor FastAPI.",
    )
    debug: bool = Field(
        default=False,
        description="Activa recarga automática y logs detallados. "
                    "Nunca usar True en producción.",
    )

    # ── Reconocimiento facial ─────────────────────────────────────────────────
    tolerance: float = Field(
        default=0.6,
        ge=0.0,
        le=1.0,
        description="Umbral de distancia euclidiana para considerar dos rostros "
                    "como la misma persona. Valores más bajos = mayor estrictez. "
                    "Rango recomendado: 0.5 (estricto) – 0.65 (permisivo).",
    )
    model: str = Field(
        default="hog",
        description="Modelo de detección de rostros: 'hog' (CPU, rápido) "
                    "o 'cnn' (GPU, más preciso).",
    )
    num_jitters: int = Field(
        default=1,
        ge=1,
        description="Número de veces que se recalcula el encoding por imagen. "
                    "Valores altos mejoran precisión a costa de velocidad.",
    )
    encodings_path: str = Field(
        default="data/encodings.pkl",
        description="Ruta al archivo donde se persisten los encodings faciales "
                    "registrados.",
    )

    # ── API ───────────────────────────────────────────────────────────────────
    max_image_size_mb: float = Field(
        default=5.0,
        gt=0,
        description="Tamaño máximo permitido para imágenes recibidas (en MB).",
    )

    @field_validator("model")
    @classmethod
    def validate_model(cls, v: str) -> str:
        allowed = {"hog", "cnn"}
        if v.lower() not in allowed:
            raise ValueError(f"model debe ser uno de {allowed}, recibido: '{v}'")
        return v.lower()

    @property
    def max_image_bytes(self) -> int:
        return int(self.max_image_size_mb * 1024 * 1024)

    @property
    def base_url(self) -> str:
        return f"http://{self.host}:{self.port}"


# Instancia única – importar desde cualquier módulo:
#   from facial_service.config import settings
settings = Settings()
