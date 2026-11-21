using ConviTEC.Core.Configuracion;
using ConviTEC.Core.Persistencia;
using Microsoft.Extensions.FileProviders;
using ConviTEC.Api.Endpoints;

string raiz;
ConfigSede config;

try
{
    raiz = BuscarRaiz();
    string nombreSede = ObtenerNombreSede(args);
    config = ConfigSede.Cargar(Path.Combine(raiz, "config", $"sede-{nombreSede}.json"));
}
catch (Exception ex) when (ex is ArgumentException or IOException or InvalidDataException)
{
    Console.Error.WriteLine($"Error de configuración: {ex.Message}");
    return 1;
}

string carpetaDatos = Path.GetFullPath(Path.Combine(raiz, config.CarpetaDatos));
Directory.CreateDirectory(carpetaDatos);

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://localhost:{config.Puerto}");
builder.Services.AddSingleton(config);
builder.Services.AddSingleton(new GestorArchivos(carpetaDatos));

var app = builder.Build();

// El frontend (HTML/CSS/JS) vive en la carpeta frontend/ de la raíz del repositorio.
var archivosFrontend = new PhysicalFileProvider(Path.Combine(raiz, "frontend"));
app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = archivosFrontend });
app.UseStaticFiles(new StaticFileOptions { FileProvider = archivosFrontend });

app.MapGet("/api/sede", () => Results.Ok(new
{
    codigo = config.Codigo,
    nombre = config.Nombre,
    puerto = config.Puerto
}));
app.MapEndpointsSimulados(config);

Console.WriteLine($"Sede {config.Nombre} ({config.Codigo}) en http://localhost:{config.Puerto}");
Console.WriteLine($"Datos locales: {carpetaDatos}");

app.Run();
return 0;

// Sube por los directorios hasta encontrar la raíz del repo (tiene config/ y src/).
static string BuscarRaiz()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null)
    {
        if (Directory.Exists(Path.Combine(dir.FullName, "config")) &&
            Directory.Exists(Path.Combine(dir.FullName, "src")))
            return dir.FullName;

        dir = dir.Parent;
    }

    throw new DirectoryNotFoundException("No se encontró la raíz del proyecto (carpetas 'config' y 'src').");
}

// La sede se indica con --sede <nombre> o con la variable de entorno CONVITEC_SEDE.
static string ObtenerNombreSede(string[] args)
{
    for (int i = 0; i < args.Length - 1; i++)
    {
        if (args[i] == "--sede")
            return args[i + 1].ToLowerInvariant();
    }

    string? entorno = Environment.GetEnvironmentVariable("CONVITEC_SEDE");
    if (!string.IsNullOrWhiteSpace(entorno))
        return entorno.ToLowerInvariant();

    throw new ArgumentException(
        "Indique la sede: dotnet run --project src\\ConviTEC.Api -- --sede liberia");
}