# Contratos entre módulos

Los contratos se acuerdan al inicio y se actualizan en el mismo PR que los cambie.
Los de este documento son una **propuesta**: cada responsable debe revisarla y comentar en el PR.

> Convención: los contratos usan arreglos nativos (`T[]`) para no depender de las
> estructuras propias. Los valores "no encontrado" se devuelven como `null` (objetos)
> o `-1` (números), nunca con excepciones.

## Resumen

| Quién lo usa | Qué necesita | Quién lo entrega |
|---|---|---|
| Pedidos | `BuscarProducto`, `Disponibilidad`, `Reservar`, `Liberar`, `Confirmar` | Inventario y estructuras (Ulfran) |
| Pedidos | `EsAlcanzable`, `TiempoMinimo` | Grafos y distribución (Isaac) |
| Estadísticas | Pedido entregado con monto y tiempo de traslado | Pedidos (Christian) y Grafos (Isaac) |
| Frontend | Endpoints de cada consulta y operación | Cada módulo backend |
| Persistencia | `NombreArchivo`, `Serializar()`, `Reconstruir()` | Cada módulo |
| Todos | Lista, Cola, ArbolAVL, ordenamiento y búsqueda | Inventario y estructuras (Ulfran) |

## IPersistible (acordado, implementado)

```csharp
string NombreArchivo { get; }
string Serializar();
void Reconstruir(string json);
```

## IInventario (propuesta)

```csharp
Producto? BuscarProducto(string codigo);                          // null si no existe
int Disponibilidad(string codigo);                                // 0 si no existe
bool Reservar(string idPedido, string codigo, int cantidad);      // atómico: verifica y reserva
void Liberar(string idPedido);                                    // libera todas las reservas del pedido
void Confirmar(string idPedido);                                  // descuenta las reservas del pedido
```

## IGestorRutas (propuesta)

```csharp
bool EsAlcanzable(string origen, string destino);
int TiempoMinimo(string origen, string destino);                  // minutos; -1 si es inalcanzable
```

## IMensajeria (propuesta)

```csharp
void Enviar(string sedeDestino, Mensaje mensaje);
void SuscribirRecepcion(Action<Mensaje> alRecibir);
```

## IGestorPedidos (propuesta)

```csharp
ResultadoPedido RegistrarPedido(string destino, DetallePedido[] detalles);   // aceptado o rechazado con motivo
Pedido[] ObtenerPendientes();
Pedido[] ObtenerHistorial();
```

## Pendiente de definir

- Campos de `Mensaje` y `ResultadoPedido` (Christian).
- Formato JSON de cada módulo (cada responsable documenta el suyo).
- Endpoints REST (Ignacio, cuando existan los módulos).

## Persistencia en cada módulo (acordado)

Cada módulo que deba sobrevivir al reinicio implementa `IPersistible` y se registra en
`Program.cs` con `persistencia.Registrar(modulo)`.

- **Orden de registro = orden de carga.** Registrar primero los módulos de los que dependen otros
  (catálogo, luego inventario, luego pedidos y ventas).
- **Guardar tras cada cambio:** el módulo (o su gestor) recibe el `ServicioPersistencia` y llama
  `persistencia.Guardar(this)` después de cada operación que modifique sus datos.
  Un módulo no registrado lanza `InvalidOperationException` al guardar.
- **`Reconstruir(json)`** valida el contenido de su propio archivo y lanza una excepción con
  mensaje claro si es inválido. La sede no arranca con un archivo corrupto y nunca lo sobrescribe.
- **`IValidable` (opcional):** si el módulo necesita revisar referencias a otros módulos
  (ej. un pedido que apunta a un producto), implementa `Validar()`. Se ejecuta cuando todos
  los módulos ya fueron cargados.
- Cada módulo debe tener un `NombreArchivo` distinto (ej. `inventario.json`, `pedidos.json`).