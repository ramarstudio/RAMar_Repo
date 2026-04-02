# Control de Asistencia Biometrico

El **Control de Asistencia Biometrico** es un sistema de escritorio que registra la entrada y salida del personal mediante reconocimiento facial. Opera 100% en la red local, sin internet y sin almacenar fotografias.

---

## Caracteristicas

| Caracteristica | Detalle |
|---|---|
| **Reconocimiento facial** | Identificacion en menos de 1 segundo con InsightFace (ArcFace) |
| **Privacidad** | Cero fotos almacenadas — solo vectores matematicos cifrados con AES-256 |
| **Sin internet** | Funciona completamente offline en la red local de la empresa |
| **Panel admin** | Dashboard, empleados, horarios, marcajes, reportes, auditoria |
| **Roles** | Empleado, Administrador, SuperAdministrador |
| **Configurable** | Tolerancia de tardanzas, horarios, parametros del sistema |

---

## Stack tecnologico

| Componente | Tecnologia |
|---|---|
| Aplicacion de escritorio | C# .NET 8 (WPF) |
| Motor de reconocimiento facial | Python 3.13 + FastAPI + InsightFace |
| Base de datos | PostgreSQL + Entity Framework Core |
| Cifrado biometrico | AES-256 |
| Comunicacion interna | HTTP localhost:8000 |

---

## Arquitectura

```mermaid
graph LR
    A[Empleado] --> B[Camara]
    B --> C[App WPF - C#]
    C -->|HTTP local| D[Motor IA - Python]
    D -->|Embedding 512d| C
    C --> E[(PostgreSQL)]
    C --> F[Resultado en pantalla]
```

**Flujo de marcaje:**

1. El empleado se acerca a la camara
2. La app WPF captura el frame y lo envia al motor Python
3. Python genera un embedding facial de 512 dimensiones
4. La app compara el embedding contra los registrados en PostgreSQL
5. Se muestra el resultado (aprobado/denegado) y se registra el marcaje

---

## Comenzar

- **[Que es este producto](producto/index.md)** — vision general, para quien es, diferenciadores
- **[Guia de instalacion](instalacion/index.md)** — requisitos y pasos para desplegar
- **[Arquitectura tecnica](arquitectura/index.md)** — como esta construido internamente
- **[Ingenieria](ingenieria/index.md)** — metodologia, requisitos y diagramas

---

> **RAMar Software Studio** — Innovacion, privacidad computacional y soluciones corporativas.
