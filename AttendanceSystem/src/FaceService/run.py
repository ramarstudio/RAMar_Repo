"""
Script de inicio directo: python run.py
Alternativa a: uvicorn app.main:app --host 0.0.0.0 --port 5001
"""

import uvicorn
from app.config import get_settings


def main():
    settings = get_settings()
    uvicorn.run(
        "app.main:app",
        host=settings.host,
        port=settings.port,
        workers=settings.workers,
        log_level=settings.log_level.lower(),
    )


if __name__ == "__main__":
    main()
