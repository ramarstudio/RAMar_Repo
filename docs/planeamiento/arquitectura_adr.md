# 🏗️ Sustentación de Decisiones: La Victoria de WPF C# frente la "Web Tradicional"

Este es el Análisis de Registro de Decisión Arquitéctónica (ADR) real donde nuestros ingenieros descartaron construir este sistema en una Página Web, decidiendo apostar 100% por una solución Nativa en Desktop.

---

## 1. El Planteamiento Original
Originalmente, el cliente y nuestro departamento pensó que la forma más estándar de crear el kiosco de los empleados era construyendo un Front-End Web React (o similar) renderizándolo desde el navegador `Chrome` o `Borde`.

## 2. El Terible Defecto Encontrado
Al construir los prototipos, se descubrieron gravísimos problemas con la arquitectura del entorno `WEB`:
1. **La barrera del Hardware:** Una página web no puede controlar la GPU y RAM de la cámara físicamente; solo la pide "prestada" mediante APIs del navegador muy pesadas.
2. **Latencia por Protocolos de Red HTTP:** Para que la Web de React analice una cara en Python, tiene que codificarla en imágenes base64 o subir "Pedazos" de video al Backend. Este rebote de ida y vuelta a lo largo del servidor corporativo producía grandes interrupciones.

## 3. La Decisión Brutal
Al pivoteaar toda la solución gráfica hacia una **Aplicación Nativa Interactiva Instalable para Windows** escrita en `C# (.NET 8 WPF)`, todo el problema evaporó.

* **La Ventaja de Windows Presentation Foundation (WPF):** 
C# permitió invocar las librerías Core puras de la Webcam (DirectShow y Punteros Libres No-Administrados). Así, la cámara empezó a emitir a más de 30 FPS sin parpadeos y pasaba las variables directas de la memoria a Python de forma local.

**Resultado Final:** Cero Overhead Web, UI Ultra-Fluído en Monitores Táctiles y RAM Liberada brutalmente.
