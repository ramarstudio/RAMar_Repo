# ADR — Por que WPF y no Web

!!! abstract "Resumen de la decision"
    Se descarto la arquitectura web (React) a favor de una aplicacion nativa de escritorio con **WPF (.NET 8)** para obtener acceso directo al hardware y eliminar la latencia de red.

---

## Contexto

El planteamiento original era construir el kiosco de asistencia como una **aplicacion web** con React renderizada en el navegador del equipo.

## Problema identificado

Durante la construccion de prototipos, se descubrieron problemas criticos con el enfoque web:

### 1. Barrera del hardware

Una pagina web no puede controlar directamente la GPU y la camara. Solo accede al hardware mediante APIs del navegador que introducen capas de abstraccion innecesarias y consumen mas recursos.

### 2. Latencia de red HTTP

Para que React envie un frame de video a Python, debe:

1. Capturar el frame via la API del navegador
2. Codificarlo como Base64 o Blob
3. Enviarlo por HTTP al backend
4. Esperar la respuesta

Este ciclo de ida y vuelta, incluso dentro de la red local, produce latencia perceptible e inconsistente.

### 3. Consumo de recursos

El navegador Chrome consume una cantidad significativa de RAM y CPU solo para renderizar la interfaz, compitiendo por recursos con el motor de reconocimiento facial.

---

## Decision

Migrar toda la solucion grafica a una **aplicacion nativa de escritorio Windows** escrita en C# (.NET 8 WPF).

## Consecuencias positivas

| Aspecto | Web (React) | Nativo (WPF) |
|---|---|---|
| Acceso a camara | API del navegador (indirecto) | DirectShow / punteros en memoria (directo) |
| FPS de video | ~15 FPS con parpadeos | > 30 FPS fluidos |
| CPU en reposo | ~8-12% (navegador) | ~1% |
| Latencia biometrica | Variable (HTTP + encoding) | < 1s (memoria local a HTTP localhost) |
| RAM base | ~300 MB (Chrome) | ~80 MB |

## Consecuencias negativas

- Limitado a **Windows** (no es multiplataforma)
- Requiere instalacion en cada equipo (no basta un navegador)
- El equipo de desarrollo necesita conocer C#/WPF

Estas limitaciones son aceptables porque el sistema opera exclusivamente en computadoras de oficina con Windows, que es el caso de uso del cliente.
