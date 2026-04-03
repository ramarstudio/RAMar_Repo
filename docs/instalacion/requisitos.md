# Requisitos del sistema

Verifica que el equipo cumple estos requisitos antes de instalar.

---

## Hardware mínimo

| Componente | Mínimo | Recomendado |
|---|---|---|
| **Procesador** | Intel Core i3 / AMD Ryzen 3 (4 núcleos) | Core i5 / Ryzen 5 o superior |
| **RAM** | 8 GB | 16 GB |
| **Almacenamiento libre** | 4 GB | 8 GB |
| **Cámara** | 720p USB o integrada | 1080p con buena apertura |
| **Sistema operativo** | Windows 10 64-bit (build 1903+) | Windows 10 / 11 64-bit |
| **Internet** | Solo durante la instalación | No requerido en uso diario |

!!! info "¿Por qué 4 GB de espacio libre?"
    El sistema descarga e instala varios componentes la primera vez:

    | Componente | Tamaño aproximado |
    |---|---|
    | Modelo de reconocimiento facial `buffalo_l` | ~600 MB |
    | Entorno Python + librerías de IA | ~1.2 GB |
    | Aplicación .NET compilada | ~200 MB |
    | Base de datos + datos operativos (inicial) | ~500 MB |
    | **Total estimado** | **~2.5 GB** |

    Con el tiempo la base de datos crece según la cantidad de empleados y marcajes registrados.

!!! warning "RAM: por qué importa"
    El motor de reconocimiento facial (InsightFace) carga el modelo en memoria cuando está activo. Consume entre **400–600 MB de RAM adicionales**. Con menos de 8 GB el equipo puede volverse lento durante el reconocimiento.

    Cuando el motor no está en uso, se apaga automáticamente y libera esa memoria.

---

## Software requerido

Estos tres programas deben estar instalados **antes** de ejecutar `iniciar.bat`.

### PostgreSQL 15 o superior

Base de datos donde se guardan empleados, marcajes y configuraciones.

- Descarga: [postgresql.org/download/windows](https://www.postgresql.org/download/windows/)
- Durante la instalación: anota la contraseña del usuario `postgres` — la necesitarás
- Puerto por defecto: `5432` (no cambiar)
- Con el nuevo `iniciar.bat`, **no necesitas crear la base de datos manualmente** — el script lo hace por ti

### .NET 8 SDK (64-bit)

Plataforma de ejecución del panel de administración.

- Descarga: [dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- Descargar el **SDK** (no solo el Runtime)
- Verificar instalación: abrir CMD y escribir `dotnet --version` — debe mostrar `8.x.x`

### Python 3.10, 3.11 o 3.12

Motor del servicio de reconocimiento facial.

!!! danger "Versión crítica — lee esto antes de instalar"
    Instala **únicamente Python 3.10, 3.11 o 3.12**.

    Python 3.13+ no es compatible con `onnxruntime` (motor de inferencia de IA) y causará errores durante la instalación.
    El script `iniciar.bat` bloquea versiones incompatibles y te indica qué versión descargar.

- Descarga: [python.org/downloads](https://www.python.org/downloads/) — busca la sección "Looking for a specific release?" y descarga la **3.12.x**
- **Obligatorio:** marcar la casilla **"Add Python to PATH"** en la primera pantalla del instalador

---

## Librería de compatibilidad C++ (si hay errores de biometría)

La mayoría de equipos ya la tienen instalada. Si ves un error relacionado con el motor ONNX o biometría:

- Descarga: [Visual C++ Redistributable x64](https://aka.ms/vs/17/release/vc_redist.x64.exe)
- Instala y reinicia el equipo

---

## Verificación rápida

Abre el símbolo del sistema (CMD) y ejecuta uno a uno:

```cmd
dotnet --version
python --version
psql --version
```

Si los tres responden sin error, el equipo está listo para instalar.

---

## Cámara

El sistema funciona con cualquier cámara que Windows reconozca automáticamente:

- Cámaras USB estándar (UVC compatible)
- Cámaras integradas de laptop
- No requiere drivers especiales ni software adicional

Para mejores resultados: buena iluminación frontal, sin contraluz directo.

---

## Red y conectividad

| | Instalación | Uso diario |
|---|---|---|
| **Internet** | Requerido (descargar modelo de IA) | No requerido |
| **Red local** | Opcional | Solo si PostgreSQL está en otro equipo |
| **Puertos** | — | `5001` (servicio IA, solo localhost) |

El sistema opera completamente **offline** una vez instalado.
