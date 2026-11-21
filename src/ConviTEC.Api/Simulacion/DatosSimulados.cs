namespace ConviTEC.Api.Simulacion;

public record ProductoSim(string Codigo, string Nombre, string Descripcion, string Categoria, int Precio);

public record Detalle(string Codigo, string Nombre, int Cantidad, int PrecioUnitario)
{
    public int Subtotal => Cantidad * PrecioUnitario;
}

public record PedidoSim(
    string Id,
    DateTime FechaCreacion,
    string Estado,
    string Destino,
    string[] SedesParticipantes,
    Detalle[] Detalles,
    DateTime? FechaEntrega = null,
    int? TiempoTrasladoMin = null)
{
    public int MontoTotal
    {
        get
        {
            int total = 0;
            for (int i = 0; i < Detalles.Length; i++)
                total += Detalles[i].Subtotal;
            return total;
        }
    }

    // Rendimiento = monto (colones) / tiempo de traslado (minutos). Solo existe si fue entregado.
    public double? Rendimiento =>
        TiempoTrasladoMin is > 0 and int minutos ? Math.Round((double)MontoTotal / minutos, 2) : null;
}

/// <summary>Datos de mentira para que el frontend avance. Se reemplazan al integrar los módulos reales.</summary>
public static class DatosSimulados
{
    public static readonly string[] Sedes = { "Liberia", "Alajuela", "San José", "Limón" };

    // Ubicaciones de la tabla de rutas del enunciado.
    public static readonly string[] Destinos =
    {
        "Liberia", "Cañas", "Puntarenas", "Orotina", "Alajuela", "San José", "Heredia",
        "Cartago", "Paraíso", "Turrialba", "Guápiles", "Siquirres", "Limón"
    };

    public static readonly ProductoSim[] Productos =
    {
        new("P001", "Agua 600 ml", "Botella de agua purificada", "Bebidas", 800),
        new("P002", "Galletas de chocolate", "Paquete de galletas con chips de chocolate", "Snacks", 1200),
        new("P003", "Café molido 250 g", "Café tostado y molido", "Abarrotes", 3500),
        new("P004", "Leche entera 1 L", "Leche entera pasteurizada", "Lácteos", 1100),
        new("P005", "Pan de molde", "Pan de molde blanco", "Panadería", 1900),
    };

    // Existencias[producto][sede], con las sedes en el orden del arreglo Sedes.
    public static readonly int[][] Existencias =
    {
        new[] { 30, 50, 40, 20 },
        new[] { 15, 25, 30, 10 },
        new[] { 5, 12, 8, 3 },
        new[] { 20, 40, 35, 15 },
        new[] { 0, 10, 6, 4 },
    };

    public static readonly PedidoSim[] Pedidos =
    {
        new("PED-001", new DateTime(2026, 11, 10, 9, 30, 0), "Entregado", "Heredia",
            new[] { "Alajuela" }, new[] { D("P001", 3), D("P002", 2) },
            new DateTime(2026, 11, 10, 11, 40, 0), 20),

        new("PED-002", new DateTime(2026, 11, 12, 14, 0, 0), "Entregado", "Turrialba",
            new[] { "Alajuela", "San José" }, new[] { D("P003", 2) },
            new DateTime(2026, 11, 12, 15, 50, 0), 65),

        new("PED-003", new DateTime(2026, 11, 15, 10, 0, 0), "Entregado", "Cartago",
            new[] { "San José" }, new[] { D("P004", 5), D("P001", 2) },
            new DateTime(2026, 11, 15, 11, 35, 0), 35),

        new("PED-004", new DateTime(2026, 11, 19, 16, 20, 0), "Confirmado", "Heredia",
            new[] { "Alajuela" }, new[] { D("P002", 4) }),

        new("PED-005", new DateTime(2026, 11, 20, 9, 10, 0), "Cancelado", "Puntarenas",
            new[] { "Alajuela", "Limón" }, new[] { D("P005", 10) }),

        new("PED-006", new DateTime(2026, 11, 20, 13, 45, 0), "Confirmado", "Cartago",
            new[] { "San José" }, new[] { D("P001", 6), D("P005", 1) }),
    };

    private static Detalle D(string codigo, int cantidad)
    {
        ProductoSim producto = Productos[IndiceProducto(codigo)];
        return new Detalle(producto.Codigo, producto.Nombre, cantidad, producto.Precio);
    }

    public static int IndiceProducto(string codigo)
    {
        for (int i = 0; i < Productos.Length; i++)
        {
            if (string.Equals(Productos[i].Codigo, codigo, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    public static int IndiceDeSede(string nombre)
    {
        for (int i = 0; i < Sedes.Length; i++)
        {
            if (string.Equals(Sedes[i], nombre, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    public static PedidoSim[] PorEstado(string estado)
    {
        int cantidad = 0;
        for (int i = 0; i < Pedidos.Length; i++)
            if (Pedidos[i].Estado == estado) cantidad++;

        var resultado = new PedidoSim[cantidad];
        int posicion = 0;
        for (int i = 0; i < Pedidos.Length; i++)
            if (Pedidos[i].Estado == estado) resultado[posicion++] = Pedidos[i];

        return resultado;
    }
}