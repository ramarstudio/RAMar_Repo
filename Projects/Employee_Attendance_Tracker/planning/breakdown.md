# Cronograma del Proyecto — MVP

Este documento presenta el cronograma de trabajo correspondiente al desarrollo del MVP de la aplicación nativa de control de asistencia. El cronograma detalla las actividades, duraciones, dependencias y fechas estimadas, sirviendo como base de planificación, seguimiento y control del proyecto.

---

## 1. Fase de Requisitos

| ID  | Tarea                                | Duración | Inicio           | Terminado        | Predecesores |
|-----|--------------------------------------|----------|-----------------|-----------------|--------------|
| 1   | Análisis de requisitos               | 6 days   | 26/12/25 08:00  | 02/01/26 17:00  | —            |
| 1.1 | Recopilación de requisitos           | 2 days   | 26/12/25 08:00  | 29/12/25 17:00  | —            |
| 1.2 | Identificación de actores y RF       | 1 day    | 30/12/25 08:00  | 30/12/25 17:00  | 1.1          |
| 1.3 | Documentación de casos de uso (MVP) | 2 days   | 31/12/25 08:00  | 01/01/26 17:00  | 1.2          |
| 1.4 | Validación de requisitos             | 1 day    | 02/01/26 08:00  | 02/01/26 17:00  | 1.3          |

**⏱ Duración total:** 6 días

---

## 2. Fase de Diseño

| ID  | Tarea                               | Duración | Inicio           | Terminado        | Predecesores |
|-----|-------------------------------------|----------|-----------------|-----------------|--------------|
| 2   | Diseño del sistema                  | 4 days   | 05/01/26 08:00  | 08/01/26 17:00  | 1.4          |
| 2.1 | Arquitectura del software           | 2 days   | 05/01/26 08:00  | 06/01/26 17:00  | 1.4          |
| 2.2 | Modelo de datos del sistema         | 2 days   | 07/01/26 08:00  | 08/01/26 17:00  | 2.1          |
| 2.3 | Flujo de reconocimiento facial      | 1 day    | 07/01/26 08:00  | 07/01/26 17:00  | 2.1          |
| 2.4 | Mockups UI MVP                      | 2 days   | 07/01/26 08:00  | 08/01/26 17:00  | 2.1          |

**⏱ Duración total:** 4 días

---

## 3. Fase de Implementación

| ID  | Tarea                               | Duración | Inicio           | Terminado        | Predecesores |
|-----|-------------------------------------|----------|-----------------|-----------------|--------------|
| 3   | Implementación del MVP              | 11 days  | 09/01/26 08:00  | 23/01/26 17:00  | 8,10         |
| 3.1 | Configuración del entorno local     | 1 day    | 09/01/26 08:00  | 09/01/26 17:00  | 8            |
| 3.2 | Backend base                        | 4 days   | 12/01/26 08:00  | 15/01/26 17:00  | 12           |
| 3.3 | Lógica de negocio de asistencias    | 3 days   | 16/01/26 08:00  | 20/01/26 17:00  | 13           |
| 3.4 | Gestión de usuarios y roles         | 2 days   | 16/01/26 08:00  | 19/01/26 17:00  | 13           |
| 3.5 | Frontend MVP                        | 4 days   | 09/01/26 08:00  | 14/01/26 17:00  | 10           |
| 3.6 | Integración reconocimiento facial   | 3 days   | 21/01/26 08:00  | 23/01/26 17:00  | 14,16        |
| 3.7 | Auditoría y trazabilidad            | 1 day    | 21/01/26 08:00  | 21/01/26 17:00  | 14           |

**⏱ Duración total:** 11 días

---

## 4. Fase de Pruebas

| ID  | Tarea                               | Duración | Inicio           | Terminado        | Predecesores |
|-----|-------------------------------------|----------|-----------------|-----------------|--------------|
| 4   | Pruebas del sistema                 | 6 days   | 26/01/26 08:00  | 02/02/26 17:00  | 17,18        |
| 4.1 | Pruebas unitarias                   | 2 days   | 26/01/26 08:00  | 27/01/26 17:00  | 14,15        |
| 4.2 | Pruebas de integración              | 2 days   | 28/01/26 08:00  | 29/01/26 17:00  | 17,20        |
| 4.3 | Pruebas funcionales (RF)           | 2 days   | 30/01/26 08:00  | 02/02/26 17:00  | 21           |
| 4.4 | Pruebas de rendimiento              | 1 day    | 30/01/26 08:00  | 30/01/26 17:00  | 21           |

**⏱ Duración total:** 6 días

---

## 5. Entrega

| ID  | Tarea                              | Duración | Inicio           | Terminado        | Predecesores |
|-----|------------------------------------|----------|-----------------|-----------------|--------------|
| 5   | Entrega del MVP                    | 4 days   | 03/02/26 08:00  | 06/02/26 17:00  | 22,23        |
| 5.1 | Documentación final                | 2 days   | 03/02/26 08:00  | 04/02/26 17:00  | 22           |
| 5.2 | Preparación presentación final     | 1 day    | 05/02/26 08:00  | 05/02/26 17:00  | 25           |
| 5.3 | Demostración del MVP               | 1 day    | 06/02/26 08:00  | 06/02/26 17:00  | 26           |

