# 📸 Control de Asistencia Biométrico (Attendance System)

El **Control de Asistencia Biométrico** es un sistema de escritorio diseñado para facilitar el registro de entrada y salida de los empleados utilizando **reconocimiento facial avanzado**, sin necesidad de internet y garantizando la privacidad de los datos.

En lugar de usar huelleros físicos o tarjetas, el empleado simplemente se para frente a la cámara web de la computadora, y el sistema registra su asistencia en menos de un segundo.

---

## 🌟 Beneficios Principales (Para Directivos y Usuarios)

*   **Rápido y Sin Contacto:** Evita las filas y la poca higiene de los lectores de huella tradicionales. Todo se procesa al instante mediante la cámara.
*   **Privacidad Absoluta (100% Local):** Las fotos faciales de los empleados **nunca** se guardan ni se envían a internet. El sistema convierte el rostro en un código matemático (vector) indescifrable, garantizando la seguridad de la información.
*   **Ahorro Total:** Al ejecutarse de manera autónoma en las computadoras locales de la empresa, no existen cobros ni ataduras a suscripciones de plataformas en la nube (Cloud).
*   **Panel Administrativo Fácil de Usar:** Recursos Humanos puede gestionar altas de empleados, revisar históricos de marcajes, visualizar tardanzas y controlar el sistema mediante una interfaz moderna y sumamente sencilla.

---

## ⚙️ ¿Cómo funciona bajo el capó? (Para Desarrolladores)

El sistema está construido bajo una potente arquitectura híbrida. En lugar de empujar el video por internet, divide el trabajo en dos motores de manera local (Modelo Cliente y Microservicio Local).

### 1. Aplicación Principal y UI (C# / .NET 8 WPF)
*   Actúa como el Orquestador y Front-end. Administra las pantallas, las reglas del negocio, el panel de administración y la base de datos local usando **Entity Framework Core**.
*   **Rendimiento Extremo:** Lee el video de la cámara copiando la memoria pura fotograma a fotograma (Punteros e interfaz DirectShow). Al no hacer conversiones inútiles de formato, mantiene el uso del procesador (CPU) casi inactivo.

### 2. Motor de Inteligencia Artificial (Python FastAPI)
*   Cuando la interfaz C# requiere confirmar a un empleado, le envía la alerta a un pequeño servidor interno escrito en **Python 3.13**.
*   Este servidor analiza matemáticamente las características de la cara utilizando una librería científica llamada `InsightFace`.
*   **Aparcado Automático:** Para no sobrecargar la computadora de la oficina, si el sistema detecta que nadie se acerca a la cámara durante 10 minutos seguidos, "duerme" temporalmente la inteligencia artificial liberando toda la Memoria RAM hasta que alguien vuelva a acercarse.

### 3. Almacenamiento Estructurado (PostgreSQL)
*   Una base de datos relacional aloja de forma muy estable la lista de trabajadores, los eventos de asistencia (Entrada/Salida) y los datos matemáticos cifrados de los rostros.

---

## 🛠️ Requisitos de Instalación (Despliegue Rápido)

Para hacer funcionar el sistema, el entorno requiere:
1.  **Sistema Operativo:** Windows 10 u 11 con `.NET 8 Desktop Runtime` instalado.
2.  **Hardware Físico:** Cualquier Cámara Web funcional.
3.  **Entorno Python:** `Python 3.13+` habilitado globalmente (Path) junto a las dependencias matemáticas instaladas (`pip install -r python/requirements.txt`).
4.  **Base de Datos:** Motor **PostgreSQL 15+** corriendo y credenciales válidas provistas en nuestro archivo local `.env`.

> Mantenido con pasión y estándares corporativos por **RAMar Software Studio**.
