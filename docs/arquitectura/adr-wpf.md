# WPF vs Web — Decisión de arquitectura

!!! abstract "Resumen"
    Se descartó la arquitectura web (React) a favor de una aplicación nativa de escritorio con **WPF (.NET 8)** para obtener acceso directo al hardware y eliminar la latencia de red.

---

## Contexto

El planteamiento original era construir el kiosco de asistencia como una **aplicación web** con React renderizada en el navegador del equipo.

## Problema identificado

Durante la construcción de prototipos se descubrieron problemas críticos con el enfoque web:

### 1. Barrera del hardware

Una página web no puede controlar directamente la GPU y la cámara. Solo accede al hardware mediante APIs del navegador que introducen capas de abstracción innecesarias.

### 2. Latencia de red HTTP

Para que React envíe un frame de video a Python, debe:

1. Capturar el frame vía la API del navegador
2. Codificarlo como Base64 o Blob
3. Enviarlo por HTTP al backend
4. Esperar la respuesta

Este ciclo produce latencia perceptible e inconsistente, incluso dentro de la red local.

### 3. Consumo de recursos

El navegador Chrome consume RAM y CPU significativos solo para renderizar la interfaz, compitiendo con el motor de reconocimiento facial.

---

## Decisión

Migrar toda la solución gráfica a una **aplicación nativa de escritorio Windows** escrita en C# (.NET 8 WPF).

## Comparativa

| Aspecto | Web (React) | Nativo (WPF) |
|---|---|---|
| Acceso a cámara | API del navegador (indirecto) | DirectShow / punteros en memoria |
| FPS de video | ~15 FPS con parpadeos | > 30 FPS fluidos |
| CPU en reposo | ~8-12% (navegador) | ~1% |
| Latencia biométrica | Variable | < 1 segundo |
| RAM base | ~300 MB (Chrome) | ~80 MB |

## Consecuencias negativas aceptadas

- Limitado a **Windows** (no es multiplataforma)
- Requiere instalación en cada equipo
- El equipo necesita conocer C#/WPF

Estas limitaciones son aceptables porque el sistema opera exclusivamente en computadoras de oficina con Windows.
