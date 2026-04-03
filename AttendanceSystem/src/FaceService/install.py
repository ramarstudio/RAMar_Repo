"""
install.py — Instalador automático de dependencias del FaceService.

Resuelve el problema de que 'insightface' NO tiene wheels oficiales
para Windows en PyPI, lo que causa errores de compilación C++ al
hacer pip install directo.

Este script:
1. Instala todas las dependencias normales desde requirements.txt
2. Detecta la versión de Python
3. Descarga el wheel comunitario correcto de insightface
4. Lo instala automáticamente

Uso:
    python install.py
"""

import subprocess
import sys
import os
import urllib.request
import tempfile

# URL base de los wheels comunitarios de insightface para Windows
WHEEL_BASE_URL = (
    "https://github.com/Gourieff/Assets/raw/main/Insightface"
)

INSIGHTFACE_VERSION = "0.7.3"

# Versiones de Python soportadas (con wheels disponibles)
SUPPORTED_VERSIONS = {"3.10", "3.11", "3.12"}


def get_python_version_tag() -> str:
    """Retorna el tag de versión de Python (ej: '311' para 3.11)."""
    major = sys.version_info.major
    minor = sys.version_info.minor
    return f"{major}{minor}"


def get_python_version_short() -> str:
    """Retorna la versión corta (ej: '3.11')."""
    return f"{sys.version_info.major}.{sys.version_info.minor}"


def run_pip(*args: str) -> None:
    """Ejecuta pip como subproceso."""
    cmd = [sys.executable, "-m", "pip"] + list(args)
    print(f"  → {' '.join(cmd)}")
    subprocess.check_call(cmd)


def install_requirements() -> None:
    """Instala las dependencias desde requirements.txt (excepto insightface)."""
    req_file = os.path.join(os.path.dirname(__file__), "requirements.txt")

    if not os.path.exists(req_file):
        print("ERROR: No se encontró requirements.txt")
        sys.exit(1)

    # Leer requirements y filtrar insightface
    with open(req_file, "r", encoding="utf-8") as f:
        lines = f.readlines()

    filtered = []
    for line in lines:
        stripped = line.strip()
        if stripped and not stripped.startswith("#") and "insightface" not in stripped.lower():
            filtered.append(stripped)

    if filtered:
        print("\n📦 Instalando dependencias base...")
        run_pip("install", *filtered)
    print("✅ Dependencias base instaladas.\n")


def install_insightface() -> None:
    """Descarga e instala el wheel comunitario de insightface."""
    ver_tag = get_python_version_tag()
    ver_short = get_python_version_short()

    if ver_short not in SUPPORTED_VERSIONS:
        print(f"⚠️  Python {ver_short} no tiene wheel de insightface disponible.")
        print(f"    Versiones soportadas: {', '.join(sorted(SUPPORTED_VERSIONS))}")
        print("    Intenta instalar manualmente: pip install insightface==0.7.3")
        return

    wheel_name = f"insightface-{INSIGHTFACE_VERSION}-cp{ver_tag}-cp{ver_tag}-win_amd64.whl"
    wheel_url = f"{WHEEL_BASE_URL}/{wheel_name}"

    print(f"🔍 Python {ver_short} detectado.")
    print(f"📥 Descargando insightface wheel: {wheel_name}")

    # Descargar a un directorio temporal
    tmp_dir = tempfile.mkdtemp()
    wheel_path = os.path.join(tmp_dir, wheel_name)

    try:
        urllib.request.urlretrieve(wheel_url, wheel_path)
        print(f"✅ Descarga completada ({os.path.getsize(wheel_path) // 1024} KB)")
    except Exception as e:
        print(f"❌ Error al descargar: {e}")
        print(f"   URL: {wheel_url}")
        print(f"   Descárgalo manualmente y ejecuta: pip install {wheel_name}")
        return

    # Instalar el wheel
    print("📦 Instalando insightface...")
    try:
        run_pip("install", wheel_path)
        print("✅ insightface instalado correctamente.\n")
    except subprocess.CalledProcessError:
        print(f"❌ Error al instalar. Intenta manualmente: pip install {wheel_path}")
    finally:
        # Limpiar archivo temporal
        try:
            os.remove(wheel_path)
            os.rmdir(tmp_dir)
        except OSError:
            pass


def main() -> None:
    print("=" * 60)
    print("  FaceService — Instalador de Dependencias")
    print("=" * 60)
    print(f"  Python:   {sys.version}")
    print(f"  Platform: {sys.platform}")
    print("=" * 60)

    # Actualizar pip primero
    print("\n📦 Actualizando pip...")
    run_pip("install", "--upgrade", "pip")

    # Instalar dependencias normales
    install_requirements()

    # Instalar insightface con wheel comunitario
    if sys.platform == "win32":
        install_insightface()
    else:
        # En Linux/Mac, pip install directo suele funcionar
        print("📦 Instalando insightface...")
        try:
            run_pip("install", f"insightface=={INSIGHTFACE_VERSION}")
            print("✅ insightface instalado correctamente.\n")
        except subprocess.CalledProcessError:
            print("⚠️  Error al instalar insightface. Revisa que tengas build tools instalados.")

    print("=" * 60)
    print("  ✅ Instalación completada")
    print("=" * 60)
    print("\n  Para ejecutar el servicio:")
    print("    python run.py\n")


if __name__ == "__main__":
    main()
