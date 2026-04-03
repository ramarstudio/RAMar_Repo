# Instalación

Elige la ruta según tu perfil.

---

## ¿Cuál es tu caso?

=== "Soy usuario final o administrador"

    Solo quiero instalar el sistema y usarlo. No me interesa el código.

    **Tiempo estimado: 10–15 minutos (la primera vez)**

    1. Lee los [requisitos del equipo](requisitos.md)
    2. Sigue la [guía paso a paso — Usuario](guia.md#guia-usuario)

=== "Soy técnico o desarrollador"

    Quiero desplegar, configurar o modificar el sistema.

    **Tiempo estimado: 5–10 minutos**

    1. Revisa los [requisitos técnicos](requisitos.md)
    2. Sigue la [guía técnica](guia.md#guia-tecnica)

---

## Resumen del proceso

```mermaid
graph TD
    A[Instalar PostgreSQL, .NET 8, Python 3.10-3.12] --> B[Doble clic en iniciar.bat]
    B --> C{Primera vez?}
    C -- Sí --> D[Ingresar contraseña de PostgreSQL]
    D --> E[Script crea BD, instala IA automáticamente]
    E --> F[App abre. Login: admin / admin123]
    C -- No --> F
```

---

!!! success "Sin editar archivos de configuración"
    El script `iniciar.bat` configura todo automáticamente. No necesitas abrir ningún archivo JSON ni terminal.
