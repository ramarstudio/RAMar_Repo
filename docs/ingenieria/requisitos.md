# Requisitos — Matriz MoSCoW

Documento funcional donde quedan plasmadas las reglas del producto final. Define las obligaciones del sistema y limita el alcance para lograr un MVP enfocado y eficiente.

---

## Matriz de priorización

| ID | Requisito | Descripción | Prioridad |
|---|---|---|:---:|
| RF01 | Marcado de entrada | Registrar el ingreso del empleado al inicio de su jornada | **Must** |
| RF02 | Marcado de salida | Registrar la salida al finalizar la jornada | **Must** |
| RF03 | Marcado de breaks | Registrar inicio y fin de pausas durante la jornada | **Must** |
| RF04 | Múltiples marcajes | Permitir registrar varios eventos de asistencia por día | **Must** |
| RF05 | Límite de marcaje | Restringir a un único marcaje principal de entrada diario | **Must** |
| RF06 | Registro fuera de horario | Permitir marcajes fuera del horario asignado | **Must** |
| RF07 | Etiqueta tardanza | Identificar automáticamente marcajes tardíos | **Must** |
| RF08 | Tolerancia configurable | Configurar minutos de tolerancia según políticas empresariales | **Must** |
| RF09 | Sistema local | Operar únicamente dentro de la red local corporativa | **Must** |
| RF10 | Sincronización inmediata | Guardar marcajes de forma inmediata en el servidor local | **Must** |
| RF11 | Persistencia local | Almacenar marcajes temporalmente ante fallos del servidor | **Must** |
| RF12 | Reconocimiento facial | Validar identidad mediante cámara web local | **Must** |
| RF13 | No hardware externo | Excluir el uso de lectores biométricos dedicados | **Must** |
| RF14 | App de escritorio | Ejecutable nativo WPF con interfaz moderna | **Must** |
| RF15 | Tiempo de marcado | Completar el marcaje en menos de 30 segundos | **Must** |
| RF18 | Almacenamiento biométrico | Guardar datos biométricos en infraestructura local | **Must** |
| RF19 | Cifrado de datos | Proteger datos biométricos en tránsito y reposo (AES) | **Must** |
| RF21 | Cumplimiento normativo | Cumplir regulaciones de protección de datos personales | **Must** |
| RF22 | Rol empleado | Restringir al empleado a funciones de marcaje únicamente | **Must** |
| RF23 | Rol administrador | Permitir al admin gestionar usuarios y marcajes | **Must** |
| RF36 | No trabajo remoto | Bloquear marcajes fuera del entorno local | **Must** |
| RF37 | Geolocalización | Implementar validación por ubicación GPS | **Won't** |
| RF38 | Roles intermedios | Incorporar sub-managers o roles adicionales | **Won't** |
| RF39 | Gestión de pagos | Calcular pagos, planillas o sueldos | **Won't** |

---

## Restricciones clave

!!! warning "Límites del sistema"
    - Sistema confinado estrictamente a la red corporativa
    - **No** se almacenan fotografías, solo vectores matemáticos
    - Respuestas biométricas en menos de **1 segundo** de latencia
    - Módulos financieros y de geolocalización quedan **fuera del alcance**
