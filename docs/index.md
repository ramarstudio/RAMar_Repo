---
hide:
  - navigation
  - toc
---

<div class="hero-section" markdown>

<span class="badge">:material-shield-check: v1.0 — Produccion</span>
<span class="badge">:material-wifi-off: 100% Offline</span>
<span class="badge">:material-language-csharp: .NET 8</span>

# Control de Asistencia Biometrico

<p class="hero-subtitle">
Reconocimiento facial en tiempo real, sin internet, sin fotografias almacenadas, con tiempos de respuesta menores a un segundo.
</p>

[Conocer el producto](producto/index.md){ .md-button .md-button--primary }
[Guia de instalacion](instalacion/index.md){ .md-button }

</div>

<div class="stat-grid" markdown>

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

<div class="feature-grid" markdown>

<div class="feature-card">
<span class="card-icon">:material-lightning-bolt:</span>

### Respuesta instantanea

El empleado se acerca a la camara y el sistema responde en milisegundos. Sin filas, sin contacto fisico, sin tarjetas que olvidar.
</div>

<div class="feature-card">
<span class="card-icon">:material-shield-lock:</span>

### Privacidad por diseno

Nunca se almacenan fotografias. Los rostros se transforman en vectores matematicos cifrados con AES-256, completamente irreversibles.
</div>

<div class="feature-card">
<span class="card-icon">:material-server-off:</span>

### Sin dependencias externas

Opera exclusivamente en la red local. Sin internet, sin suscripciones cloud, sin enviar datos biometricos a terceros.
</div>

<div class="feature-card">
<span class="card-icon">:material-monitor-dashboard:</span>

### Panel de administracion

Dashboard con metricas en tiempo real, gestion de empleados, horarios, tardanzas, reportes exportables y auditoria completa.
</div>

<div class="feature-card">
<span class="card-icon">:material-brain:</span>

### IA de alto rendimiento

Motor InsightFace (ArcFace) con precision del 99.8%. Se activa bajo demanda y libera RAM cuando no se usa.
</div>

<div class="feature-card">
<span class="card-icon">:material-cog-outline:</span>

### Configurable por el admin

Tolerancia de tardanzas, horarios por empleado, roles diferenciados (Admin / SuperAdmin / Empleado) y parametros del sistema ajustables.
</div>

</div>

---

<span class="section-label">Arquitectura</span>

## Como esta construido

<div class="tech-stack">
<span class="tech-pill">:material-language-csharp: C# .NET 8</span>
<span class="tech-pill">:material-language-python: Python 3.13</span>
<span class="tech-pill">:material-database: PostgreSQL</span>
<span class="tech-pill">:material-api: FastAPI</span>
<span class="tech-pill">:material-eye: InsightFace</span>
<span class="tech-pill">:material-lock: AES-256</span>
<span class="tech-pill">:material-layers: Entity Framework Core</span>
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

    style C fill:#1a1a2e,stroke:#00d2ff,color:#fff
    style D fill:#16213e,stroke:#00d2ff,color:#fff
    style E fill:#0f3460,stroke:#00d2ff,color:#fff
```

</div>

<div class="feature-grid" markdown>

<div class="feature-card">
<span class="card-icon">:material-desktop-classic:</span>

### Frontend nativo (WPF)

Aplicacion de escritorio con acceso directo al hardware de la camara via DirectShow. Consumo de CPU inferior al 1%.
</div>

<div class="feature-card">
<span class="card-icon">:material-robot:</span>

### Motor biometrico (Python)

Microservicio FastAPI con InsightFace. Genera embeddings de 512 dimensiones. Se inicia solo cuando se necesita.
</div>

<div class="feature-card">
<span class="card-icon">:material-database-lock:</span>

### Base de datos (PostgreSQL)

Embeddings cifrados, auditoria completa, integridad referencial. Entity Framework Core para acceso seguro.
</div>

</div>

---

<div style="text-align: center; padding: 2rem 0;" markdown>

<span class="section-label">Desarrollado por</span>

**RAMar Software Studio**

Innovacion, privacidad computacional y construccion de soluciones corporativas.

[:fontawesome-brands-github: Repositorio](https://github.com/ramarstudio/RAMar_Repo){ .md-button }

</div>
