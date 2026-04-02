⚠️ **Confidential & Sensitive Content**  
Este repositorio contiene material confidencial exclusivo correspondiente a los procesos de desarrollo y arquitecturas de **RAMar Software Studio**. 

---

> 🔥 **¡NUEVO PORTAL LIBRERÍA DE DOCUMENTACIÓN! (Tipo .io)**  
> Te invitamos a leer toda la Ingeniería de este código sin saltar de README en README. Hemos implementado un sitio web interactivo con barras laterales, indexación y barra de búsqueda interna (Desplegable y alojable en formato `tu-usuario.github.io`).
> 
> **Cómo iniciar tu libro interactivo (Local):**
> 1. Abre tu terminal.
> 2. Asegura tener instalado el constructor visual: `pip install mkdocs-material`
> 3. Ejecuta en esta misma carpeta: `mkdocs serve`
> 4. Entra al link que aparece en consola (generalmente `http://localhost:8000`) y enamórate de la documentación.

---

<div align="center">
  <h1>🚀 RAMar Software Studio</h1>
  <p><em>Innovación, Privacidad Computacional y Construcción de Soluciones Corporativas</em></p>
</div>
  <p><em>Innovación, Privacidad Computacional y Construcción de Soluciones Corporativas</em></p>
</div>

---

## 🏢 ¿Quiénes Somos?
Somos un estudio de Ingeniería de Software dedicado a construir y orquestar herramientas digitales corporativas. Analizamos cada requerimiento operativo y de negocio a profundidad para entregar plataformas diseñadas inteligentemente desde sus cimientos. Codificamos ecosistemas sólidos, ágiles y altamente funcionales adaptados a las exigencias corporativas reales, generando tecnología que permite a las empresas y MYPES escalar con total control y seguridad.

## ⚙️ ¿Cómo Trabajamos?
Nuestro modelo de desarrollo se rige por la integridad profesional, el conocimiento técnico y la transparencia total:
1. **Resolución Directa y Honesta:** No sobredimensionamos herramientas ni vendemos tecnologías innecesarias. Evaluamos el problema real de cada empresa y estructuramos la tecnología precisa que resuelve esa fricción de manera eficiente.
2. **Ingeniería Antes de Codificar:** No trabajamos a ciegas ni improvisamos. Trazamos los requerimientos, diseñamos la experiencia visual (UX) y validamos las reglas del negocio en conjunto con el cliente antes de que los desarrolladores pasen a producción.
3. **Desarrollo Íntegro y Seguro:** Adoptamos la protección de datos como una norma ética inquebrantable. Cuidamos cada bloque de código y base de datos para asegurar que la información de las empresas esté operando sobre un piso sólido y seguro.

## 💡 ¿Qué Proponemos?
Proponemos un abanico de soluciones de software altamente auditables para modernizar negocios de manera eficiente. Nuestra meta es optimizar y automatizar departamentos enteros —como el control de recursos y tiempos— garantizando flujos de trabajo inteligentes, interfaces amigables al usuario y arquitecturas limpias y mantenibles.

---

## 📸 Proyecto Central: Sistema de Control de Asistencia Biométrico

Hemos desarrollado desde cero una aplicación de escritorio diseñada para registrar y gestionar la asistencia del personal de manera automática. Este sistema reconoce los rostros de los trabajadores al instante, frenando la falsificación de identidades (el clásico "marcar por un amigo") y protegiendo totalmente la privacidad corporativa.

### 🎯 Lo que resuelve este sistema (Perfil Comercial / Negocios)
Esta sección expone cómo el proyecto impacta positivamente en tu empresa, en un lenguaje sencillo y directo:
* **Velocidad 100% Sin Internet:** El sistema no se "cuelga" ni falla si el internet de la oficina se corta, porque opera localmente en la propia computadora.
* **Reportes y Administración Fácil:** Recursos Humanos cuenta con un panel sumamente intuitivo donde, a simple vista, sabe a qué hora entró el personal, visualiza retardos y maneja altas de empleados.
* **Privacidad y Cero Fotografías:** Protegemos legalmente a la empresa. Nunca guardamos fotos del personal; el sistema transforma el rostro humano en un código matemático que, si un hacker se robara, no le serviría de nada.
👉 **[VER CÓMO FUNCIONA ESTE NEGOCIO: Entra aquí para leer los Planos y Reglas de la Solución](./Projects/README.md)**

### ⚙️ Cómo está construido (Perfil Ingeniería / Desarrollador)
Esta sección expone el "Bajo el capó" de la aplicación. Es decir, con qué herramientas especializadas logramos construir un software tan rápido y seguro:
* **El Orquestador (Aplicación Visual):** Escrito en **C# .NET 8 (WPF)** con el Patrón MVVM. Al ser nativo de Windows, logra atrapar el video de la cámara usando directamente el hardware, casi sin consumir el procesador (CPU).
* **El Motor de Reconocimiento:** Un microservicio local encapsulado en **Python (FastAPI)** que utiliza potentes modelos matemáticos de Inteligencia Artificial para extraer los vectores del rostro.
* **La Bóveda de Datos:** Una base estricta y blindada orquestada mediante **PostgreSQL** y *Entity Framework Core*, asegurando que el registro de entradas y salidas sea inalterable.
👉 **[VER CÓDIGO Y ARQUITECTURA: Entra aquí para explorar el Software y sus Lenguajes](./AttendanceSystem/README.md)**

---

<br>

> **Legal & Copyright:**  
> This repository is intended **exclusively for storage, administration, and management purposes**.  
> It is **not meant for public distribution, reuse, or external reference**. Unauthorized access, disclosure, or use of the information contained in this repository is strictly prohibited.  
> © RAMar Software Studio. All rights reserved.
