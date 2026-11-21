using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ConviTEC.Tests;

/// <summary>Pruebas de la sede de Liberia, que está aislada del resto del grafo (Guanacaste).</summary>
public class ApiLiberiaTests : IClassFixture<ApiLiberia>
{
    private readonly HttpClient _cliente;

    public ApiLiberiaTests(ApiLiberia api) => _cliente = api.CreateClient();

    [Fact]
    public async Task Sede_DevuelveLosDatosDeLiberia()
    {
        JsonElement sede = await _cliente.GetFromJsonAsync<JsonElement>("/api/sede");

        Assert.Equal("LIB", sede.GetProperty("codigo").GetString());
        Assert.Equal("Liberia", sede.GetProperty("nombre").GetString());
    }

    [Fact]
    public async Task RealizarPedido_HaciaFueraDeGuanacaste_EsRechazadoConMotivo()
    {
        HttpResponseMessage respuesta = await _cliente.PostJson("/api/pedidos",
            """{"destino":"Heredia","items":[{"codigo":"P001","cantidad":1}]}""");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        JsonElement cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(cuerpo.GetProperty("aceptado").GetBoolean());
        Assert.False(string.IsNullOrWhiteSpace(cuerpo.GetProperty("motivo").GetString()));
    }

    [Fact]
    public async Task RealizarPedido_DentroDeGuanacaste_EsAceptado()
    {
        HttpResponseMessage respuesta = await _cliente.PostJson("/api/pedidos",
            """{"destino":"Cañas","items":[{"codigo":"P001","cantidad":1}]}""");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        JsonElement cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(cuerpo.GetProperty("aceptado").GetBoolean());
    }
}