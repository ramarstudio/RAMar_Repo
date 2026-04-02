---
icon: material/rocket-launch
---

# Guia paso a paso

## 1. Clonar el repositorio

```bash
git clone https://github.com/ramarstudio/RAMar_Repo.git
cd RAMar_Repo
```

---

## 2. Configurar PostgreSQL

Crear la base de datos y el usuario con permisos completos:

```sql
CREATE DATABASE attendance_db;
CREATE USER attendance_user WITH PASSWORD 'tu_password_seguro';
GRANT ALL PRIVILEGES ON DATABASE attendance_db TO attendance_user;
```

---

## 3. Variables de entorno

Crear el archivo `.env` en `AttendanceSystem/`:

```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=attendance_db
DB_USER=attendance_user
DB_PASSWORD=tu_password_seguro
```

!!! danger "Seguridad"
    Nunca subas el archivo `.env` al repositorio. Ya esta incluido en `.gitignore`.

---

## 4. Dependencias de Python

```bash
cd AttendanceSystem/src/FaceService
pip install -r requirements.txt
```

!!! info "Primera ejecucion"
    La primera vez se descargan automaticamente los modelos de InsightFace (~200 MB). Esto ocurre **una sola vez**.

---

## 5. Compilar la aplicacion

```bash
cd AttendanceSystem
dotnet build
```

---

## 6. Ejecutar

```bash
dotnet run --project src/AttendanceSystem.App
```

La aplicacion WPF se abrira. El motor de reconocimiento facial se iniciara **automaticamente** cuando se necesite — no es necesario ejecutar Python manualmente.

---

## Primer uso

<ul class="step-list">
<li>Inicia sesion con las credenciales de administrador por defecto</li>
<li>Cambia la contrasena inmediatamente desde el panel</li>
<li>Registra los empleados desde <strong>Usuarios</strong></li>
<li>Captura el rostro de cada empleado desde <strong>Registro Facial</strong></li>
<li>Los empleados ya pueden marcar asistencia con su rostro</li>
</ul>

!!! tip "Credenciales iniciales"
    El sistema crea un usuario `admin` con rol SuperAdmin en la primera ejecucion. Consulta la documentacion interna para la contrasena inicial.
