# Guia paso a paso

## 1. Clonar el repositorio

```bash
git clone https://github.com/ramarstudio/RAMar_Repo.git
cd RAMar_Repo
```

## 2. Configurar la base de datos

Crear una base de datos PostgreSQL y un usuario con permisos:

```sql
CREATE DATABASE attendance_db;
CREATE USER attendance_user WITH PASSWORD 'tu_password_seguro';
GRANT ALL PRIVILEGES ON DATABASE attendance_db TO attendance_user;
```

## 3. Variables de entorno

Crear el archivo `.env` en la raiz del proyecto `AttendanceSystem/`:

```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=attendance_db
DB_USER=attendance_user
DB_PASSWORD=tu_password_seguro
```

!!! danger "Seguridad"
    Nunca subas el archivo `.env` al repositorio. Ya esta incluido en `.gitignore`.

## 4. Instalar dependencias de Python

```bash
cd AttendanceSystem/src/FaceService
pip install -r requirements.txt
```

La primera ejecucion descargara automaticamente los modelos de InsightFace (~200 MB). Esto ocurre una sola vez.

## 5. Compilar la aplicacion

```bash
cd AttendanceSystem
dotnet build
```

## 6. Ejecutar

```bash
dotnet run --project src/AttendanceSystem.App
```

La aplicacion WPF se abrira. El motor de reconocimiento facial se iniciara automaticamente cuando se necesite.

---

## Primer uso

1. Inicia sesion con las credenciales de administrador por defecto
2. Cambia la contrasena inmediatamente
3. Registra los empleados desde el panel de administracion
4. Captura el rostro de cada empleado
5. Los empleados ya pueden marcar asistencia

!!! tip "Credenciales iniciales"
    El sistema crea un usuario `admin` en la primera ejecucion. Consulta la documentacion interna para la contrasena inicial.
