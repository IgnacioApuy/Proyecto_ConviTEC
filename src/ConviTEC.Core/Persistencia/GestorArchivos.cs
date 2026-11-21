using ConviTEC.Core.Contratos;

namespace ConviTEC.Core.Persistencia;

/// <summary>
/// Guarda y carga módulos IPersistible como archivos JSON en la carpeta de datos de una sede.
/// </summary>
public class GestorArchivos
{
    private readonly string _carpetaDatos;
    private readonly object _candado = new();

    public GestorArchivos(string carpetaDatos)
    {
        if (string.IsNullOrWhiteSpace(carpetaDatos))
            throw new ArgumentException("La carpeta de datos no puede estar vacía.", nameof(carpetaDatos));

        _carpetaDatos = carpetaDatos;
    }

    public string CarpetaDatos => _carpetaDatos;

    /// <summary>Serializa el módulo y lo escribe en disco de forma segura.</summary>
    public void Guardar(IPersistible modulo)
    {
        ArgumentNullException.ThrowIfNull(modulo);

        string json = modulo.Serializar();
        string destino = RutaDe(modulo);
        string temporal = destino + ".tmp";

        lock (_candado)
        {
            Directory.CreateDirectory(_carpetaDatos);
            File.WriteAllText(temporal, json);
            File.Move(temporal, destino, overwrite: true);
        }
    }

    /// <summary>
    /// Carga el archivo del módulo y reconstruye su estado.
    /// Devuelve false si el archivo no existe (primer arranque).
    /// Lanza InvalidDataException si el archivo está vacío o es inválido.
    /// </summary>
    public bool Cargar(IPersistible modulo)
    {
        ArgumentNullException.ThrowIfNull(modulo);

        string origen = RutaDe(modulo);
        string json;

        lock (_candado)
        {
            if (!File.Exists(origen))
                return false;

            json = File.ReadAllText(origen);
        }

        if (string.IsNullOrWhiteSpace(json))
            throw new InvalidDataException($"El archivo '{modulo.NombreArchivo}' está vacío.");

        try
        {
            modulo.Reconstruir(json);
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException(
                $"El archivo '{modulo.NombreArchivo}' es inválido: {ex.Message}", ex);
        }

        return true;
    }

    public bool Existe(IPersistible modulo)
    {
        ArgumentNullException.ThrowIfNull(modulo);
        return File.Exists(RutaDe(modulo));
    }

    private string RutaDe(IPersistible modulo)
    {
        if (string.IsNullOrWhiteSpace(modulo.NombreArchivo))
            throw new ArgumentException("El módulo no define un nombre de archivo.", nameof(modulo));

        return Path.Combine(_carpetaDatos, modulo.NombreArchivo);
    }
}