using ConviTEC.Api.Simulacion;
using ConviTEC.Core.Configuracion;

namespace ConviTEC.Api.Endpoints;

public record ItemSolicitud(string? Codigo, int Cantidad);
public record SolicitudPedido(string? Destino, ItemSolicitud[]? Items);

/// <summary>
/// Endpoints REST de cada sede con datos simulados.
/// Al integrar cada módulo real, se reemplaza el origen de los datos de ese endpoint.
/// </summary>
public static class EndpointsSimulados
{
    private static int _contadorPedidos = 6;

    public static void MapEndpointsSimulados(this WebApplication app, ConfigSede config)
    {
        int sede = DatosSimulados.IndiceDeSede(config.Nombre);

        // ---- Consultas ----
        app.MapGet("/api/ventas", () => Results.Ok(ConstruirVentas(config.Nombre)));
        app.MapGet("/api/pedidos", () => Results.Ok(DatosSimulados.Pedidos));
        app.MapGet("/api/pedidos/pendientes", () => Results.Ok(DatosSimulados.PorEstado("Confirmado")));
        app.MapGet("/api/inventario", () => Results.Ok(ConstruirInventario(sede)));
        app.MapGet("/api/catalogo", () => Results.Ok(ConstruirCatalogo()));
        app.MapGet("/api/estadisticas/producto-estrella", () => ProductoEstrella());
        app.MapGet("/api/estadisticas/peores-pedidos", () => Results.Ok(PeoresPedidos()));

        // ---- Operaciones ----
        app.MapPost("/api/pedidos", (SolicitudPedido? solicitud) => RealizarPedido(config, solicitud));
        app.MapGet("/api/entregas/pendientes", () => Results.Ok(DatosSimulados.PorEstado("Confirmado")));
        app.MapPost("/api/entregas/siguiente", () => SiguienteEntrega(config));
    }

    private static object[] ConstruirVentas(string nombreSede)
    {
        PedidoSim[] entregados = DatosSimulados.PorEstado("Entregado");
        var resultado = new object[entregados.Length];

        for (int i = 0; i < entregados.Length; i++)
        {
            PedidoSim pedido = entregados[i];
            resultado[i] = new
            {
                IdVenta = "V" + pedido.Id.Substring(4),
                IdPedido = pedido.Id,
                Fecha = pedido.FechaEntrega,
                Sede = nombreSede,
                pedido.Detalles,
                pedido.MontoTotal
            };
        }

        return resultado;
    }

    private static object[] ConstruirInventario(int sede)
    {
        if (sede < 0) return new object[0];

        var resultado = new object[DatosSimulados.Productos.Length];
        for (int i = 0; i < resultado.Length; i++)
        {
            ProductoSim p = DatosSimulados.Productos[i];
            resultado[i] = new
            {
                p.Codigo,
                p.Nombre,
                p.Categoria,
                p.Precio,
                Cantidad = DatosSimulados.Existencias[i][sede]
            };
        }

        return resultado;
    }

    private static object[] ConstruirCatalogo()
    {
        var resultado = new object[DatosSimulados.Productos.Length];
        for (int i = 0; i < resultado.Length; i++)
        {
            ProductoSim p = DatosSimulados.Productos[i];

            var disponibilidad = new object[DatosSimulados.Sedes.Length];
            for (int s = 0; s < disponibilidad.Length; s++)
                disponibilidad[s] = new { Sede = DatosSimulados.Sedes[s], Cantidad = DatosSimulados.Existencias[i][s] };

            resultado[i] = new { p.Codigo, p.Nombre, p.Descripcion, p.Categoria, p.Precio, Disponibilidad = disponibilidad };
        }

        return resultado;
    }

    // Ingreso = unidades vendidas x precio unitario, sobre los pedidos entregados.
    private static IResult ProductoEstrella()
    {
        PedidoSim[] entregados = DatosSimulados.PorEstado("Entregado");
        int[] unidades = new int[DatosSimulados.Productos.Length];

        foreach (PedidoSim pedido in entregados)
        {
            foreach (Detalle detalle in pedido.Detalles)
                unidades[DatosSimulados.IndiceProducto(detalle.Codigo)] += detalle.Cantidad;
        }

        int mejor = -1;
        int mejorIngreso = 0;
        for (int i = 0; i < unidades.Length; i++)
        {
            int ingreso = unidades[i] * DatosSimulados.Productos[i].Precio;
            if (ingreso > mejorIngreso)
            {
                mejorIngreso = ingreso;
                mejor = i;
            }
        }

        if (mejor < 0)
            return Results.Ok(new { hayVentas = false, mensaje = "Aún no hay ventas registradas." });

        ProductoSim producto = DatosSimulados.Productos[mejor];
        return Results.Ok(new
        {
            hayVentas = true,
            producto.Codigo,
            producto.Nombre,
            UnidadesVendidas = unidades[mejor],
            producto.Precio,
            Ingresos = mejorIngreso
        });
    }

    // Peor rendimiento primero. Ordenamiento temporal: se reemplaza por el de Ulfran.
    private static PedidoSim[] PeoresPedidos()
    {
        PedidoSim[] resultado = DatosSimulados.PorEstado("Entregado");

        for (int i = 1; i < resultado.Length; i++)
        {
            PedidoSim actual = resultado[i];
            int j = i - 1;
            while (j >= 0 && (resultado[j].Rendimiento ?? 0) > (actual.Rendimiento ?? 0))
            {
                resultado[j + 1] = resultado[j];
                j--;
            }
            resultado[j + 1] = actual;
        }

        return resultado;
    }

    private static IResult RealizarPedido(ConfigSede config, SolicitudPedido? solicitud)
    {
        string? error = Validar(solicitud);
        if (error != null)
            return Results.BadRequest(new { error });

        ItemSolicitud[] items = solicitud!.Items!;
        int monto = 0;

        // 1. Disponibilidad total entre todas las sedes.
        for (int i = 0; i < items.Length; i++)
        {
            int indice = DatosSimulados.IndiceProducto(items[i].Codigo!);
            int total = 0;
            for (int s = 0; s < DatosSimulados.Sedes.Length; s++)
                total += DatosSimulados.Existencias[indice][s];

            ProductoSim producto = DatosSimulados.Productos[indice];
            if (items[i].Cantidad > total)
                return Rechazo($"Inventario insuficiente de '{producto.Nombre}': se piden {items[i].Cantidad} y hay {total} en total.");

            monto += items[i].Cantidad * producto.Precio;
        }

        // 2. Ruta hacia el destino. Guanacaste es un componente aislado del grafo.
        string? destino = BuscarDestino(solicitud.Destino!.Trim());
        if (destino == null)
            return Rechazo($"El destino '{solicitud.Destino.Trim()}' no existe en la red de distribución.");

        if (EsGuanacaste(config.Nombre) != EsGuanacaste(destino))
            return Rechazo($"No existe ruta desde {config.Nombre} hasta {destino}: Guanacaste es un componente aislado.");

        int numero = Interlocked.Increment(ref _contadorPedidos);
        string idPedido = $"PED-{numero:000}";

        return Results.Ok(new
        {
            aceptado = true,
            idPedido,
            mensaje = $"Pedido {idPedido} aceptado. Destino: {destino}.",
            montoTotal = monto
        });
    }

    private static IResult Rechazo(string motivo) =>
        Results.Ok(new { aceptado = false, motivo });

    private static string? Validar(SolicitudPedido? solicitud)
    {
        if (solicitud == null) return "Falta el cuerpo de la solicitud.";
        if (string.IsNullOrWhiteSpace(solicitud.Destino)) return "Debe indicar el destino.";
        if (solicitud.Items == null || solicitud.Items.Length == 0)
            return "El pedido debe incluir al menos un producto.";

        for (int i = 0; i < solicitud.Items.Length; i++)
        {
            ItemSolicitud item = solicitud.Items[i];

            if (item == null || string.IsNullOrWhiteSpace(item.Codigo))
                return $"El producto #{i + 1} no tiene código.";
            if (DatosSimulados.IndiceProducto(item.Codigo) < 0)
                return $"El producto '{item.Codigo}' no existe en el catálogo.";
            if (item.Cantidad <= 0)
                return $"La cantidad de '{item.Codigo}' debe ser mayor que cero.";

            for (int j = 0; j < i; j++)
            {
                if (string.Equals(solicitud.Items[j].Codigo, item.Codigo, StringComparison.OrdinalIgnoreCase))
                    return $"El producto '{item.Codigo}' está repetido en el pedido.";
            }
        }

        return null;
    }

    private static string? BuscarDestino(string nombre)
    {
        for (int i = 0; i < DatosSimulados.Destinos.Length; i++)
        {
            if (string.Equals(DatosSimulados.Destinos[i], nombre, StringComparison.OrdinalIgnoreCase))
                return DatosSimulados.Destinos[i];
        }
        return null;
    }

    private static bool EsGuanacaste(string ubicacion) =>
        ubicacion == "Liberia" || ubicacion == "Cañas";

    // Simulado: devuelve siempre el primer pedido confirmado y no cambia el estado.
    private static IResult SiguienteEntrega(ConfigSede config)
    {
        PedidoSim[] pendientes = DatosSimulados.PorEstado("Confirmado");
        if (pendientes.Length == 0)
            return Results.Ok(new { hayPendientes = false, mensaje = "No hay entregas pendientes." });

        PedidoSim primero = pendientes[0];
        return Results.Ok(new
        {
            hayPendientes = true,
            ubicacionActual = config.Nombre,
            destino = primero.Destino,
            tiempoMinutos = 20,
            ruta = new[] { config.Nombre, primero.Destino },
            pedidos = new[] { primero.Id }
        });
    }
}