# Ingenieria de software

En esta seccion se documentan los procesos de ingenieria que preceden y acompanan al desarrollo del sistema.

---

## Proceso de desarrollo

```mermaid
graph LR
    A[Casos de uso] --> B[Priorizacion MoSCoW]
    B --> C[Decision de arquitectura]
    C --> D[Diseno UX/UML]
    D --> E[Desarrollo iterativo]
    E --> F[Pruebas y validacion]
```

Cada paso queda documentado formalmente antes de escribir una sola linea de codigo. Este enfoque previene desviaciones de alcance, sobrecostos y la acumulacion de deuda tecnica.

---

## Contenido de esta seccion

| Documento | Descripcion |
|---|---|
| [Metodologia SDLC](metodologia.md) | Ciclo de vida y fases de produccion |
| [Requisitos MoSCoW](requisitos.md) | Matriz de priorizacion de funcionalidades |
| [Diagramas UML](diagramas.md) | Diagramas de casos de uso y relaciones |
