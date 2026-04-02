"""
Utilidades de imagen: decodificación Base64 → NumPy array.
Aislado para que la capa de API no conozca OpenCV.
"""

import base64
import numpy as np
import cv2


def decode_base64_image(b64_string: str) -> np.ndarray:
    """
    Decodifica imagen Base64 (JPEG/PNG) a array BGR (formato OpenCV).
    Valida integridad del payload antes de procesar.

    Raises:
        ValueError: si el payload está vacío, corrupto o no es imagen válida.
    """
    if not b64_string:
        raise ValueError("Imagen Base64 vacía.")

    # Limpiar posible prefijo data:image/...;base64,
    if "," in b64_string[:80]:
        b64_string = b64_string.split(",", 1)[1]

    try:
        raw_bytes = base64.b64decode(b64_string, validate=True)
    except Exception as exc:
        raise ValueError(f"Base64 inválido: {exc}") from exc

    if len(raw_bytes) < 100:
        raise ValueError("Payload demasiado pequeño para ser una imagen.")

    buf = np.frombuffer(raw_bytes, dtype=np.uint8)
    image = cv2.imdecode(buf, cv2.IMREAD_COLOR)

    if image is None:
        raise ValueError("No se pudo decodificar la imagen (formato no soportado o corrupto).")

    return image
