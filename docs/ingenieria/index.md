---
icon: material/book-open-variant
---

# Ingenieria de software

<span class="section-label">Proceso de desarrollo</span>

Cada paso queda documentado formalmente antes de escribir codigo. Este enfoque previene desviaciones de alcance, sobrecostos y deuda tecnica.

<div class="diagram-box">

```mermaid
graph LR
    A[Casos de uso] --> B[MoSCoW]
    B --> C[ADR]
    C --> D[Diseno UX/UML]
    D --> E[Desarrollo]
    E --> F[Validacion]
```

</div>

---

<span class="section-label">Documentacion</span>

## Contenido de esta seccion

<div class="grid cards" markdown>

-   :material-reload:{ .lg .middle } **Metodologia SDLC**

    ---

    Ciclo de vida y fases de produccion que garantizan calidad antes de codificar.

    [:octicons-arrow-right-24: Ver metodologia](metodologia.md)

-   :material-filter:{ .lg .middle } **Requisitos MoSCoW**

    ---

    Matriz de priorizacion: que se construye, que se descarta, y por que.

    [:octicons-arrow-right-24: Ver requisitos](requisitos.md)

-   :material-vector-polyline:{ .lg .middle } **Diagramas UML**

    ---

    Casos de uso, arquitectura y flujos de marcaje documentados visualmente.

    [:octicons-arrow-right-24: Ver diagramas](diagramas.md)

</div>
