# ConviTEC

**Sistema distribuido de inventario y planificación de entregas**

Proyecto 2 del curso **CE1103 Algoritmos y Estructuras de Datos 1**
Escuela de Ingeniería en Computadores, Instituto Tecnológico de Costa Rica.

---

## Descripción

ConviTEC simula una cadena de tiendas de conveniencia con cuatro sedes en distintas provincias de Costa Rica: **Liberia, Alajuela, San José y Limón**. Cada sede es una instancia independiente de la aplicación, con su propio frontend web, su backend y sus archivos locales (productos, inventario y pedidos).

Las sedes se comunican **únicamente por paso de mensajes** (RabbitMQ). Ninguna sede accede a las estructuras de datos o archivos de otra.

Funcionalidades principales:

- **Catálogo e inventario:** cada sede administra su inventario local y mantiene una copia sincronizada del catálogo global.
- **Pedidos distribuidos:** un pedido puede incluir productos de otras sedes. Se reserva el inventario en la sede dueña, se valida la ruta y, si algo falla, se cancela el pedido completo y se liberan las reservas.
- **Rutas de entrega:** la red de distribución es un grafo no dirigido y ponderado (matriz de adyacencia) cargado desde un JSON. Se usa Dijkstra, ejecutado de nuevo tras cada entrega, para elegir el destino pendiente más cercano. Cada sede sale dos veces al día (11:00 a. m. y 3:00 p. m.).
- **Guanacaste aislada:** la sede de Liberia puede abastecerse del inventario global, pero solo entrega dentro de su componente del grafo. Los destinos inalcanzables se rechazan con una explicación.
- **Estadísticas:** historial de ventas, producto estrella (cantidad × precio unitario) y peores pedidos (monto ÷ tiempo de traslado).
- **Persistencia local:** archivos JSON por sede, recuperados al iniciar para reconstruir el árbol, el grafo y las colas.

### Restricciones del enunciado

- Sin colecciones nativas (`List`, `Dictionary`, `Queue`, `Stack`, etc.); solo arreglos nativos como base.
- Sin `Sort()`, `OrderBy()` ni equivalentes; búsqueda, recorrido y ordenamiento propios.
- Sin gestor de base de datos.
- La interfaz no contiene lógica de negocio.

---

## Integrantes

| Integrante | Rol | Alcance |
|---|---|---|
| Ulfran | Backend: inventario y estructuras | Estructuras de datos propias, árbol AVL del catálogo, inventario, ventas, estadísticas, UML |
| Christian | Backend: pedidos y mensajería | Pedidos, reservas, cancelaciones, RabbitMQ, protocolo entre sedes |
| Isaac | Backend: grafos y distribución | Matriz de adyacencia, JSON de rutas, Dijkstra, recorrido de entregas |
| Ignacio | Frontend y persistencia | Interfaz web, API REST, archivos locales, README |

> Roles pendientes de confirmar.

---

## Tecnologías

- **Lenguaje:** C# (.NET 10)
- **Backend / API:** ASP.NET Core
- **Frontend:** HTML, CSS y JavaScript, servidos por la propia API (ASP.NET Core) de cada sede
- **Mensajería:** RabbitMQ
- **Persistencia:** archivos JSON locales
- **Pruebas:** xUnit
- **Control de versiones:** Git

---

## Estructura del proyecto

```
ConviTEC/
├── .gitignore
├── README.md
├── ConviTEC.sln
├── config/                      Configuración de cada sede
│   ├── sede-liberia.json        nombre, puerto, carpeta de datos, RabbitMQ
│   ├── sede-alajuela.json
│   ├── sede-sanjose.json
│   └── sede-limon.json
├── data/
│   ├── rutas.json               Rutas del grafo de distribución
│   └── sedes/                   Archivos locales de cada sede (no se suben a Git)
│       ├── liberia/
│       ├── alajuela/
│       ├── sanjose/
│       └── limon/
├── docs/
│   ├── contratos.md             Contratos entre módulos
│   ├── protocolo-mensajes.md    Mensajes entre sedes
│   ├── diagrama-uml.png         Diagrama UML de clases
│   └── documento-diseno.md      Documento de diseño (DI1 a DI4)
├── frontend/                    Interfaz web
├── src/
│   ├── ConviTEC.Core/           Biblioteca de clases (lógica de negocio)
│   │   ├── Estructuras/         Lista, Cola, Pila, ArbolAVL, Ordenamiento
│   │   ├── Modelo/              Producto, Sede, Venta
│   │   ├── Inventario/          Inventario, Estadisticas
│   │   ├── Pedidos/             Pedido, DetallePedido, EstadoPedido, GestorPedidos
│   │   ├── Mensajeria/          Mensaje, MensajeriaRabbitMq
│   │   ├── Rutas/               Grafo, GestorRutas
│   │   ├── Persistencia/        GestorArchivos
│   │   └── Contratos/           IInventario, IGestorPedidos, IGestorRutas,
│   │                            IMensajeria, IPersistible
│   └── ConviTEC.Api/            API REST de cada sede
│       └── Program.cs
└── tests/
    └── ConviTEC.Tests/          Pruebas xUnit de cada módulo
        ├── EstructurasTests.cs
        ├── PedidosTests.cs
        ├── RutasTests.cs
        └── PersistenciaTests.cs
```

### Responsables por carpeta

| Carpeta | Responsable |
|---|---|
| `Estructuras/`, `Modelo/`, `Inventario/`, `docs/diagrama-uml.png` | Ulfran |
| `Pedidos/`, `Mensajeria/`, `docs/protocolo-mensajes.md` | Christian |
| `Rutas/`, `data/rutas.json` | Isaac |
| `frontend/`, `Persistencia/`, `ConviTEC.Api/`, `config/`, `README.md` | Ignacio |
| `Contratos/`, `docs/contratos.md`, `docs/documento-diseno.md`, `tests/` | Todos |

---

## Configuración, compilación y ejecución

> Pendiente: se completará cuando el proyecto base compile y las sedes puedan ejecutarse.

### Requisitos

- .NET 10 SDK
- RabbitMQ
- Git

### Configuración de una sede

_Pendiente._

### Compilación

_Pendiente._

### Ejecución de las 4 sedes en 2 computadoras

_Pendiente._

---

## Flujo de trabajo con Git

- Ramas principales: `main` (versión estable) y `develop` (avances).
- Cada tarea se desarrolla en su propia rama y se integra por Pull Request a `develop`.
- Cada Pull Request debe ser revisado por otra persona, compilar y pasar sus pruebas.
- Mensajes de commit con el formato `tipo: descripción corta`, con tipos `feat`, `fix`, `test`, `docs` y `refactor`.
  Ejemplo: `fix: liberación de reservas al cancelar pedido`.
- No se suben binarios ni datos locales de las sedes (`data/sedes/*/*.json` está en `.gitignore`).