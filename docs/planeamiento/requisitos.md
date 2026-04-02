# 📋 Análisis de Requisitos y Limitaciones (Scope)

El documento funcional donde quedan plasmadas las reglas físicas y lógicas del producto final. Aquí se documentó explícitamente la necesidad atómica del producto, impidiendo que los ingenieros o programadores gasten recursos haciendo "demasiadas cosas".

---

## Matriz Formal de MoSCoW

| ID  | Requisito | Descripción clara | Permiso |
| --- | --------- | ----------------- | :----: |
| RF01 | Marcado de entrada diario | Registrar el ingreso puntual del empleado | Must |
| RF02 | Marcado de salida diario | Registrar la salida del empleado | Must |
| RF04 | Múltiples salidas de Breack | Permitir registrar varios eventos temporales durante el día | Must |
| RF07 | Etiquetado por tardanzas | Identificar automáticamente registros de mora o falta | Must |
| RF09 | Localización de Interfaz LAN | Operar la base de la terminal desde un servidor local | Must |
| RF12 | Motor Biométrico puro | Validar identidades usando modelo offline facial | Must |
| RF15 | Tiempos Límite (Rendimiento) | Cerrar marcajes biométricos en menos de 30 segundos (Sin lag) | Must |
| RF19 | Criptografía | Uso completo en tránsito de métodos blindados biométricos y AES  | Must |
| RF23 | Segregación de Control | Crear el Rol Administrador para controlar los reportes libremente | Must |
| RF36 | No trabajo Remoto / Web | Sistema completamente cerrado a la red ofimática corporativa | Must |
| RF37 | Tracking de Ubicaciones / Mapas | Implementar GPS y rastreo web móvil para el empleado | Won’t |
| RF39 | Gestor de Finanzas / Pagos | Manipulación de cálculos financieros, planillas o sueldos | Won’t |

---

### Conclusión de Diseño
Gracias a que colocamos en calidad de **Wen't** (No se construirá bajo ninguna circunstancia el alcance de Módulos Financieros/Sueldo ni de Localización Geográfica Externa al Área Inmobiliaria), toda la concentración de código C# WPF y Python logró alcanzar su MVP final libre de "Spaguetti Code" operando masivamente el rubro de RRHH en el registro ágil.
