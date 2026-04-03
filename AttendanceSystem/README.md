# 📸 Control de Asistencia Biométrico (Attendance System)

El **Control de Asistencia Biométrico** es un sistema de escritorio diseñado para facilitar el registro de entrada y salida de los empleados utilizando **reconocimiento facial avanzado**, sin necesidad de internet y garantizando la privacidad de los datos.

En lugar de usar huelleros físicos o tarjetas, el empleado simplemente se para frente a la cámara web de la computadora, y el sistema registra su asistencia en menos de un segundo.

---

## 🌟 Beneficios Principales (Para Directivos y Usuarios)

*   **Rápido y Sin Contacto:** Evita las filas y la poca higiene de los lectores de huella tradicionales. Todo se procesa al instante mediante la cámara.
*   **Privacidad Absoluta (100% Local):** Las fotos faciales de los empleados **nunca** se guardan ni se envían a internet. El sistema convierte el rostro en un código matemático (vector de 512 dimensiones) indescifrable, garantizando la seguridad de la información.
*   **Ahorro Total:** Al ejecutarse de manera autónoma en las computadoras locales de la empresa, no existen cobros ni ataduras a suscripciones de plataformas en la nube (Cloud).
*   **Panel Administrativo Fácil de Usar:** Recursos Humanos puede gestionar altas de empleados, revisar históricos de marcajes, visualizar tardanzas y controlar el sistema mediante una interfaz moderna y sumamente sencilla.

---

## ⚙️ ¿Cómo funciona bajo el capó? (Para Desarrolladores)

El sistema está construido bajo una potente arquitectura híbrida. En lugar de empujar el video por internet, divide el trabajo en dos motores de manera local (Modelo Cliente y Microservicio Local).

### 1. Aplicación Principal y UI (C# / .NET 8 WPF)
*   Actúa como el Orquestador y Front-end. Administra las pantallas, las reglas del negocio, el panel de administración y la base de datos local usando **Entity Framework Core**.
*   **Rendimiento Extremo:** Lee el video de la cámara copiando la memoria pura fotograma a fotograma (Punteros e interfaz DirectShow). Al no hacer conversiones inútiles de formato, mantiene el uso del procesador (CPU) casi inactivo.
*   **Gestión Inteligente del Motor IA:** El `FaceServiceManager` arranca el microservicio Python automáticamente cuando se necesita y lo detiene tras 10 minutos de inactividad, liberando ~400-500 MB de RAM.

### 2. Motor de Inteligencia Artificial (Python FastAPI)
*   Cuando la interfaz C# requiere confirmar a un empleado, le envía la imagen a un pequeño servidor interno escrito en **Python** corriendo en `localhost:5001`.
*   Este servidor analiza matemáticamente las características de la cara utilizando **InsightFace** (modelo ArcFace, precisión 99.8% en benchmark LFW).
*   Genera un **embedding de 512 dimensiones** normalizado L2, que se compara por similitud coseno contra los embeddings registrados.
*   **Aparcado Automático:** Si el sistema detecta que nadie se acerca a la cámara durante 10 minutos seguidos, "duerme" temporalmente la inteligencia artificial liberando toda la Memoria RAM hasta que alguien vuelva a acercarse.

### 3. Almacenamiento Estructurado (PostgreSQL)
*   Una base de datos relacional aloja de forma muy estable la lista de trabajadores, los eventos de asistencia (Entrada/Salida), los datos matemáticos cifrados de los rostros, y las claves de seguridad del sistema.

---

## 🛠️ Requisitos de Instalación (Despliegue Rápido)

Para hacer funcionar el sistema, el entorno requiere:

1.  **Sistema Operativo:** Windows 10 u 11 (x64)
2.  **Software:**
    - `.NET 8 SDK` — [descargar aquí](https://dotnet.microsoft.com/download/dotnet/8.0)
    - `Python 3.10+` — [descargar aquí](https://www.python.org/downloads/) (marcar "Add to PATH")
    - `PostgreSQL 15+` — [descargar aquí](https://www.postgresql.org/download/windows/)
3.  **Hardware:** Cualquier cámara web USB o integrada + 4 GB RAM mínimo (8 GB recomendado)

### Pasos esenciales

1. Crea la base de datos PostgreSQL mediante psql o pgAdmin:
   `CREATE DATABASE AttendanceSystem;`
2. Ve a la carpeta raíz del repositorio (`RAMar_Repo`).
3. Haz doble clic en el archivo **`iniciar.bat`**.

El script guiado te pedirá colocar tu contraseña, creará los entornos de Python descargando la Inteligencia artificial y finalmente arrancará la interfaz.

### Credenciales iniciales

| Campo | Valor |
|---|---|
| **Usuario** | `admin` |
| **Contraseña** | `admin123` |
| **Rol** | SuperAdmin |

> ⚠️ **Cambia la contraseña inmediatamente** en el primer uso.

---

> Mantenido con pasión y estándares corporativos por **RAMar Software Studio**.
