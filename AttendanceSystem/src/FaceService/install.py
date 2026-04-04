"""
install.py — Instalador completo y autónomo del FaceService.

El usuario solo necesita ejecutar:
    python install.py

Este script se encarga de todo:
1. Detecta la versión de Python
2. Si es 3.13+, busca automáticamente py -3.12
3. Crea el entorno virtual con la versión correcta
4. Se re-ejecuta dentro del venv (sin que el usuario active nada)
5. Instala todas las dependencias incluyendo insightface
"""

import subprocess
import sys
import os
import urllib.request
import tempfile

WHEEL_BASE_URL  = "https://github.com/Gourieff/Assets/raw/main/Insightface"
INSIGHTFACE_VERSION = "0.7.3"
SUPPORTED_VERSIONS  = {"3.10", "3.11", "3.12"}
VENV_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "venv")


# ── Helpers ────────────────────────────────────────────────────────────────────

def ver_short(executable=None):
    if executable:
        out = subprocess.check_output([executable, "-c",
              "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')"],
              text=True).strip()
        return out
    return f"{sys.version_info.major}.{sys.version_info.minor}"


def ver_tag(executable=None):
    v = ver_short(executable)
    return v.replace(".", "")


def run_pip(*args):
    subprocess.check_call([sys.executable, "-m", "pip"] + list(args))


def separator(title=""):
    print("=" * 60)
    if title:
        print(f"  {title}")
        print("=" * 60)


# ── Paso 1: Verificar si ya estamos dentro del venv correcto ──────────────────

def inside_correct_venv():
    """True si el intérprete actual ES el python del venv de este proyecto."""
    venv_python = os.path.join(VENV_DIR, "Scripts", "python.exe")
    return os.path.abspath(sys.executable).lower() == os.path.abspath(venv_python).lower()


# ── Paso 2: Encontrar el Python adecuado para crear el venv ──────────────────

def find_compatible_python():
    """
    Devuelve la ruta al ejecutable de Python 3.10-3.12.
    Si el Python actual es compatible, lo retorna directamente.
    Si es 3.13+, busca 'py -3.12' o 'py -3.11'.
    """
    current = ver_short()

    if current in SUPPORTED_VERSIONS:
        return sys.executable, current

    # Python incompatible (3.13+) — buscar con Python Launcher
    print(f"\n  Python {current} detectado — no compatible con onnxruntime.")
    print("  Buscando Python 3.12 mediante Python Launcher (py)...\n")

    for target in ("3.12", "3.11", "3.10"):
        try:
            result = subprocess.run(
                ["py", f"-{target}", "-c",
                 "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')"],
                capture_output=True, text=True, timeout=5
            )
            if result.returncode == 0 and result.stdout.strip() == target:
                py_path = subprocess.check_output(
                    ["py", f"-{target}", "-c", "import sys; print(sys.executable)"],
                    text=True
                ).strip()
                print(f"  Encontrado: Python {target} en {py_path}")
                return py_path, target
        except (FileNotFoundError, subprocess.TimeoutExpired):
            continue

    return None, None


# ── Paso 3: Crear venv con el Python correcto ─────────────────────────────────

def create_venv(python_exe):
    print(f"\n  Creando entorno virtual con Python {ver_short(python_exe)}...")
    subprocess.check_call([python_exe, "-m", "venv", VENV_DIR])
    print("  Entorno virtual creado.")


# ── Paso 4: Re-ejecutar este script dentro del venv ──────────────────────────

def relaunch_in_venv():
    venv_python = os.path.join(VENV_DIR, "Scripts", "python.exe")
    print(f"\n  Relanzando instalador dentro del entorno virtual...")
    result = subprocess.run([venv_python, os.path.abspath(__file__)])
    sys.exit(result.returncode)


# ── Instalación de dependencias (ya dentro del venv) ─────────────────────────

def install_requirements():
    req_file = os.path.join(os.path.dirname(os.path.abspath(__file__)), "requirements.txt")
    if not os.path.exists(req_file):
        print("ERROR: No se encontró requirements.txt")
        sys.exit(1)

    with open(req_file, "r", encoding="utf-8") as f:
        lines = f.readlines()

    deps = [
        line.strip() for line in lines
        if line.strip() and not line.startswith("#") and "insightface" not in line.lower()
    ]

    print("\n  Instalando dependencias base...")
    run_pip("install", "--upgrade", "pip", "-q")
    run_pip("install", *deps, "-q")
    print("  Dependencias base instaladas.")


def install_insightface():
    vtag  = ver_tag()
    vshort = ver_short()

    if vshort not in SUPPORTED_VERSIONS:
        print(f"\n  AVISO: Python {vshort} no tiene wheel de insightface.")
        return

    wheel_name = f"insightface-{INSIGHTFACE_VERSION}-cp{vtag}-cp{vtag}-win_amd64.whl"
    wheel_url  = f"{WHEEL_BASE_URL}/{wheel_name}"

    print(f"\n  Descargando insightface {INSIGHTFACE_VERSION} para Python {vshort}...")
    tmp_dir    = tempfile.mkdtemp()
    wheel_path = os.path.join(tmp_dir, wheel_name)

    try:
        urllib.request.urlretrieve(wheel_url, wheel_path)
        print(f"  Descarga completada ({os.path.getsize(wheel_path) // 1024} KB)")
    except Exception as e:
        print(f"\n  ERROR al descargar insightface: {e}")
        print(f"  URL: {wheel_url}")
        print("  Puedes instalarlo manualmente luego con:")
        print(f"    venv\\Scripts\\activate.bat && pip install {wheel_name}")
        return

    try:
        run_pip("install", wheel_path, "-q")
        print("  insightface instalado.")
    except subprocess.CalledProcessError:
        print("  ERROR al instalar insightface desde wheel.")
    finally:
        try:
            os.remove(wheel_path)
            os.rmdir(tmp_dir)
        except OSError:
            pass


def install_insightface_linux():
    print("\n  Instalando insightface (Linux/Mac)...")
    try:
        run_pip("install", f"insightface=={INSIGHTFACE_VERSION}", "-q")
        print("  insightface instalado.")
    except subprocess.CalledProcessError:
        print("  ERROR: revisa que tengas build tools instalados (gcc, cmake).")


# ── Main ──────────────────────────────────────────────────────────────────────

def main():
    separator("FaceService — Instalador")
    print(f"  Python actual : {sys.version.split()[0]}")
    print(f"  Ejecutable    : {sys.executable}")
    separator()

    # ── Fase bootstrap (fuera del venv) ──────────────────────────────────────
    if not inside_correct_venv():

        python_exe, python_ver = find_compatible_python()

        if python_exe is None:
            separator("ERROR — Python compatible no encontrado")
            print("  Se requiere Python 3.10, 3.11 o 3.12.")
            print()
            print("  Instala Python 3.12 con:")
            print("    winget install Python.Python.3.12")
            print()
            print("  O descarga desde: https://www.python.org/downloads/")
            print("  Marca 'Add Python to PATH' al instalar.")
            separator()
            sys.exit(1)

        # Crear venv si no existe
        venv_python = os.path.join(VENV_DIR, "Scripts", "python.exe")
        if not os.path.exists(venv_python):
            create_venv(python_exe)
        else:
            # Verificar que el venv existente usa la versión correcta
            try:
                existing_ver = ver_short(venv_python)
                if existing_ver not in SUPPORTED_VERSIONS:
                    print(f"\n  Venv existente usa Python {existing_ver} (incompatible). Recreando...")
                    import shutil
                    shutil.rmtree(VENV_DIR)
                    create_venv(python_exe)
                else:
                    print(f"\n  Venv existente con Python {existing_ver} — reutilizando.")
            except Exception:
                create_venv(python_exe)

        relaunch_in_venv()
        return  # nunca llega aquí

    # ── Fase instalación (dentro del venv correcto) ───────────────────────────
    separator(f"Instalando dependencias (Python {ver_short()})")

    install_requirements()

    if sys.platform == "win32":
        install_insightface()
    else:
        install_insightface_linux()

    separator("Instalacion completada")
    print()
    print("  Para iniciar la aplicacion:")
    print("    dotnet run --project AttendanceSystem/src/AttendanceSystem.App")
    print()


if __name__ == "__main__":
    main()
