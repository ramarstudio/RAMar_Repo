# Metodología SDLC

Se aplica un ciclo de vida de desarrollo de software riguroso para asegurar calidad corporativa antes de escribir código.

---

## Fases de producción

### 1. Identificación de casos de uso

Se traducen las necesidades operativas de la empresa en historias técnicas y medibles. Se modela qué debe hacer cada actor (administrador, empleado) y cuáles son sus límites.

### 2. Priorización MoSCoW

Antes de cualquier presupuesto, se prioriza el alcance:

| Categoría | Significado | Acción |
|---|---|---|
| **Must** | Obligatorio | Se construye sin excepciones |
| **Should** | Recomendado | Se incluye si hay tiempo |
| **Could** | Opcional | Se evalúa para futuras versiones |
| **Won't** | Descartado | No se construye en este MVP |

!!! warning "Feature creep"
    La priorización MoSCoW previene la acumulación de funcionalidades innecesarias que desbordan presupuestos y plazos.

### 3. Decisiones de arquitectura (ADR)

Se evalúan objetivamente las alternativas tecnológicas y se **justifica por escrito** la decisión final mediante un Architecture Decision Record.

Véase: [WPF vs Web (ADR)](../arquitectura/adr-wpf.md)

### 4. Diseño UX / UML

Se trazan diagramas de interacción y bocetos visuales que garantizan que el usuario final no experimentará fricción al usar el sistema.

Véase: [Diagramas](diagramas.md)
