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
- **Persistencia local:** archivos JSON por sede, recuperados al iniciar para reconstruir el árbol, el grafo y las colas, y guardados tras cada cambio.

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
- **Frontend:** HTML, CSS y JavaScript (módulos ES, sin librerías), servidos por la propia API de cada sede
- **Mensajería:** RabbitMQ
- **Persistencia:** archivos JSON locales
- **Pruebas:** xUnit
- **Control de versiones:** Git

---

## Arquitectura

Hay dos canales de comunicación distintos:

```
   Navegador (dependiente)
          │
          │  REST (HTTP): el usuario habla con SU sede
          ▼
   ┌─────────────────┐                      ┌─────────────────┐
   │ Backend Liberia │                      │ Backend San José│
   └────────┬────────┘                      └────────┬────────┘
            │   Mensajes: las sedes hablan entre sí  │
            ▼                                        ▼
        ┌────────────────────────────────────────────────┐
        │                   RabbitMQ                     │
        └────────────────────────────────────────────────┘
```

- **REST** conecta el navegador con **su propia sede**. La interfaz solo pide datos a la API y los muestra.
- **RabbitMQ** conecta **sede con sede**. Es la única vía para pedir o modificar datos de otra sede.
- Las 4 sedes son **el mismo programa** ejecutado con distinta configuración (`config/sede-*.json`).
- Cada sede sirve su frontend y su API en el mismo puerto, por lo que no hay problemas de CORS.

### Estado actual

| Parte | Estado |
|---|---|
| Proyecto base, configuración de sedes y servidor | Listo |
| Interfaz web (7 consultas y 3 operaciones) | Lista |
| API REST | Con **datos simulados** (`ConviTEC.Api/Simulacion`); se reemplazan al integrar cada módulo real |
| Persistencia (guardar y cargar por sede) | Lista; los módulos se registran al integrarse |
| Estructuras, inventario, pedidos, mensajería y rutas | En desarrollo por cada rol |

La documentación de los endpoints está en [`docs/api-endpoints.md`](docs/api-endpoints.md) y los contratos entre módulos en [`docs/contratos.md`](docs/contratos.md).

---

## Estructura del proyecto

```
ConviTEC/
├── .gitignore
├── README.md
├── ConviTEC.sln
├── config/                          Configuración de cada sede
│   ├── sede-liberia.json            código, nombre, puerto, carpeta de datos, RabbitMQ
│   ├── sede-alajuela.json
│   ├── sede-sanjose.json
│   └── sede-limon.json
├── data/
│   ├── rutas.json                   Rutas del grafo de distribución
│   └── sedes/                       Archivos locales de cada sede (no se suben a Git)
│       ├── liberia/
│       ├── alajuela/
│       ├── sanjose/
│       └── limon/
├── docs/
│   ├── api-endpoints.md             Endpoints REST de cada sede
│   ├── contratos.md                 Contratos entre módulos
│   ├── protocolo-mensajes.md        Mensajes entre sedes
│   ├── diagrama-uml.png             Diagrama UML de clases
│   └── documento-diseno.md          Documento de diseño (DI1 a DI4)
├── frontend/                        Interfaz web (sin lógica de negocio)
│   ├── index.html
│   ├── css/
│   │   └── estilos.css
│   └── js/
│       ├── app.js                   Menú, navegación por hash y carga de vistas
│       ├── api.js                   Cliente de la API (fetch)
│       ├── ui.js                    Utilidades para construir la interfaz
│       └── vistas.js                Las 7 consultas y 3 operaciones
├── src/
│   ├── ConviTEC.Core/               Biblioteca de clases (lógica de negocio)
│   │   ├── Configuracion/           ConfigSede
│   │   ├── Estructuras/             Lista, Cola, Pila, ArbolAVL, Ordenamiento
│   │   ├── Modelo/                  Producto, Sede, Venta
│   │   ├── Inventario/              Inventario, Estadisticas
│   │   ├── Pedidos/                 Pedido, DetallePedido, EstadoPedido, GestorPedidos
│   │   ├── Mensajeria/              Mensaje, MensajeriaRabbitMq
│   │   ├── Rutas/                   Grafo, GestorRutas
│   │   ├── Persistencia/            GestorArchivos, ServicioPersistencia
│   │   └── Contratos/               IInventario, IGestorPedidos, IGestorRutas,
│   │                                IMensajeria, IPersistible, IValidable
│   └── ConviTEC.Api/                API REST de cada sede
│       ├── Program.cs               Arranque de la sede, archivos estáticos y persistencia
│       ├── Endpoints/               Endpoints REST
│       └── Simulacion/              Datos simulados (temporal)
└── tests/
    └── ConviTEC.Tests/              Pruebas xUnit de cada módulo
        ├── EstructurasTests.cs
        ├── PedidosTests.cs
        ├── RutasTests.cs
        ├── PersistenciaTests.cs
        ├── ServicioPersistenciaTests.cs
        ├── ConfigSedeTests.cs
        ├── ApiContratoTests.cs
        ├── ApiLiberiaTests.cs
        ├── ApiDatosSimuladosTests.cs    (temporal: se borra al integrar los módulos)
        └── ApiFactory.cs
```

### Responsables por carpeta

| Carpeta | Responsable |
|---|---|
| `Estructuras/`, `Modelo/`, `Inventario/`, `docs/diagrama-uml.png` | Ulfran |
| `Pedidos/`, `Mensajeria/`, `docs/protocolo-mensajes.md` | Christian |
| `Rutas/`, `data/rutas.json` | Isaac |
| `frontend/`, `Persistencia/`, `Configuracion/`, `ConviTEC.Api/`, `config/`, `docs/api-endpoints.md`, `README.md` | Ignacio |
| `Contratos/`, `docs/contratos.md`, `docs/documento-diseno.md`, `tests/` | Todos |

---

## Configuración, compilación y ejecución

### Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Git
- RabbitMQ (necesario cuando se integre la mensajería entre sedes)

Los comandos de esta sección se ejecutan desde la **raíz del repositorio** y funcionan igual en Windows, macOS y Linux.

### Configuración de una sede

Cada sede se configura con su archivo `config/sede-<nombre>.json`:

```json
{
  "codigo": "LIB",
  "nombre": "Liberia",
  "puerto": 5001,
  "carpetaDatos": "data/sedes/liberia",
  "rabbitMq": {
    "host": "localhost",
    "puerto": 5672,
    "usuario": "guest",
    "contrasena": "guest"
  }
}
```

| Campo | Descripción |
|---|---|
| `codigo` | Código único de la sede |
| `nombre` | Nombre de la sede |
| `puerto` | Puerto en el que escucha la API y el frontend (1 a 65535) |
| `carpetaDatos` | Carpeta de los archivos locales de la sede, relativa a la raíz del repositorio |
| `rabbitMq` | Datos de conexión al servidor de RabbitMQ. Si se omite, usa `localhost:5672` con `guest`/`guest` |

Puertos por defecto:

| Sede | Archivo | Puerto |
|---|---|---|
| Liberia | `config/sede-liberia.json` | 5001 |
| Alajuela | `config/sede-alajuela.json` | 5002 |
| San José | `config/sede-sanjose.json` | 5003 |
| Limón | `config/sede-limon.json` | 5004 |

Si un campo falta o es inválido, la sede no arranca y muestra el error de configuración.

### Compilación

```bash
dotnet build
```

### Pruebas

```bash
dotnet test
```

### Ejecución de una sede

Cada sede se levanta indicando su nombre (el de su archivo de configuración):

```bash
dotnet run --project src/ConviTEC.Api -- --sede liberia
```

Alternativamente, con la variable de entorno `CONVITEC_SEDE`:

```bash
# Windows (cmd)
set CONVITEC_SEDE=liberia
dotnet run --project src/ConviTEC.Api

# macOS / Linux
CONVITEC_SEDE=liberia dotnet run --project src/ConviTEC.Api
```

Al iniciar, la sede muestra su nombre, su puerto y su carpeta de datos, carga sus archivos locales y deja disponible la interfaz en `http://localhost:<puerto>`. Por ejemplo, Liberia en `http://localhost:5001`.

Para detener la sede se usa `Ctrl+C`; antes de cerrar guarda todos sus datos.

### Ejecución de las 4 sedes en una computadora

Se abre una terminal por sede y se ejecuta, en cada una, el comando con su nombre:

```bash
dotnet run --project src/ConviTEC.Api -- --sede liberia
dotnet run --project src/ConviTEC.Api -- --sede alajuela
dotnet run --project src/ConviTEC.Api -- --sede sanjose
dotnet run --project src/ConviTEC.Api -- --sede limon
```

### Ejecución de las 4 sedes en 2 computadoras

> Pendiente de confirmar cuando se integre la mensajería (RabbitMQ).

Plan general:

1. Instalar RabbitMQ en una de las computadoras y dejarla accesible en la red.
2. Crear en RabbitMQ un usuario distinto de `guest`, porque `guest` solo acepta conexiones desde la misma máquina.
3. En las sedes de **ambas** computadoras, poner en `rabbitMq` de su configuración la IP de la máquina que aloja RabbitMQ, junto con el usuario y la contraseña creados.
4. Repartir las sedes entre las dos computadoras (por ejemplo, Liberia y Alajuela en una, San José y Limón en la otra) y levantar cada una con su comando.

---

## Datos locales de cada sede

Cada sede guarda sus datos en la carpeta indicada por `carpetaDatos` (por ejemplo `data/sedes/liberia/`), un archivo JSON por módulo.

- Al iniciar, la sede carga sus archivos y reconstruye sus estructuras, validando la consistencia. Si un archivo está corrupto, la sede **no arranca** y **no lo sobrescribe**.
- Tras cada cambio, el módulo correspondiente guarda su archivo. La escritura es segura: se escribe un archivo temporal y luego se reemplaza.
- Si es el primer arranque y no hay archivos, la sede inicia vacía.
- Estos archivos **no se suben a Git** (`data/sedes/*/*.json` está en `.gitignore`).

---

## Flujo de trabajo con Git

- Ramas principales: `main` (versión estable) y `develop` (avances).
- Cada tarea se desarrolla en su propia rama y se integra por Pull Request a `develop`.
- Cada Pull Request debe ser revisado por otra persona, compilar y pasar sus pruebas.
- Mensajes de commit con el formato `tipo: descripción corta`, con tipos `feat`, `fix`, `test`, `docs` y `refactor`.
  Ejemplo: `fix: liberación de reservas al cancelar pedido`.
- No se suben binarios ni datos locales de las sedes.