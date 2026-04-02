# Motor biométrico

El motor de reconocimiento facial es un **microservicio Python** que corre localmente en la misma máquina que la aplicación WPF.

---

## Tecnologías

| Componente | Tecnología | Función |
|---|---|---|
| Framework web | **FastAPI** | API REST asíncrona |
| Motor facial | **InsightFace** (modelo `buffalo_l`) | Detección y embedding |
| Modelo | **ArcFace** | Vectores de 512 dimensiones |
| Runtime ML | **ONNX Runtime** | Ejecución eficiente de modelos |

## Cómo funciona el reconocimiento

```mermaid
graph LR
    A[Frame de video] --> B[Detección de rostro]
    B --> C[Alineación facial]
    C --> D[Extracción de embedding]
    D --> E[Vector 512-d]
    E --> F{Comparación coseno}
    F -->|Similitud > umbral| G[Identidad confirmada]
    F -->|Similitud < umbral| H[No reconocido]
```

### Paso a paso

1. **Detección**: localiza el rostro dentro del frame usando RetinaFace
2. **Alineación**: normaliza la posición, rotación y escala del rostro
3. **Embedding**: genera un vector de 512 números que representan las características únicas del rostro
4. **Comparación**: calcula la similitud coseno entre el vector generado y los almacenados

---

## Seguridad de los datos biométricos

- Los embeddings se almacenan **cifrados con AES-256**
- Se descifran **solo en memoria** durante la comparación
- Un embedding es un vector matemático: **no puede reconstruir un rostro**
- Si la base de datos es comprometida, los vectores cifrados son inútiles

---

## Gestión de recursos

El motor no está siempre activo. El `FaceServiceManager` en la aplicación WPF controla su ciclo de vida:

- **Inicio bajo demanda**: se activa cuando se necesita verificación biométrica
- **Apagado por inactividad**: se detiene automáticamente tras un periodo sin uso
- **Consumo típico**: ~300-500 MB de RAM cuando está activo, 0 MB cuando está dormido

---

## Endpoints de la API

| Método | Ruta | Función |
|---|---|---|
| `GET` | `/health` | Verificar que el servicio está activo |
| `POST` | `/encode` | Generar embedding a partir de una imagen |
| `POST` | `/verify` | Comparar un frame contra embeddings almacenados |
