using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ConviTEC.Tests;

/// <summary>
/// Levanta la API de una sede en memoria. La sede se elige con la variable de entorno
/// CONVITEC_SEDE, que Program.cs lee al arrancar.
/// </summary>
public abstract class ApiFactory : WebApplicationFactory<Program>
{
    // La variable de entorno es global al proceso: se serializa para que
    // dos fábricas creadas en paralelo no se pisen.
    private static readonly object Candado = new();

    protected ApiFactory(string sede)
    {
        lock (Candado)
        {
            string? anterior = Environment.GetEnvironmentVariable("CONVITEC_SEDE");
            Environment.SetEnvironmentVariable("CONVITEC_SEDE", sede);
            try
            {
                _ = Server; // Fuerza a construir la aplicación ahora, mientras la variable está fijada.
            }
            finally
            {
                Environment.SetEnvironmentVariable("CONVITEC_SEDE", anterior);
            }
        }
    }
}

public class ApiAlajuela : ApiFactory
{
    public ApiAlajuela() : base("alajuela") { }
}

public class ApiLiberia : ApiFactory
{
    public ApiLiberia() : base("liberia") { }
}

public static class ClienteExtensiones
{
    public static Task<HttpResponseMessage> PostJson(this HttpClient cliente, string ruta, string json) =>
        cliente.PostAsync(ruta, new StringContent(json, Encoding.UTF8, "application/json"));
}