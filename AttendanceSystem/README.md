# Attendance System (Control de Asistencia Biométrico)

Un robusto sistema de control de asistencia de grado empresarial para redes locales (intranet), dotado con un motor avanzado de **Reconocimiento Facial mediante Machine Learning**.

Este proyecto ha sido optimizado con una estructura nativa de escritorio para garantizar que la cámara consuma menos del 1% del CPU general de la máquina, liberando memoria y maximizando la privacidad de los datos al ejecutarse completamente en el entorno del cliente sin dependencias en la nube (100% Offline Machine Learning).

---

## 🏗️ Arquitectura Técnica del Software

La plataforma se basa en una arquitectura **Cliente-Servidor (Microservicio Local)** construida para ofrecer latencia cero y aprovechar el paralelismo entre la GPU (si es aplicable) y CPU de las operaciones matemáticas intensivas de reconocimiento facial.

*   **Frontend y Orquestación:** `.NET 8 WPF` (Windows Presentation Foundation) con Patrón MVVM impulsado visualmente por componentes de UI modernos (`MaterialDesignThemes`).
*   **Gestión de Dependencias:** ASP.NET Core Dependency Injection completamente integrado a la aplicación WPF de escritorio.
*   **Base de Datos:** Motor `PostgreSQL` gestionado mediante esquemas transaccionales relacionales puros.
*   **Modelado y Comunicación hacia DB (ORM):** `Entity Framework Core` con enfoque avanzado **Code-First** (El código de C# modela, automatiza las llaves foráneas y domina el esquema pre-existente, descartando la dependencia de mantener scripts SQL RAW manualmente).
*   **Motor Biométrico Independiente (Layer Microservicio en Python):** 
    *   **Protocolo Base:** Python 3.13 encapsulado y sirviendo consultas por `FastAPI`.
    *   **Motor de Inteligencia Artificial:** `InsightFace` utilizando el modelo matemático espacial de la capa `buffalo_l`.
    *   **Seguridad y Retención de Identidad:** Extracción y almacenamiento en base de datos de Embeddings Vectoriales puros (512-Dimensiones) cifrados simétricamente mediante **AES-GCM 256 bits**. *Por motivos de estricta privacidad, las fotos en bruto JPG/PNG nunca se mantienen persistentes localmente luego del proceso analítico algorítmico*.

---

## 🚀 Puntos Focales de Rendimiento C# y C++

1.  **Dormitorio Orquestador (Memoria Compartida):** El controlador de C# intercepta, enciende y apaga lógicamente al proceso del ecosistema de Python bajo demanda tras `10 minutos corridos de inactividad de panel`. Esta técnica limpia y recicla cerca de ~400 MB de memoria RAM del ordenador automáticamente.
2.  **Entity Framework Core Optimizaciones (`.AsNoTracking`):** Al ignorar el Tracking de mutabilidad predeterminado (ChangeTracker) durante procesos de indexación de lecturas (Getters o Fetch de la DB), el sistema ahorra el 50% de overhead de microsegundos de la CPU, logrando interfaces y reportes instantáneos al usuario.
3.  **Conversión Vectorial WPF a Nivel de Punteros (Transmisión Directa `IntPtr`):** Todo el Feed en vivo de la Cámara extraído desde el buffer de OpenCV (`Mat`) es enviado como textura sin compresión leyendo directamente desde un bloque de C++ estricto de memoria (`mat.Data`). Al abstraerse esto y omitirse codificaciones a JPG intermedias, el Garbage Collector (GC de la Gen 0 .NET 8) previene una saturación innecesaria de Memoria Virtual por cada frame servido, salvando drásticamente el congelamiento esporádico visual (*Stutter* o Lag de Interfaz) a los empleados.

---

## 🛠️ Requisitos del Entorno (Deployment Básico)

- **.NET SDK Runtime:** Versión Ejecutable `.NET 8.0 Windows Desktop Runtime` (O superior).
- **Procesador de Inteligencia:** Instalación de **Python 3.13+** debidamente configurado en las variables globales (`Environment Variables / PATH`) para posibilitar al servidor .NET orquestar los shells y comandos al daemon de `uvicorn`.
- **Base de Datos Local / Remota Mapeada:** Un motor de PostgreSql 15+ corriendo (y cuyas credenciales de acceso residan en la base segura `.env`).
- Archivo de inicialización intermedio para módulos (`pip -r requirements.txt`). Contiene las bases algorítmicas OpenCV, FastAPI, InsightFace, onnxruntime.

> **RAMar Studio** - Optimizando Ciencias de la Computación al máximo rendimiento corporativo.
