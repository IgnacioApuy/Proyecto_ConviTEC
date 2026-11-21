using System.Text.Json;

namespace ConviTEC.Core.Configuracion;

/// <summary>Datos de conexión a RabbitMQ de una sede.</summary>
public class ConfigRabbitMq
{
    public string Host { get; set; } = "localhost";
    public int Puerto { get; set; } = 5672;
    public string Usuario { get; set; } = "guest";
    public string Contrasena { get; set; } = "guest";
}

/// <summary>Configuración de una sede, leída desde config/sede-*.json.</summary>
public class ConfigSede
{
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public int Puerto { get; set; }
    public string CarpetaDatos { get; set; } = "";
    public ConfigRabbitMq RabbitMq { get; set; } = new();

    /// <summary>
    /// Carga y valida la configuración. Lanza FileNotFoundException si el archivo
    /// no existe e InvalidDataException si el contenido es inválido.
    /// </summary>
    public static ConfigSede Cargar(string ruta)
    {
        if (!File.Exists(ruta))
            throw new FileNotFoundException($"No existe el archivo de configuración: {ruta}", ruta);

        string json = File.ReadAllText(ruta);

        ConfigSede? config;
        try
        {
            config = JsonSerializer.Deserialize<ConfigSede>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"El JSON de '{ruta}' es inválido: {ex.Message}", ex);
        }

        if (config == null)
            throw new InvalidDataException($"El archivo '{ruta}' está vacío.");

        config.Validar();
        return config;
    }

    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Codigo))
            throw new InvalidDataException("La configuración no define 'codigo'.");
        if (string.IsNullOrWhiteSpace(Nombre))
            throw new InvalidDataException("La configuración no define 'nombre'.");
        if (Puerto < 1 || Puerto > 65535)
            throw new InvalidDataException($"Puerto inválido: {Puerto}.");
        if (string.IsNullOrWhiteSpace(CarpetaDatos))
            throw new InvalidDataException("La configuración no define 'carpetaDatos'.");
        if (string.IsNullOrWhiteSpace(RabbitMq.Host))
            throw new InvalidDataException("La configuración de RabbitMQ no define 'host'.");
    }
}