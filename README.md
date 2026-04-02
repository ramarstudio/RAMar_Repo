⚠️ **Confidential & Sensitive Content**

This repository contains **sensitive and explicit** materials related to the internal development processes of projects carried out by **RAMar Software Studio**.

El siguiente documento describe la arquitectura y estado del proyecto principal alojado en este repositorio.

---

# RAMar Studio - Attendance System (Control de Asistencia Biométrico)

Un robusto sistema de control de asistencia de grado empresarial para redes locales (intranet), dotado con un motor avanzado de **Reconocimiento Facial mediante Machine Learning**.

Este proyecto ha sido optimizado con una estructura nativa de escritorio para garantizar que la cámara consuma 1% del CPU, liberando memoria y maximizando la privacidad de los datos al ejecutarse completamente en el entorno del cliente sin dependencias en la nube (100% Offline Machine Learning).

---

## 🏗️ Arquitectura Técnica del Software

La plataforma se basa en una arquitectura **Cliente-Servidor (Microservicio Local)** construida para ofrecer latencia cero y aprovechar el paralelismo entre la GPU (Si es aplicable) y CPU de las operaciones faciales.

*   **Frontend UI:** `.NET 8 WPF` (Windows Presentation Foundation) con Patrón MVVM impulsado por `MaterialDesignThemes`.
*   **Gestor de Inyección:** ASP.NET Core Dependency Injection (Agnóstico e integrado a WPF).
*   **Base de Datos:** `PostgreSQL` gestionada mediante esquemas transaccionales relacionales puros.
*   **Modelado ORM:** `Entity Framework Core` con enfoque avanzado **Code-First** (El código administra automáticamente la estructura, llaves foráneas y esquemas SQL subyacentes, descartando la dependencia de scripts DB-first obsoletos).
*   **Motor Biométrico (Microservicio Python):** 
    *   Marco: Python 3.13 con `FastAPI`.
    *   IA: `InsightFace` (modelo `buffalo_l`).
    *   Extracción y Procesamiento Vectorial de 512-Dimensiones encriptado AES-GCM 256 bits.
*   **Comunicación Interna:** REST HTTP Local (vía `WaitMsBeforeAsync` loop inter-procesos, manejado automáticamente por el `FaceServiceManager.cs`).

---

## 🚀 Características Principales

1.  **Orquestador Inteligente:** El proyecto C# enciende y apaga el servidor Python bajo demanda a los `10 minutos de reposo`, liberando entre 300 y 400 MB de memoria RAM dinámicamente.
2.  **Tracking y Persistencia Libre:** Despegado por completo el "Change Tracking" de EF Core en lecturas (`.AsNoTracking`), bajando a la mitad los tiempos de consulta para listar usuarios ágilmente.
3.  **Optimizaciones WPF Extremas:** Conversión de matrices (`Mat` OpenCV) a `BitmapSource` mediante copia directa de Puntero no-administrado `IntPtr` (buffer-copy). Previene el bloqueo del Thread UI y libra a `.NET Gen 0 GC` (Garbage Collector) de recolectar megabytes de basura.
4.  **Encriptamiento Biométrico Fuerte:** No almacenamos las fotos capturadas. Sólo vectores matemáticos codificados con la arquitectura simétrica `AES`. Ningún hacker puede extraer la foto del usuario basándose en los números flotantes cifrados en la DB.

---

## 🛠️ Requisitos de Entorno

- **.NET SDK:** Versión `8.0` (o superior).
- **Python:** Versión `3.13.0` (Obligatorio en el System PATH para que C# logre invocar el `FaceService`).
- **PostgreSQL:** Servidor corriendo y credenciales configuradas correspondientes en el Sistema / Variables de entorno por `DotEnv`.
- Instalación de librerías Python (`pip -r requirements.txt` situados en la carpeta `/python`).
  - OpenCV, FastAPI, InsightFace, Uvicorn, ONNXRuntime.

## 📐 Decisiones de Refactorización Recientes
*   *Reemplazo Web vs PC:* Se descartó un modelo Web UI debido a la latencia implícita en transmisiones de video hacia un back-end en red, migrando el control al Hardware Nativo (OpenCV).
*   *Limpieza de Raw SQL:* Todos los `triggers`, `seeds`, y `migrations` SQL manuales alojados en este servidor fueron depurados a favor de C# puro. Todo el esquema lo asume exclusivamente Entity Framework.

---

This repository is intended **exclusively for storage, administration, and management purposes**.  
It is **not meant for public distribution, reuse, or external reference**.
Unauthorized access, disclosure, or use of the information contained in this repository is strictly prohibited.

© RAMar Software Studio. All rights reserved.
