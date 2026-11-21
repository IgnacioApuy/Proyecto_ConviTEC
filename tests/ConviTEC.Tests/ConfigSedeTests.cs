using ConviTEC.Core.Configuracion;

namespace ConviTEC.Tests;

public class ConfigSedeTests : IDisposable
{
    private readonly string _carpeta;

    public ConfigSedeTests()
    {
        _carpeta = Path.Combine(Path.GetTempPath(), "convitec-config-" + Guid.NewGuid());
        Directory.CreateDirectory(_carpeta);
    }

    public void Dispose() => Directory.Delete(_carpeta, recursive: true);

    private string Escribir(string contenido)
    {
        string ruta = Path.Combine(_carpeta, "sede.json");
        File.WriteAllText(ruta, contenido);
        return ruta;
    }

    private const string JsonValido = """
        {
          "codigo": "LIB",
          "nombre": "Liberia",
          "puerto": 5001,
          "carpetaDatos": "data/sedes/liberia",
          "rabbitMq": { "host": "192.168.1.10", "puerto": 5672, "usuario": "u", "contrasena": "c" }
        }
        """;

    [Fact]
    public void Cargar_ArchivoValido_LeeTodosLosCampos()
    {
        var config = ConfigSede.Cargar(Escribir(JsonValido));

        Assert.Equal("LIB", config.Codigo);
        Assert.Equal("Liberia", config.Nombre);
        Assert.Equal(5001, config.Puerto);
        Assert.Equal("data/sedes/liberia", config.CarpetaDatos);
        Assert.Equal("192.168.1.10", config.RabbitMq.Host);
    }

    [Fact]
    public void Cargar_RabbitMqOmitido_UsaValoresPorDefecto()
    {
        var config = ConfigSede.Cargar(Escribir(
            """{ "codigo": "LIB", "nombre": "Liberia", "puerto": 5001, "carpetaDatos": "d" }"""));

        Assert.Equal("localhost", config.RabbitMq.Host);
        Assert.Equal(5672, config.RabbitMq.Puerto);
    }

    [Fact]
    public void Cargar_ArchivoInexistente_LanzaFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() =>
            ConfigSede.Cargar(Path.Combine(_carpeta, "no-existe.json")));
    }

    [Fact]
    public void Cargar_JsonMalformado_LanzaInvalidDataException()
    {
        Assert.Throws<InvalidDataException>(() => ConfigSede.Cargar(Escribir("{ esto no es json")));
    }

    [Fact]
    public void Cargar_PuertoInvalido_LanzaInvalidDataException()
    {
        Assert.Throws<InvalidDataException>(() => ConfigSede.Cargar(Escribir(
            """{ "codigo": "LIB", "nombre": "Liberia", "puerto": 0, "carpetaDatos": "d" }""")));
    }

    [Fact]
    public void Cargar_SinNombre_LanzaInvalidDataException()
    {
        Assert.Throws<InvalidDataException>(() => ConfigSede.Cargar(Escribir(
            """{ "codigo": "LIB", "puerto": 5001, "carpetaDatos": "d" }""")));
    }
}