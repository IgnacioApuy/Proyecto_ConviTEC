# API REST de cada sede

Cada sede publica la misma API en su propio puerto (`config/sede-*.json`).
Todas las respuestas son JSON. Los datos actuales son **simulados**.

## Consultas

| Endpoint | Descripción |
|---|---|
| `GET /api/sede` | Código, nombre y puerto de la sede |
| `GET /api/ventas` | Historial de ventas: productos, cantidades, precios, fecha y monto |
| `GET /api/pedidos` | Historial de pedidos, en cualquier estado |
| `GET /api/pedidos/pendientes` | Pedidos confirmados que aún no se entregan |
| `GET /api/inventario` | Inventario local: código, nombre, categoría, precio y cantidad |
| `GET /api/catalogo` | Catálogo global con la disponibilidad en cada sede |
| `GET /api/estadisticas/producto-estrella` | Producto con más ingresos (unidades x precio) |
| `GET /api/estadisticas/peores-pedidos` | Pedidos entregados, de menor a mayor rendimiento (monto / minutos) |

## Operaciones

### `POST /api/pedidos`: realizar un pedido

Cuerpo:

```json
{
  "destino": "Heredia",
  "items": [
    { "codigo": "P001", "cantidad": 2 },
    { "codigo": "P003", "cantidad": 1 }
  ]
}
```

Respuestas:

| Código | Cuándo | Cuerpo |
|---|---|---|
| `200` | Aceptado | `{ "aceptado": true, "idPedido": "PED-007", "mensaje": "...", "montoTotal": 5100 }` |
| `200` | Rechazado por el negocio (sin inventario, destino inalcanzable o inexistente) | `{ "aceptado": false, "motivo": "..." }` |
| `400` | Datos inválidos (sin destino, sin productos, código inexistente, cantidad <= 0, producto repetido) | `{ "error": "..." }` |

### `GET /api/entregas/pendientes`: consultar entregas pendientes

Igual que `GET /api/pedidos/pendientes`.

### `POST /api/entregas/siguiente`: calcular el siguiente punto de entrega

Sin cuerpo. Respuesta:

```json
{
  "hayPendientes": true,
  "ubicacionActual": "Alajuela",
  "destino": "Heredia",
  "tiempoMinutos": 20,
  "ruta": ["Alajuela", "Heredia"],
  "pedidos": ["PED-004"]
}
```

Si no hay entregas pendientes: `{ "hayPendientes": false, "mensaje": "..." }`.

## Notas

- Estados de un pedido: `Pendiente`, `Confirmado`, `Entregado`, `Cancelado`. Un pedido *pendiente de entrega* está `Confirmado`.
- Cada sede devuelve únicamente sus propios registros, salvo `/api/catalogo`.