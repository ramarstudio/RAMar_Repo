"""
Middleware de seguridad: API key validation + rate limiting básico.
"""

import time
import logging
from collections import defaultdict
from fastapi import Request, HTTPException
from starlette.middleware.base import BaseHTTPMiddleware

from app.config import get_settings

logger = logging.getLogger(__name__)


class ApiKeyMiddleware(BaseHTTPMiddleware):
    """
    Valida header X-API-Key contra la clave configurada.
    Si api_key está vacío en settings → bypass (modo desarrollo).
    """

    async def dispatch(self, request: Request, call_next):
        settings = get_settings()

        # Sin API key configurada → modo desarrollo, todo pasa
        if not settings.api_key:
            return await call_next(request)

        # Health endpoint siempre accesible
        if request.url.path.endswith("/health"):
            return await call_next(request)

        key = request.headers.get("X-API-Key", "")
        if key != settings.api_key:
            logger.warning("API key inválida desde %s", request.client.host if request.client else "unknown")
            raise HTTPException(401, detail="API key inválida o ausente.")

        return await call_next(request)


class RateLimitMiddleware(BaseHTTPMiddleware):
    """
    Rate limiting por IP — ventana deslizante simple.
    Protege contra abuso sin complejidad de Redis.
    """

    def __init__(self, app, max_requests: int = 60, window_seconds: int = 60):
        super().__init__(app)
        self._max = max_requests
        self._window = window_seconds
        self._requests: dict[str, list[float]] = defaultdict(list)

    async def dispatch(self, request: Request, call_next):
        # El health check es consultado cada 1-2s durante el arranque del modelo.
        # Excluirlo del rate limit evita falsos 429 que el cliente C# interpreta
        # como caída del servicio, lo que provocaría matar y reiniciar el proceso.
        if request.url.path.endswith("/health"):
            return await call_next(request)

        client_ip = request.client.host if request.client else "0.0.0.0"
        now = time.monotonic()

        recent = [t for t in self._requests[client_ip] if now - t < self._window]

        if not recent:
            del self._requests[client_ip]
            self._requests[client_ip] = []
        else:
            self._requests[client_ip] = recent

        if len(self._requests[client_ip]) >= self._max:
            raise HTTPException(429, detail="Demasiadas solicitudes. Intente de nuevo mas tarde.")

        self._requests[client_ip].append(now)
        return await call_next(request)
