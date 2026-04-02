---
icon: material/download
---

# Instalacion

<span class="section-label">Puesta en marcha</span>

Esta seccion cubre todo lo necesario para desplegar el sistema de asistencia biometrico en una maquina Windows.

<div class="feature-grid" markdown>

<div class="feature-card">
<span class="card-icon">:material-clipboard-check:</span>

### Requisitos previos

Software, hardware y puertos necesarios antes de comenzar.

[Ver requisitos](requisitos.md){ .md-button }
</div>

<div class="feature-card">
<span class="card-icon">:material-rocket-launch:</span>

### Guia paso a paso

Desde clonar el repositorio hasta el primer marcaje biometrico.

[Comenzar instalacion](guia.md){ .md-button .md-button--primary }
</div>

</div>

---

<span class="section-label">Vista rapida</span>

## Proceso de instalacion

<div class="diagram-box">

```mermaid
graph LR
    A[Requisitos] --> B[PostgreSQL]
    B --> C[Variables .env]
    C --> D[Dependencias Python]
    D --> E[Compilar WPF]
    E --> F[Ejecutar]

    style F fill:#0f3460,stroke:#00d2ff,color:#fff
```

</div>

| Paso | Tiempo estimado |
|---|---|
| Instalar requisitos previos | ~10 min |
| Configurar base de datos | ~5 min |
| Instalar dependencias Python | ~5 min |
| Primera ejecucion | ~2 min |
| **Total** | **~22 min** |
