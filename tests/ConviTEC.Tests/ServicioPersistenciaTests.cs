using ConviTEC.Core.Contratos;
using ConviTEC.Core.Persistencia;

namespace ConviTEC.Tests;

public class ServicioPersistenciaTests : IDisposable
{
    private readonly string _carpeta;
    private readonly ServicioPersistencia _servicio;

    public ServicioPersistenciaTests()
    {
        _carpeta = Path.Combine(Path.GetTempPath(), "convitec-servicio-" + Guid.NewGuid());
        Directory.CreateDirectory(_carpeta);
        _servicio = new ServicioPersistencia(new GestorArchivos(_carpeta));
    }

    public void Dispose() => Directory.Delete(_carpeta, recursive: true);

    private void Escribir(string archivo, string contenido) =>
        File.WriteAllText(Path.Combine(_carpeta, archivo), contenido);

    // Anota el orden en que se cargan los módulos (sin colecciones nativas).
    private class Bitacora
    {
        public string[] Eventos = new string[16];
        public int Cantidad;
        public void Registrar(string evento) => Eventos[Cantidad++] = evento;
    }

    private class ModuloFalso : IPersistible
    {
        private readonly Bitacora? _bitacora;
        public string NombreArchivo { get; }
        public string Contenido { get; set; } = "";

        public ModuloFalso(string nombreArchivo, Bitacora? bitacora = null)
        {
            NombreArchivo = nombreArchivo;
            _bitacora = bitacora;
        }

        public string Serializar() => Contenido;

        public void Reconstruir(string json)
        {
            if (json == "invalido") throw new FormatException("JSON inválido");
            Contenido = json;
            _bitacora?.Registrar(NombreArchivo);
        }
    }

    private class ModuloValidable : ModuloFalso, IValidable
    {
        public bool Falla { get; set; }

        public ModuloValidable(string nombreArchivo) : base(nombreArchivo) { }

        public void Validar()
        {
            if (Falla) throw new InvalidOperationException("referencia rota");
        }
    }

    [Fact]
    public void CargarTodo_SinArchivos_ReportaSinArchivo()
    {
        _servicio.Registrar(new ModuloFalso("a.json"));

        ResultadoCarga[] resultados = _servicio.CargarTodo();

        Assert.Single(resultados);
        Assert.Equal(EstadoCarga.SinArchivo, resultados[0].Estado);
    }

    [Fact]
    public void GuardarTodo_LuegoCargarTodo_RecuperaLosDatos()
    {
        _servicio.Registrar(new ModuloFalso("a.json") { Contenido = "datosA" });
        _servicio.Registrar(new ModuloFalso("b.json") { Contenido = "datosB" });
        _servicio.GuardarTodo();

        var nuevoServicio = new ServicioPersistencia(new GestorArchivos(_carpeta));
        var a = new ModuloFalso("a.json");
        var b = new ModuloFalso("b.json");
        nuevoServicio.Registrar(a);
        nuevoServicio.Registrar(b);

        ResultadoCarga[] resultados = nuevoServicio.CargarTodo();

        Assert.Equal("datosA", a.Contenido);
        Assert.Equal("datosB", b.Contenido);
        Assert.Equal(EstadoCarga.Cargado, resultados[0].Estado);
        Assert.Equal(EstadoCarga.Cargado, resultados[1].Estado);
    }

    [Fact]
    public void CargarTodo_RespetaElOrdenDeRegistro()
    {
        var bitacora = new Bitacora();
        Escribir("b.json", "x");
        Escribir("a.json", "x");
        Escribir("c.json", "x");

        _servicio.Registrar(new ModuloFalso("b.json", bitacora));
        _servicio.Registrar(new ModuloFalso("a.json", bitacora));
        _servicio.Registrar(new ModuloFalso("c.json", bitacora));
        _servicio.CargarTodo();

        Assert.Equal(3, bitacora.Cantidad);
        Assert.Equal("b.json", bitacora.Eventos[0]);
        Assert.Equal("a.json", bitacora.Eventos[1]);
        Assert.Equal("c.json", bitacora.Eventos[2]);
    }

    [Fact]
    public void CargarTodo_ArchivoCorrupto_LanzaYNoLoSobrescribe()
    {
        Escribir("a.json", "invalido");
        _servicio.Registrar(new ModuloFalso("a.json"));

        Assert.Throws<InvalidDataException>(() => _servicio.CargarTodo());

        Assert.Equal("invalido", File.ReadAllText(Path.Combine(_carpeta, "a.json")));
    }

    [Fact]
    public void CargarTodo_ValidacionFalla_LanzaConMensajeClaro()
    {
        Escribir("a.json", "x");
        _servicio.Registrar(new ModuloValidable("a.json") { Falla = true });

        var ex = Assert.Throws<InvalidDataException>(() => _servicio.CargarTodo());

        Assert.Contains("a.json", ex.Message);
        Assert.Contains("referencia rota", ex.Message);
    }

    [Fact]
    public void Registrar_ArchivoDuplicado_LanzaArgumentException()
    {
        _servicio.Registrar(new ModuloFalso("a.json"));

        Assert.Throws<ArgumentException>(() => _servicio.Registrar(new ModuloFalso("A.JSON")));
    }

    [Fact]
    public void Registrar_ModuloNulo_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _servicio.Registrar(null!));
    }

    [Fact]
    public void Registrar_MasModulosQueLaCapacidadInicial_FuncionaCorrectamente()
    {
        for (int i = 0; i < 10; i++)
            _servicio.Registrar(new ModuloFalso($"modulo{i}.json"));

        Assert.Equal(10, _servicio.Cantidad);
        Assert.Equal(10, _servicio.CargarTodo().Length);
    }

    [Fact]
    public void Guardar_ModuloNoRegistrado_LanzaInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _servicio.Guardar(new ModuloFalso("a.json") { Contenido = "x" }));
    }

    [Fact]
    public void Guardar_ModuloRegistrado_EscribeElArchivo()
    {
        var modulo = new ModuloFalso("a.json") { Contenido = "datos" };
        _servicio.Registrar(modulo);

        _servicio.Guardar(modulo);

        Assert.Equal("datos", File.ReadAllText(Path.Combine(_carpeta, "a.json")));
    }

    [Fact]
    public void CargarTodo_SinModulosRegistrados_DevuelveArregloVacio()
    {
        Assert.Empty(_servicio.CargarTodo());
    }
}