namespace ConviTEC.Core.Modelo;

//Producto que comercializa ConviTEC. Es inmutable: para cambiar un dato se crea un Producto nuevo

public class Producto
{
    public string Codigo { get; }
    public string Nombre { get; }
    public string Descripcion { get; }
    public string Categoria { get; }

    //Precio de venta en colones.
    public decimal Precio { get; }

    public Producto(string codigo, string nombre, string descripcion, string categoria, decimal precio)
    {
        Codigo = Requerido(codigo, nameof(codigo));
        Nombre = Requerido(nombre, nameof(nombre));
        Categoria = Requerido(categoria, nameof(categoria));
        Descripcion = descripcion?.Trim() ?? string.Empty; // la descripción puede estar vacía

        if (precio < 0)
            throw new ArgumentOutOfRangeException(nameof(precio), "El precio no puede ser negativo.");

        Precio = precio;
    }

    internal static string Requerido(string? valor, string nombreParametro)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("El valor es obligatorio y no puede estar vacío.", nombreParametro);

        return valor.Trim();
    }

    public override string ToString() => $"{Codigo} - {Nombre} (₡{Precio})";
}
