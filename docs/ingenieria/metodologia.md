---
icon: material/reload
---

# Metodologia SDLC

<span class="section-label">Ciclo de vida</span>

Se aplica un ciclo de vida de desarrollo de software riguroso para asegurar calidad corporativa antes de escribir codigo.

---

## Fases de produccion

### 1. Identificacion de casos de uso

Se traducen las necesidades operativas de la empresa en historias tecnicas y medibles. Se modela que debe hacer cada actor (administrador, empleado) y cuales son sus limites de interaccion.

### 2. Priorizacion MoSCoW

Antes de cualquier presupuesto, se prioriza el alcance:

| Categoria | Significado | Accion |
|---|---|---|
| **Must** | Obligatorio | Se construye sin excepciones |
| **Should** | Recomendado | Se incluye si hay tiempo |
| **Could** | Opcional | Se evalua para futuras versiones |
| **Won't** | Descartado | No se construye en este MVP |

!!! warning "Feature creep"
    La priorizacion MoSCoW previene la acumulacion de funcionalidades innecesarias que desbordan presupuestos y plazos.

### 3. Decisiones de arquitectura (ADR)

Se evaluan objetivamente las alternativas tecnologicas y se **justifica por escrito** la decision final mediante un Architecture Decision Record.

[Ver ADR: Por que WPF y no Web :material-arrow-right:](../arquitectura/adr-wpf.md)

### 4. Diseno UX / UML

Se trazan diagramas de interaccion y bocetos visuales que garantizan que el usuario final no experimentara friccion al usar el sistema.

[Ver diagramas :material-arrow-right:](diagramas.md)
