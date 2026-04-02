# Metodologia SDLC

Se aplica un ciclo de vida de desarrollo de software riguroso para asegurar calidad corporativa antes de escribir codigo.

---

## Fases de produccion

### 1. Identificacion de casos de uso

Se traducen las necesidades operativas de la empresa en historias tecnicas y medibles. Se modela que debe hacer cada actor (administrador, empleado) y cuales son sus limites.

### 2. Priorizacion MoSCoW

Antes de cualquier presupuesto, se prioriza el alcance dividiendo los modulos en:

| Categoria | Significado | Accion |
|---|---|---|
| **Must** | Obligatorio | Se construye sin excepciones |
| **Should** | Recomendado | Se incluye si hay tiempo |
| **Could** | Opcional | Se evalua para futuras versiones |
| **Won't** | Descartado | No se construye en este MVP |

Esto previene desviaciones de tiempo y sobrecostos por acumulacion de funcionalidades innecesarias (*feature creep*).

### 3. Decisiones de arquitectura (ADR)

Se evaluan objetivamente las alternativas tecnologicas (web, escritorio, movil, nube) y se **justifica por escrito** la decision final mediante un Architecture Decision Record.

Ver: [ADR — Por que WPF y no Web](../arquitectura/adr-wpf.md)

### 4. Diseno UX / UML

Se trazan diagramas de interaccion y bocetos visuales que garantizan que el usuario final no experimentara friccion al usar el sistema.

Ver: [Diagramas UML](diagramas.md)
