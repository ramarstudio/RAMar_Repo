# Guía de Instalación Definitiva 🚀

Esta guía está diseñada para que cualquier persona, sin importar su nivel técnico, pueda desplegar el **RAMar Attendance System** de forma exitosa en menos de 5 minutos.

---

## 📋 Fase 1: Lo que necesitas tener instalado

Antes de tocar el código, asegúrate de tener estas sólidas bases en tu PC:

| Software | ¿Por qué? | Link Directo |
| :--- | :--- | :--- |
| **.NET 8 SDK** | Es el corazón del panel de control. | [Descargar x64](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **Python 3.12** | Es el cerebro de la Inteligencia Facial. | [Descargar](https://www.python.org/downloads/) |
| **PostgreSQL 15+** | Es la memoria donde se guardan los datos. | [Descargar](https://www.postgresql.org/download/windows/) |

!!! important "⚠️ Nota Crítica para Python"
    Al instalar Python, **DEBES marcar la casilla "Add Python to PATH"** en la primera ventana del instalador. Sin esto, el asistente no podrá arrancar.

---

## 🏗️ Fase 2: Preparar la Base de Datos

El sistema guardará todo en una base de datos local. Sigue estos pasos:

1. Abre **pgAdmin 4** (se instaló con PostgreSQL).
2. Conéctate con tu contraseña maestra.
3. Haz clic derecho en **Databases** -> **Create** -> **Database...**
4. En el nombre, escribe exactamente: `AttendanceSystem`
5. Dale a **Save**. ¡Listo! Ya puedes cerrar pgAdmin.

---

## 🪄 Fase 3: Despliegue en un clic con `iniciar.bat`

Ahora viene la magia. Hemos automatizado 15 pasos técnicos en un solo archivo inteligente.

1. **Descarga o clona** el repositorio en una carpeta limpia.
2. Entra a la carpeta y busca el archivo llamado **`iniciar.bat`**.
3. Haz **Doble Clic** sobre él.

### ¿Qué sucederá en la pantalla negra?

1. **Validación**: El script revisará que tengas .NET y Python instalados.
2. **Tu Contraseña**: El script se detendrá y te pedirá: *">> Ingrese la CONTRASEÑA de su base de datos PostgreSQL:"*. Escríbela y dale a **Enter**.
3. **Autoconfiguración**: El asistente inyectará esa clave en los archivos internos por ti. **(Cero edición manual de JSON)**.
4. **Instalación IA**: Se abrirá una pequeña descarga automática de las librerías de IA. Esto ocurre solo la primera vez.
5. **¡Éxito!**: Verás el logotipo de RAMar y la aplicación se abrirá sola.

---

## 💡 Fase 4: Primer Inicio de Sesión

Una vez abierta la aplicación azul de RAMar, ingresa con estas credenciales:

- **Usuario:** `admin`
- **Contraseña:** `admin123`

!!! tip "Primeros Pasos Recomendados"
    1. Ve a **Configuración** y cambia tu contraseña.
    2. Ve a **Usuarios** y registra a tu primer empleado.
    3. Ve a **Registro Facial** y captura su rostro para activar la biometría.

---

### 🛠️ ¿Problemas? (Checklist de Seguridad)

- [ ] **"Error de Biometría"**: Instala el [Visual C++ Redistributable](https://aka.ms/vs/17/release/vc_redist.x64.exe). Es necesario para el motor ONNX.
- [ ] **"Base de datos no encontrada"**: Revisa que la base se llame `AttendanceSystem` (respetando mayúsculas/minúsculas).
- [ ] **"Cámara no abre"**: Asegúrate de que ninguna otra app (Zoom, Teams) esté usando la cámara en ese momento.
