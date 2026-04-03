# Guía de Instalación (Zero-Touch)

Hemos diseñado el sistema para que **cualquier persona pueda arrancarlo en menos de 5 minutos** sin necesidad de conocimientos técnicos avanzados.

## 1. Clonar el repositorio

```bash
git clone https://github.com/ramarstudio/RAMar_Repo.git
cd RAMar_Repo
```

---

## 2. Un solo clic: Arrancar `iniciar.bat`

Dirígete a la carpeta `RAMar_Repo` y haz **doble clic en el archivo `iniciar.bat`**.

El asistente inteligente hará todo lo siguiente de forma automática:
1. **Verificará** si tienes instalado `.NET 8` y `Python`.
2. **Configurará tu base de datos**: El script te preguntará la contraseña de tu PostgreSQL en la misma ventana negra y **la inyectará automáticamente** en el archivo de configuración.
3. **Preparará la IA**: Creará un entorno virtual aislado y descargará el motor de reconocimiento facial compatible con tu PC.
4. **Lanzará la Aplicación**: Compilará y abrirá el panel de control.

---

## 3. Requisitos previos (Solo si no los tienes)

Si el asistente te indica que falta software, simplemente instala estos tres programas:

- [**.NET 8 SDK**](https://dotnet.microsoft.com/download/dotnet/8.0) — descarga el "SDK" x64.
- [**Python 3.10+**](https://www.python.org/downloads/) — **IMPORTANTE**: marca "Add Python to PATH" al instalar.
- [**PostgreSQL 15+**](https://www.postgresql.org/download/windows/) — al instalarlo, ponle una contraseña que recuerdes.

!!! tip "Sobre la Base de Datos"
    Antes de correr el script por primera vez, asegúrate de tener creada una base de datos vacía llamada **`AttendanceSystem`** en tu PostgreSQL (puedes crearla usando pgAdmin).

---

## Primer uso en el Panel

Una vez abierta la aplicación, usa las credenciales maestras:

| Campo | Valor |
|---|---|
| **Usuario** | `admin` |
| **Contraseña** | `admin123` |

!!! warning "Seguridad"
    Cambia la contraseña inmediatamente desde el panel de administración tras ingresar por primera vez.
