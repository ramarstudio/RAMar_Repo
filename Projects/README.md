# 📚 Centro de Especificaciones y Casos de Uso (Área de Planning)

Bienvenido al directorio de **Planificación e Investigación** de RAMar Software Studio. Toda gran aplicación comienza con un planeamiento estricto.

Este directorio no contiene código fuente. Este es el espacio de **Ingeniería de Requisitos, Análisis Funcional y Arquitectura de Sistemas**. Aquí se almacena toda la estructuración teórica, modelado de procesos e investigación de producto que sostienen nuestros sistemas mucho antes de escribir la primera línea de código.

---

## ⚙️ Metodología de Desarrollo Institucional (SDLC)

En RAMar Studio aplicamos un ciclo de vida de desarrollo de software riguroso para asegurar la máxima calidad de entrega empresarial:

1. **Identificación de Casos de Uso:** Traducimos el problema del cliente en historias de usuario técnicas y medibles.
2. **Matriz MoSCoW estricta:** Priorizamos el alcance dividiendo los módulos en *Must* (Obligatorio), *Should* (Recomendado), *Could* (Opcional) y *Won't* (Descartado para el MVP). Esto previene sobrecostos y retrasos.
3. **Decisión de Arquitectura Transaccional:** Se evalúan las tecnologías (Web, Móvil, Cloud, Escritorio) y se escoge y justifica por escrito el ecosistema técnico que brinde mayor "Rendimiento vs Seguridad".
4. **Diseño UML y UX:** Se trazan los diagramas de interacción y bocetos.

---

## 📸 Caso de Estudio: Control de Asistencia Biométrico (Attendance System)

### El Planteamiento del Problema
Desarrollar una infraestructura de auditoría de personal capaz de procesar **reconocimiento facial de alta precisión totalmente offline** para entornos corporativos rigurosos. El reto radicaba en erradicar métodos de suplantación de identidad sin violar políticas modernas de retención visual y lograrlo consumiendo la mínima fracción de CPU en las computadoras estándar de Recursos Humanos.

### Biblioteca de Documentación Aprobada:

#### 📄 A. Ingeniería de Requisitos y Limitaciones (Scope)
El documento funcional donde se plasmaron las reglas físicas y lógicas del producto final. Aquí se documentó explícitamente la necesidad atómica de transitar hacia un "Padrón Biométrico Encriptado", descartando módulos innecesarios como el pago de nóminas o geolocalizaciones complejas a favor de la pura eficiencia biométrica y la velocidad transaccional dentro de red local.
👉 **[LEER DOCUMENTO: Requisitos de Ingeniería y Tablas de Alcance (MoSCoW)](./Employee_Attendance_Tracker/requirements/requirements.md)**

#### 🏗️ B. Arquitectura Oficial e Infraestructura (ADR)
Memoria técnica que relata el ensayo y pivotaje clave que dio vida al proyecto. Este texto analiza la competencia técnica entre construir un servidor "Web clásico" versus construir un "Microservicio Híbrido C#+Python (WPF Nativo)". Detallando finalmente la aplastante victoria del modelo de escritorio nativo debido al nulo Overhead de Red HTTP en procesos de inyectado fotograma a fotograma (Live Feed Camera).
👉 **[LEER DOCUMENTO: Sustentación Final de Arquitectura (Web vs WPF)](./Employee_Attendance_Tracker/architecture/proposal.md)**

#### 🖼️ C. Diagramas Topológicos y UML Vectorial
Depósito visual que alberga la fundamentación humana y gráfica del negocio. Contiene mapas estructurales de interacción orientada a usuarios, definiendo los privilegios exclusivos para el **Administrador/RRHH** frente a la interfaz quiosquera e involuntaria a la que se expone el sujeto base: el **Empleado**.
👉 **[EXPLORAR CARPETA: Diagramas Arquitectónicos Auxiliares y Casos de Uso (UML)](./Employee_Attendance_Tracker/architecture/)**

---

*⚠️ La información contenida en estos documentos refleja el "Plano de Construcción" (Blueprint) intelectual oficial de RAMar Studio.*
