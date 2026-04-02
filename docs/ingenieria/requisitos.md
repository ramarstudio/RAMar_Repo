# Requisitos — Matriz MoSCoW

Documento funcional donde quedan plasmadas las reglas del producto final. Define las obligaciones del sistema y limita el alcance para lograr un MVP enfocado y eficiente.

---

## Matriz de priorizacion

| ID | Requisito | Descripcion | Prioridad |
|---|---|---|:---:|
| RF01 | Marcado de entrada | Registrar el ingreso del empleado al inicio de su jornada | **Must** |
| RF02 | Marcado de salida | Registrar la salida al finalizar la jornada | **Must** |
| RF03 | Marcado de breaks | Registrar inicio y fin de pausas durante la jornada | **Must** |
| RF04 | Multiples marcajes | Permitir registrar varios eventos de asistencia por dia | **Must** |
| RF05 | Limite de marcaje | Restringir a un unico marcaje principal de entrada diario | **Must** |
| RF06 | Registro fuera de horario | Permitir marcajes fuera del horario asignado | **Must** |
| RF07 | Etiqueta tardanza | Identificar automaticamente marcajes tardios | **Must** |
| RF08 | Tolerancia configurable | Configurar minutos de tolerancia segun politicas empresariales | **Must** |
| RF09 | Sistema local | Operar unicamente dentro de la red local corporativa | **Must** |
| RF10 | Sincronizacion inmediata | Guardar marcajes de forma inmediata en el servidor local | **Must** |
| RF11 | Persistencia local | Almacenar marcajes temporalmente ante fallos del servidor | **Must** |
| RF12 | Reconocimiento facial | Validar identidad mediante camara web local | **Must** |
| RF13 | No hardware externo | Excluir el uso de lectores biometricos dedicados | **Must** |
| RF14 | App de escritorio | Ejecutable nativo WPF con interfaz moderna | **Must** |
| RF15 | Tiempo de marcado | Completar el marcaje en menos de 30 segundos | **Must** |
| RF18 | Almacenamiento biometrico | Guardar datos biometricos en infraestructura local | **Must** |
| RF19 | Cifrado de datos | Proteger datos biometricos en transito y reposo (AES) | **Must** |
| RF21 | Cumplimiento normativo | Cumplir regulaciones de proteccion de datos personales | **Must** |
| RF22 | Rol empleado | Restringir al empleado a funciones de marcaje unicamente | **Must** |
| RF23 | Rol administrador | Permitir al admin gestionar usuarios y marcajes | **Must** |
| RF36 | No trabajo remoto | Bloquear marcajes fuera del entorno local | **Must** |
| RF37 | Geolocalizacion | Implementar validacion por ubicacion GPS | **Won't** |
| RF38 | Roles intermedios | Incorporar sub-managers o roles adicionales | **Won't** |
| RF39 | Gestion de pagos | Calcular pagos, planillas o sueldos | **Won't** |

---

## Restricciones clave

!!! warning "Limites del sistema"
    - Sistema confinado estrictamente a la red corporativa
    - **No** se almacenan fotografias, solo vectores matematicos
    - Respuestas biometricas en menos de **1 segundo** de latencia
    - Modulos financieros y de geolocalizacion quedan **fuera del alcance**
