---
icon: material/book-open-variant
---

# Ingenieria de software

<span class="section-label">Proceso de desarrollo</span>

Cada paso del desarrollo queda documentado formalmente antes de escribir codigo. Este enfoque previene desviaciones de alcance, sobrecostos y deuda tecnica.

<div class="diagram-box">

```mermaid
graph LR
    A[Casos de uso] --> B[MoSCoW]
    B --> C[ADR]
    C --> D[Diseno UX/UML]
    D --> E[Desarrollo]
    E --> F[Validacion]

    style A fill:#1a1a2e,stroke:#00d2ff,color:#fff
    style F fill:#0f3460,stroke:#00d2ff,color:#fff
```

</div>

---

<span class="section-label">Documentacion</span>

## Contenido de esta seccion

<div class="feature-grid" markdown>

<div class="feature-card">
<span class="card-icon">:material-reload:</span>

### Metodologia SDLC

Ciclo de vida y fases de produccion que garantizan calidad antes de codificar.

[Ver metodologia](metodologia.md){ .md-button }
</div>

<div class="feature-card">
<span class="card-icon">:material-filter:</span>

### Requisitos MoSCoW

Matriz de priorizacion: que se construye, que se descarta, y por que.

[Ver requisitos](requisitos.md){ .md-button }
</div>

<div class="feature-card">
<span class="card-icon">:material-vector-polyline:</span>

### Diagramas UML

Casos de uso, arquitectura y flujos de marcaje documentados visualmente.

[Ver diagramas](diagramas.md){ .md-button }
</div>

</div>
