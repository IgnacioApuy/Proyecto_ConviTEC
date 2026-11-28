namespace ConviTEC.Core.Modelo;

//Sede de ConviTEC: código único, nombre y ubicación (nombre del vértice en el grafo de rutas).
public class Sede
{
    public string Codigo { get; }
    public string Nombre { get; }

    //Ubicación geográfica. Debe coincidir con el nombre del vértice de la sede en data/rutas.json.
    public string Ubicacion { get; }

    public Sede(string codigo, string nombre, string ubicacion)
    {
        Codigo = Producto.Requerido(codigo, nameof(codigo));
        Nombre = Producto.Requerido(nombre, nameof(nombre));
        Ubicacion = Producto.Requerido(ubicacion, nameof(ubicacion));
    }

    public override string ToString() => $"{Codigo} - {Nombre}";
}
    