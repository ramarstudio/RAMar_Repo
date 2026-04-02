# ⚙️ Estructura Técnica y Código (AttendanceSystem)

El sistema biométrico está construido bajo una potente arquitectura híbrida que separa la carga gráfica de la inteligencia puramente algorítmica.

---

## 1. El Orquestador (Front-end Visual y Base de Datos)
* **Tecnología:** Escrito en `C# .NET 8` nativo para Windows (`WPF`).
* **Arquitectura Interna:** Se aplica estrictamente el Patrón de Diseño **MVVM** (Model-View-ViewModel) acoplado a un inyector de dependencias.
* **Ventaja del Hardware:** Al ser nativo de Windows, logra atrapar el video de la cámara usando directamente la interfaz `DirectShow` desde punteros `IntPtr` en memoria pura. Esto cancela la sobrecarga por renderizado de red (Web) y reduce el consumo del procesador al mínimo (~1%).
* **Persistencia de Datos:** Toda la bóveda de empleados y sus vectores faciales se almacena en **PostgreSQL**. Esta comunicación es orquestada exclusivamente mediante **Entity Framework Core**, previniendo inyecciones manuales y SQL Raw riesgoso.

---

## 2. El Motor de Inteligencia (Back-end Matemático)
* **Tecnología:** Microservicio asíncrono desarrollado en **Python 3.13** operando paralelamente en la máquina local.
* **Red de Api:** Expuesto mediante **FastAPI**, de modo que C# le pueda pedir reconocimientos como si fuera un servidor web, a pesar de estar dentro de la misma PC.
* **Cerebro:** Alimentado por `InsightFace` (Usando el modelo `buffalo_l`), el cual lee los `frames` en vivo de C# y extrae matemáticamente 512 puntos inalterables de características espaciales de cada rostro humano (Embeddings).

### Flujo Operacional Resumido
1. El empleado se acerca al Kiosko.
2. C# (WPF) captura su cuadro de video sin bloquear la Interfaz (Asíncrono).
3. Envía el cuadro a Python (FastAPI).
4. Python lo vectoriza y lo regresa.
5. C# compara el vector matemáticamente con la bóveda PostgreSQL.
6. Pantalla verde de aprobación o Denegación en Milisegundos.
