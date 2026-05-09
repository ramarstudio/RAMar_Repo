"""
install.py — Instalador completo y autónomo del FaceService.

El usuario solo necesita ejecutar:
    python install.py

Este script se encarga de todo:
1. Detecta la versión de Python
2. Si es 3.13+, busca automáticamente py -3.12 / -3.11 / -3.10
3. Crea el entorno virtual con la versión correcta
4. Se re-ejecuta dentro del venv (sin que el usuario active nada)
5. Instala todas las dependencias incluyendo insightface
6. Pre-descarga el modelo buffalo_l para que la app arranque al instante
"""

import subprocess
import sys
import os
import ssl
import time
import tempfile
import urllib.request
import urllib.error

WHEEL_BASE_URL      = "https://github.com/Gourieff/Assets/raw/main/Insightface"
INSIGHTFACE_VERSION = "0.7.3"
SUPPORTED_VERSIONS  = {"3.10", "3.11", "3.12"}
SCRIPT_DIR          = os.path.dirname(os.path.abspath(__file__))
VENV_DIR            = os.path.join(SCRIPT_DIR, "venv")
LOG_FILE            = os.path.join(SCRIPT_DIR, "install.log")


# ── Log a consola + archivo ───────────────────────────────────────────────────

class _Tee:
    """Escribe en consola Y en archivo de log simultáneamente."""
    def __init__(self, *streams):
        self.streams = streams

    def write(self, data):
        for s in self.streams:
            try:
                s.write(data)
                s.flush()
            except Exception:
                pass

    def flush(self):
        for s in self.streams:
            try:
                s.flush()
            except Exception:
                pass


def _setup_log():
    log = open(LOG_FILE, "a", encoding="utf-8", errors="replace")
    sys.stdout = _Tee(sys.__stdout__, log)
    return log


# ── Helpers ────────────────────────────────────────────────────────────────────

def _ver(exe=None):
    cmd = [exe or sys.executable, "-c",
           "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')"]
    return subprocess.check_output(cmd, text=True, stderr=subprocess.DEVNULL).strip()


def _pip(*args):
    subprocess.check_call([sys.executable, "-m", "pip"] + list(args))


def sep(title=""):
    print("=" * 60)
    if title:
        print(f"  {title}")
        print("=" * 60)


# ── Descarga robusta (timeout, progreso, SSL-fallback, reintentos) ─────────────

def _download(url, dest, label):
    """
    Descarga con:
    - Timeout de 120s por intento
    - Progreso en tiempo real
    - Fallback SSL sin verificación (entornos con proxy corporativo)
    - 3 reintentos con pausa de 3s entre ellos
    """
    for attempt in range(1, 4):
        for verify_ssl in (True, False):
            try:
                ctx = ssl.create_default_context()
                if not verify_ssl:
                    ctx.check_hostname = False
                    ctx.verify_mode = ssl.CERT_NONE

                note = "" if verify_ssl else " (SSL sin verificar - proxy corporativo)"
                print(f"\n  Intento {attempt}/3{note}...")

                req = urllib.request.Request(
                    url, headers={"User-Agent": "RAMar-Installer/1.0"})
                with urllib.request.urlopen(req, context=ctx, timeout=120) as r:
                    total = int(r.headers.get("Content-Length", 0))
                    done  = 0
                    with open(dest, "wb") as f:
                        while True:
                            chunk = r.read(65536)
                            if not chunk:
                                break
                            f.write(chunk)
                            done += len(chunk)
                            if total:
                                pct = min(100, done * 100 // total)
                                print(f"\r  {label}: {pct:3d}% ({done/1e6:.1f} MB)",
                                      end="", flush=True)
                            else:
                                print(f"\r  {label}: {done/1e6:.1f} MB",
                                      end="", flush=True)

                print(f"\n  Descarga completada ({os.path.getsize(dest)//1024} KB)")
                return True

            except Exception as e:
                print(f"\n  Error: {e}")
                if verify_ssl:
                    print("  Reintentando sin verificar SSL...")
                    continue
                break

        if attempt < 3:
            print("  Esperando 3 segundos antes del siguiente intento...")
            time.sleep(3)

    return False


# ── Venv ──────────────────────────────────────────────────────────────────────

def _inside_venv():
    venv_py = os.path.join(VENV_DIR, "Scripts", "python.exe")
    return os.path.abspath(sys.executable).lower() == os.path.abspath(venv_py).lower()


def _find_python():
    """Devuelve (ruta, version) de un Python 3.10-3.12 compatible."""
    cur = _ver()
    if cur in SUPPORTED_VERSIONS:
        return sys.executable, cur

    print(f"\n  Python {cur} no es compatible con onnxruntime.")
    print("  Buscando Python 3.12 / 3.11 / 3.10 via Python Launcher...")

    for target in ("3.12", "3.11", "3.10"):
        try:
            r = subprocess.run(
                ["py", f"-{target}", "-c",
                 "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')"],
                capture_output=True, text=True, timeout=10)
            if r.returncode == 0 and r.stdout.strip() == target:
                py = subprocess.check_output(
                    ["py", f"-{target}", "-c", "import sys; print(sys.executable)"],
                    text=True, stderr=subprocess.DEVNULL).strip()
                print(f"  Encontrado Python {target}: {py}")
                return py, target
        except (FileNotFoundError, subprocess.TimeoutExpired):
            continue

    return None, None


def _create_venv(py):
    print(f"\n  Creando entorno virtual con Python {_ver(py)}...")
    subprocess.check_call([py, "-m", "venv", VENV_DIR])
    print("  Entorno virtual creado.")


def _relaunch(log):
    venv_py = os.path.join(VENV_DIR, "Scripts", "python.exe")
    print("\n  Relanzando instalador dentro del entorno virtual...")
    log.close()
    sys.exit(subprocess.run([venv_py, os.path.abspath(__file__)]).returncode)


# ── Instalación de paquetes ───────────────────────────────────────────────────

def _install_requirements():
    req = os.path.join(SCRIPT_DIR, "requirements.txt")
    if not os.path.exists(req):
        print("ERROR: requirements.txt no encontrado")
        sys.exit(1)

    with open(req, encoding="utf-8") as f:
        deps = [l.strip() for l in f
                if l.strip() and not l.startswith("#")
                and "insightface" not in l.lower()]

    print("\n  Actualizando pip...")
    _pip("install", "--upgrade", "pip")
    print("\n  Instalando dependencias base (fastapi, uvicorn, onnxruntime, numpy, opencv):")
    _pip("install", *deps)
    print("  Dependencias base instaladas.")


def _install_insightface():
    vshort = _ver()
    vtag   = vshort.replace(".", "")

    if vshort not in SUPPORTED_VERSIONS:
        print(f"\n  AVISO: Python {vshort} no tiene wheel de insightface.")
        return

    wheel = f"insightface-{INSIGHTFACE_VERSION}-cp{vtag}-cp{vtag}-win_amd64.whl"
    url   = f"{WHEEL_BASE_URL}/{wheel}"
    tmp   = tempfile.mkdtemp()
    dest  = os.path.join(tmp, wheel)

    print(f"\n  Descargando insightface {INSIGHTFACE_VERSION} para Python {vshort}...")
    ok = _download(url, dest, "insightface")

    if ok:
        try:
            # --no-deps evita que pip resuelva dependencias de insightface
            # y baje numpy a 1.x. Las deps ya estan instaladas via requirements.txt.
            print("  Instalando desde wheel (sin resolver dependencias)...")
            _pip("install", dest, "--no-deps")
            print("  insightface instalado.")
        except subprocess.CalledProcessError:
            print("  Error instalando desde wheel. Intentando PyPI...")
            try:
                _pip("install", f"insightface=={INSIGHTFACE_VERSION}")
                print("  insightface instalado desde PyPI.")
            except subprocess.CalledProcessError:
                print(f"\n  ERROR: No se pudo instalar insightface.")
                print(f"    venv\\Scripts\\activate.bat")
                print(f"    pip install insightface=={INSIGHTFACE_VERSION}")
        finally:
            try:
                os.remove(dest)
                os.rmdir(tmp)
            except OSError:
                pass
    else:
        print("  Instalando insightface desde PyPI (fallback)...")
        try:
            _pip("install", f"insightface=={INSIGHTFACE_VERSION}")
            print("  insightface instalado desde PyPI.")
        except subprocess.CalledProcessError:
            print(f"\n  ERROR: No se pudo instalar insightface.")

    # Forzar numpy 2.x como ultimo paso.
    # scipy>=1.13, scikit-image>=0.24, matplotlib>=3.9 ya soportan numpy 2.x,
    # por lo que el resolver de pip no deberia bajar numpy. Aun asi forzamos
    # para cubrir cualquier dependencia transitiva inesperada.
    print("\n  Verificando y forzando numpy>=2.1.0...")
    _pip("install", "numpy>=2.1.0", "--force-reinstall", "--no-deps")

    # Verificar que numpy 2.x quedo instalado
    result = subprocess.run(
        [sys.executable, "-c",
         "import numpy as np; v=np.__version__; "
         "major=int(v.split('.')[0]); "
         "print(f'numpy {v}'); "
         "exit(0 if major >= 2 else 1)"],
        capture_output=True, text=True)
    if result.returncode != 0:
        print(f"\n  ERROR CRITICO: numpy sigue siendo 1.x ({result.stdout.strip()}).")
        print("  Alguna dependencia tiene constraint numpy<2. Instalando numpy 2.2.0 fijo...")
        _pip("install", "numpy==2.2.0", "--force-reinstall", "--no-deps")
    else:
        print(f"  {result.stdout.strip()} OK (ABI 2.x compatible)")


def _install_insightface_linux():
    print("\n  Instalando insightface (Linux/Mac)...")
    try:
        _pip("install", f"insightface=={INSIGHTFACE_VERSION}")
        print("  insightface instalado.")
    except subprocess.CalledProcessError:
        print("  ERROR: revisa que tengas gcc y cmake instalados.")


# ── Pre-descarga del modelo buffalo_l ────────────────────────────────────────

def _predownload_model():
    """
    Descarga el modelo buffalo_l (~400 MB) DURANTE la instalación.
    Sin esto, el modelo se descarga la primera vez que el usuario usa
    reconocimiento facial, causando una espera silenciosa de varios minutos.
    """
    sep("Verificando modelo de reconocimiento facial")
    print("  Modelo: buffalo_l (det_10g.onnx + w600k_r50.onnx)")

    # InsightFace agrega "\models\" al root internamente.
    # Pasamos SCRIPT_DIR para que busque en SCRIPT_DIR\models\buffalo_l
    models_dir  = SCRIPT_DIR
    buffalo_dir = os.path.join(SCRIPT_DIR, "models", "buffalo_l")
    os.makedirs(buffalo_dir, exist_ok=True)

    if os.path.isdir(buffalo_dir) and any(
        f.endswith(".onnx") for f in os.listdir(buffalo_dir)
    ):
        print("  Modelo ya existe — omitiendo descarga.")
        return True

    try:
        import insightface
        face_app = insightface.app.FaceAnalysis(
            name="buffalo_l",
            root=models_dir,
            providers=["CPUExecutionProvider"],
        )
        face_app.prepare(ctx_id=-1, det_size=(320, 320))
        print("\n  Modelo descargado correctamente.")
        return True
    except Exception as e:
        print(f"\n  AVISO: No se pudo pre-descargar el modelo: {e}")
        print("  El modelo se descargara automaticamente la primera vez")
        print("  que uses el reconocimiento facial (requiere internet).")
        return False


# ── Main ──────────────────────────────────────────────────────────────────────

def main():
    log = _setup_log()

    sep("FaceService — Instalador")
    print(f"  Python    : {sys.version.split()[0]}")
    print(f"  Ejecutable: {sys.executable}")
    print(f"  Directorio: {SCRIPT_DIR}")
    print(f"  Log       : {LOG_FILE}")
    sep()

    if not _inside_venv():
        # ── Fase bootstrap (fuera del venv) ──────────────────────────────────
        py, ver = _find_python()

        if py is None:
            sep("ERROR — Python compatible no encontrado")
            print("  Se requiere Python 3.10, 3.11 o 3.12.")
            print()
            print("  Instala Python 3.12 con:")
            print("    winget install Python.Python.3.12")
            print("  O descarga desde: https://www.python.org/downloads/")
            print("  Marca 'Add Python to PATH' al instalar.")
            sep()
            log.close()
            sys.exit(1)

        # Siempre recrear el venv para garantizar un estado limpio.
        # Evita que instalaciones anteriores con numpy incorrecto persistan.
        import shutil
        if os.path.exists(VENV_DIR):
            print("\n  Eliminando entorno virtual anterior para instalacion limpia...")
            shutil.rmtree(VENV_DIR, ignore_errors=True)
        _create_venv(py)

        _relaunch(log)
        return

    # ── Fase instalación (dentro del venv correcto) ───────────────────────────
    sep(f"Instalando dependencias (Python {_ver()})")

    _install_requirements()

    if sys.platform == "win32":
        _install_insightface()
    else:
        _install_insightface_linux()

    _predownload_model()

    sep("Instalacion completada")
    print()
    print("  El motor de reconocimiento facial esta listo.")
    print("  Abre la aplicacion desde el acceso directo en el Escritorio.")
    print(f"\n  Log completo guardado en: {LOG_FILE}")
    print()
    log.close()


if __name__ == "__main__":
    main()
