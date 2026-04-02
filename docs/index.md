---
hide:
  - navigation
  - toc
---

<div class="hero-section">
  <div class="badge-row">
    <span class="badge">v1.0 — Produccion</span>
    <span class="badge">100% Offline</span>
    <span class="badge">C# .NET 8</span>
    <span class="badge">Python + IA</span>
  </div>
  <h1>Control de Asistencia Biometrico</h1>
  <p class="subtitle">
    Reconocimiento facial en tiempo real, sin internet, sin fotografias almacenadas, con tiempos de respuesta menores a un segundo.
  </p>
  <a href="producto/" class="md-button md-button--primary">Conocer el producto</a>
  <a href="instalacion/" class="md-button">Guia de instalacion</a>
</div>

<div class="stat-grid">
  <div class="stat-card">
    <div class="stat-value">&lt; 1s</div>
    <div class="stat-label">Tiempo de marcaje</div>
  </div>
  <div class="stat-card">
    <div class="stat-value">512-d</div>
    <div class="stat-label">Vector facial</div>
  </div>
  <div class="stat-card">
    <div class="stat-value">AES-256</div>
    <div class="stat-label">Cifrado biometrico</div>
  </div>
  <div class="stat-card">
    <div class="stat-value">0</div>
    <div class="stat-label">Fotos almacenadas</div>
  </div>
</div>

---

<span class="section-label">Caracteristicas principales</span>

## Por que este sistema

<div class="grid cards" markdown>

-   :material-lightning-bolt:{ .lg .middle } **Respuesta instantanea**

    ---

    El empleado se acerca a la camara y el sistema responde en milisegundos. Sin filas, sin contacto fisico, sin tarjetas.

-   :material-shield-lock:{ .lg .middle } **Privacidad por diseno**

    ---

    Nunca se almacenan fotografias. Los rostros se transforman en vectores matematicos cifrados con AES-256, completamente irreversibles.

-   :material-server-off:{ .lg .middle } **Sin dependencias externas**

    ---

    Opera en la red local. Sin internet, sin suscripciones cloud, sin enviar datos biometricos a terceros.

-   :material-monitor-dashboard:{ .lg .middle } **Panel de administracion**

    ---

    Dashboard con metricas en tiempo real, gestion de empleados, horarios, tardanzas, reportes y auditoria completa.

-   :material-brain:{ .lg .middle } **IA de alto rendimiento**

    ---

    Motor InsightFace (ArcFace) con 99.8% de precision. Se activa bajo demanda y libera RAM cuando no se usa.

-   :material-cog-outline:{ .lg .middle } **Configurable**

    ---

    Tolerancia de tardanzas, horarios por empleado, roles diferenciados y parametros del sistema ajustables por el admin.

</div>

---

<span class="section-label">Arquitectura</span>

## Como esta construido

<div class="tech-stack">
  <span class="tech-pill">C# .NET 8</span>
  <span class="tech-pill">Python 3.13</span>
  <span class="tech-pill">PostgreSQL</span>
  <span class="tech-pill">FastAPI</span>
  <span class="tech-pill">InsightFace</span>
  <span class="tech-pill">AES-256</span>
  <span class="tech-pill">Entity Framework Core</span>
</div>

<div class="diagram-box">

```mermaid
graph LR
    A[Empleado] -->|Se acerca| B[Camara web]
    B -->|Frame| C[App WPF — C# .NET 8]
    C -->|HTTP localhost| D[Motor IA — Python]
    D -->|Embedding 512-d| C
    C -->|Query| E[(PostgreSQL)]
    E -->|Match| C
    C -->|Resultado| F[Aprobado / Denegado]
```

</div>

<div class="grid cards" markdown>

-   :material-desktop-classic:{ .lg .middle } **Frontend nativo (WPF)**

    ---

    Aplicacion de escritorio con acceso directo al hardware via DirectShow. CPU inferior al 1%.

-   :material-robot:{ .lg .middle } **Motor biometrico (Python)**

    ---

    Microservicio FastAPI con InsightFace. Embeddings de 512 dimensiones. Inicio bajo demanda.

-   :material-database-lock:{ .lg .middle } **Base de datos (PostgreSQL)**

    ---

    Embeddings cifrados, auditoria completa, integridad referencial via Entity Framework Core.

</div>

---

<div style="text-align: center; padding: 1.5rem 0;" markdown>

**RAMar Software Studio** — Innovacion, privacidad computacional y soluciones corporativas.

[:fontawesome-brands-github: Ver repositorio](https://github.com/ramarstudio/RAMar_Repo){ .md-button }

</div>
