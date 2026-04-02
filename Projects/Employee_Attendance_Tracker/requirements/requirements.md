# Especificación de Requisitos de Software
## Sistema de Control de Asistencia Local con Reconocimiento Facial

---

## 1. Introducción

### 1.1 Propósito
Este documento define formalmente los requisitos funcionales del sistema de control de asistencia basado en una aplicación nativa de escritorio (C# WPF), orientado a empresas que requieren registrar asistencia diaria sin utilizar hardware biométrico externo corporativo.

### 1.2 Alcance
El sistema permitirá registrar asistencia mediante computadoras locales dentro de la red corporativa, priorizando rapidez operativa, facilidad de uso y control administrativo, con reconocimiento facial como mejora futura.

---

## 2. Descripción General

### 2.1 Perspectiva del producto
El sistema es una aplicación nativa de Windows diseñada para operar sobre una red local en sincronía con una base de datos relacional (PostgreSQL) y un script-microservicio (FastAPI/Python), optimizando el rendimiento frente a la cámara local sin latencias de red.

### 2.2 Clases de usuarios
- **Empleado**: Usuario autorizado para realizar marcajes de asistencia.
- **Administrador**: Usuario responsable de la gestión, supervisión y auditoría del sistema.

### 2.3 Suposiciones y dependencias
- Las estaciones de trabajo cuentan con cámara web operativa.
- La empresa dispone de una red local confiable.
- El consentimiento para el uso de datos biométricos está formalizado contractualmente.

---

## 3. Requisitos Funcionales

### 3.1 Requisitos Funcionales Mandatorios

- Registrar marcajes de entrada y salida asociados a cada empleado.
- Permitir múltiples marcajes diarios correspondientes a breaks y pausas.
- Validar la identidad del empleado sin requerir dispositivos biométricos externos.
- Clasificar automáticamente los marcajes realizados fuera del horario asignado como tardanza.
- Evitar marcajes duplicados consecutivos del mismo tipo por empleado.
- Permitir a los administradores editar marcajes bajo control y autorización.
- Registrar auditoría completa de cualquier modificación realizada sobre marcajes.
- Gestionar usuarios diferenciando roles de administrador y empleado.
- Permitir la deshabilitación temporal de empleados sin eliminar su historial.
- Excluir explícitamente el cálculo y gestión de horas extra.
- Bloquear cualquier intento de marcaje fuera de las instalaciones.

---

### 3.2 Requisitos Funcionales de Mejora

- Incorporar reconocimiento facial utilizando cámaras web locales.
- Registrar el consentimiento explícito del empleado para el uso biométrico.
- Generar rankings de puntualidad para análisis interno de desempeño.
- Permitir la configuración de tolerancias de tardanza por empresa.
- Habilitar registro asistido por un administrador en casos excepcionales.
- Permitir al empleado consultar su historial de asistencias.
- Integrarse con sistemas locales de nómina para exportación de datos.
- Implementar detección básica de intentos de suplantación biométrica.

---

### 3.3 Requisitos Funcionales Fuera de Alcance

Las siguientes funcionalidades han sido identificadas explícitamente como fuera del alcance del sistema y no serán consideradas dentro del desarrollo del MVP:

- Gestión de cálculo de horas extra y pagos de planilla.
- Registro de asistencia en modalidad de trabajo remoto.
- Uso de geolocalización para validar presencia física.
- Administración de roles intermedios adicionales.
- Corrección automática de condiciones de iluminación ambiental.
- Reconocimiento facial con accesorios como lentes o mascarillas.

---

## 4. Tabla Consolidada de Requisitos Funcionales (MoSCoW)

| ID  | Requisito | Descripción clara | MoSCoW |
| --- | --------- | ----------------- | :----: |
| RF01 | Marcado de entrada | Registrar el ingreso del empleado al inicio de su jornada laboral | Must |
| RF02 | Marcado de salida | Registrar la salida del empleado al finalizar su jornada | Must |
| RF03 | Marcado de breaks | Registrar inicio y fin de pausas durante la jornada | Must |
| RF04 | Múltiples marcajes | Permitir registrar varios eventos de asistencia por día | Must |
| RF05 | Límite de marcaje | Restringir a un único marcaje principal de entrada diario | Must |
| RF06 | Registro fuera de horario | Permitir marcajes fuera del horario asignado | Must |
| RF07 | Etiqueta tardanza | Identificar automáticamente marcajes tardíos | Must |
| RF08 | Tolerancia configurable | Configurar minutos de tolerancia según políticas empresariales | Must |
| RF09 | Sistema local | Operar únicamente dentro de la red local corporativa | Must |
| RF10 | Sincronización inmediata | Guardar marcajes de forma inmediata en el servidor local | Must |
| RF11 | Persistencia local | Almacenar marcajes temporalmente ante fallos del servidor | Must |
| RF12 | Reconocimiento facial | Validar identidad mediante cámara web local | Must |
| RF13 | No hardware externo | Excluir el uso de lectores biométricos dedicados | Must |
| RF14 | App de Escritorio | Permitir uso mediante un ejecutable nativo WPF con UI moderno | Must |
| RF15 | Tiempo de marcado | Completar el marcaje en menos de 30 segundos | Must |
| RF16 | Concurrencia mínima | Soportar múltiples usuarios simultáneos | Must |
| RF17 | Límite de intentos | Gestionar intentos consecutivos de marcaje | Should |
| RF18 | Almacenamiento biométrico | Guardar datos biométricos en infraestructura local | Must |
| RF19 | Cifrado de datos | Proteger datos biométricos en tránsito y reposo | Must |
| RF20 | Consentimiento | Registrar consentimiento del empleado | Must |
| RF21 | Cumplimiento normativo | Cumplir regulaciones de protección de datos personales | Must |
| RF22 | Rol empleado | Restringir al empleado a funciones de marcaje | Must |
| RF23 | Rol administrador | Permitir al administrador gestionar usuarios y marcajes | Must |
| RF24 | Edición de marcajes | Autorizar modificaciones administrativas de marcajes | Must |
| RF25 | Trazabilidad | Auditar quién, cuándo y por qué se modificó un registro | Must |
| RF26 | Deshabilitar empleados | Desactivar empleados sin eliminar su información histórica | Must |
| RF27 | Confirmación visual | Mostrar confirmación clara tras cada marcaje | Must |
| RF28 | Mensajes de error | Mostrar mensajes comprensibles ante fallos | Must |
| RF29 | Marcado asistido | Permitir registro manual por administrador autorizado | Should |
| RF30 | Ranking puntualidad | Generar ranking interno de puntualidad | Could |
| RF31 | Reporte asistencia | Generar reportes de asistencia por periodo | Must |
| RF32 | Exportación datos | Exportar datos para uso administrativo | Must |
| RF33 | Integración nómina | Integrarse con sistemas locales de nómina | Should |
| RF34 | Historial empleado | Permitir consulta del historial personal | Should |
| RF35 | Anti-spoofing básico | Detectar intentos simples de suplantación | Should |
| RF36 | No trabajo remoto | Bloquear marcajes fuera del entorno local | Must |
| RF37 | Geolocalización | Implementar validación por ubicación | Won’t |
| RF38 | Roles intermedios | Incorporar roles adicionales | Won’t |
| RF39 | Gestión de pagos | Calcular pagos o planilla | Won’t |

## 5. Restricciones

- Operación exclusiva dentro de la red local de la empresa.
- No dependencia de servicios externos ni conectividad a internet.
- No uso de dispositivos biométricos dedicados.
- Tiempo máximo de respuesta aceptable para el proceso de marcaje.
- Cumplimiento estricto de normativas de protección de datos personales.
- Almacenamiento y procesamiento de información sensible únicamente en infraestructura local.

---

## 6. Aprobación
Este documento constituye la base formal para el diseño, desarrollo y validación del sistema.