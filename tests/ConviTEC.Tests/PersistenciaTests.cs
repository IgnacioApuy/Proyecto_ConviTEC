using ConviTEC.Core.Contratos;
using ConviTEC.Core.Persistencia;

namespace ConviTEC.Tests;

public class PersistenciaTests : IDisposable
{
    private readonly string _carpeta;
    private readonly GestorArchivos _gestor;

    public PersistenciaTests()
    {
        _carpeta = Path.Combine(Path.GetTempPath(), "convitec-tests-" + Guid.NewGuid());
        _gestor = new GestorArchivos(_carpeta);
    }

    public void Dispose()
    {
        if (Directory.Exists(_carpeta))
            Directory.Delete(_carpeta, recursive: true);
    }

    // Módulo falso para probar el gestor sin depender de otros roles.
    private class ModuloFalso : IPersistible
    {
        public string NombreArchivo { get; }
        public string Contenido { get; set; } = "";

        public ModuloFalso(string nombreArchivo = "falso.json") => NombreArchivo = nombreArchivo;

        public string Serializar() => Contenido;

        public void Reconstruir(string json)
        {
            if (json == "invalido") throw new FormatException("JSON inválido");
            Contenido = json;
        }
    }

    [Fact]
    public void Guardar_CreaCarpetaYArchivo()
    {
        var modulo = new ModuloFalso { Contenido = "{\"a\":1}" };

        _gestor.Guardar(modulo);

        Assert.True(File.Exists(Path.Combine(_carpeta, "falso.json")));
    }

    [Fact]
    public void GuardarYCargar_RecuperaElMismoContenido()
    {
        var original = new ModuloFalso { Contenido = "{\"a\":1}" };
        _gestor.Guardar(original);

        var recuperado = new ModuloFalso();
        bool cargado = _gestor.Cargar(recuperado);

        Assert.True(cargado);
        Assert.Equal("{\"a\":1}", recuperado.Contenido);
    }

    [Fact]
    public void Cargar_ArchivoInexistente_DevuelveFalse()
    {
        var modulo = new ModuloFalso { Contenido = "intacto" };

        bool cargado = _gestor.Cargar(modulo);

        Assert.False(cargado);
        Assert.Equal("intacto", modulo.Contenido);
    }

    [Fact]
    public void Cargar_ArchivoVacio_LanzaInvalidDataException()
    {
        Directory.CreateDirectory(_carpeta);
        File.WriteAllText(Path.Combine(_carpeta, "falso.json"), "   ");

        Assert.Throws<InvalidDataException>(() => _gestor.Cargar(new ModuloFalso()));
    }

    [Fact]
    public void Cargar_ContenidoInvalido_LanzaInvalidDataException()
    {
        Directory.CreateDirectory(_carpeta);
        File.WriteAllText(Path.Combine(_carpeta, "falso.json"), "invalido");

        Assert.Throws<InvalidDataException>(() => _gestor.Cargar(new ModuloFalso()));
    }

    [Fact]
    public void Guardar_SobreescribeArchivoExistente()
    {
        var modulo = new ModuloFalso { Contenido = "version1" };
        _gestor.Guardar(modulo);

        modulo.Contenido = "version2";
        _gestor.Guardar(modulo);

        var recuperado = new ModuloFalso();
        _gestor.Cargar(recuperado);
        Assert.Equal("version2", recuperado.Contenido);
    }

    [Fact]
    public void Guardar_NoDejaArchivoTemporal()
    {
        _gestor.Guardar(new ModuloFalso { Contenido = "x" });

        Assert.False(File.Exists(Path.Combine(_carpeta, "falso.json.tmp")));
    }

    [Fact]
    public void ModulosDistintos_UsanArchivosDistintos()
    {
        _gestor.Guardar(new ModuloFalso("inventario.json") { Contenido = "inv" });
        _gestor.Guardar(new ModuloFalso("pedidos.json") { Contenido = "ped" });

        var inv = new ModuloFalso("inventario.json");
        var ped = new ModuloFalso("pedidos.json");
        _gestor.Cargar(inv);
        _gestor.Cargar(ped);

        Assert.Equal("inv", inv.Contenido);
        Assert.Equal("ped", ped.Contenido);
    }

    [Fact]
    public void Guardar_ModuloNulo_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _gestor.Guardar(null!));
    }

    [Fact]
    public void Constructor_CarpetaVacia_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new GestorArchivos(""));
    }

    [Fact]
    public void Existe_ReflejaSiElArchivoFueGuardado()
    {
        var modulo = new ModuloFalso { Contenido = "x" };
        Assert.False(_gestor.Existe(modulo));

        _gestor.Guardar(modulo);

        Assert.True(_gestor.Existe(modulo));
    }
}