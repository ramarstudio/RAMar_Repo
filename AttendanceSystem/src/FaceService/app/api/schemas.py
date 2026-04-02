"""
Esquemas Pydantic — contratos de entrada/salida de la API.
Mapean exactamente al contrato esperado por BiometricoService.cs:

  POST /api/encode
    Request:  { image_base64: str }
    Response: { Embedding: list[float] }

  POST /api/verify
    Request:  { image_base64: str, known_embedding: list[float] }
    Response: { Match: bool, Confidence: float }
"""

from pydantic import BaseModel, Field


# ── Encode ────────────────────────────────────────────────────────────────

class EncodeRequest(BaseModel):
    image_base64: str = Field(..., min_length=100, description="Imagen JPEG/PNG en Base64")


class EncodeResponse(BaseModel):
    Embedding: list[float] = Field(..., description="Vector embedding normalizado L2")


# ── Verify ────────────────────────────────────────────────────────────────

class VerifyRequest(BaseModel):
    image_base64: str = Field(..., min_length=100, description="Imagen JPEG/PNG en Base64")
    known_embedding: list[float] = Field(..., min_length=64, description="Embedding almacenado del empleado")


class VerifyResponse(BaseModel):
    Match: bool = Field(..., description="True si la similitud supera el umbral")
    Confidence: float = Field(..., ge=0.0, le=1.0, description="Similitud coseno [0, 1]")


# ── Health ────────────────────────────────────────────────────────────────

class HealthResponse(BaseModel):
    status: str
    model_loaded: bool
    model_name: str
    embedding_dim: int
