using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ConviTEC.Tests;

/// <summary>
/// Pruebas del contrato de docs/api-endpoints.md sobre la sede de Alajuela.
/// Deben seguir pasando cuando los datos simulados se reemplacen por los módulos reales.
/// </summary>
public class ApiContratoTests : IClassFixture<ApiAlajuela>
{
    private readonly HttpClient _cliente;

    public ApiContratoTests(ApiAlajuela api) => _cliente = api.CreateClient();

    private static void ExigeCampos(JsonElement elemento, params string[] campos)
    {
        for (int i = 0; i < campos.Length; i++)
            Assert.True(elemento.TryGetProperty(campos[i], out _), $"Falta el campo '{campos[i]}'.");
    }

    private async Task<JsonElement> Obtener(string ruta)
    {
        HttpResponseMessage respuesta = await _cliente.GetAsync(ruta);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        return await respuesta.Content.ReadFromJsonAsync<JsonElement>();
    }

    // ---------- Consultas ----------

    [Fact]
    public async Task Sede_DevuelveLosDatosDeLaSede()
    {
        JsonElement sede = await Obtener("/api/sede");

        Assert.Equal("ALA", sede.GetProperty("codigo").GetString());
        Assert.Equal("Alajuela", sede.GetProperty("nombre").GetString());
        Assert.Equal(5002, sede.GetProperty("puerto").GetInt32());
    }

    [Theory]
    [InlineData("/api/ventas")]
    [InlineData("/api/pedidos")]
    [InlineData("/api/pedidos/pendientes")]
    [InlineData("/api/inventario")]
    [InlineData("/api/catalogo")]
    [InlineData("/api/destinos")]
    [InlineData("/api/entregas/pendientes")]
    public async Task Consultas_DevuelvenUnArregloJson(string ruta)
    {
        JsonElement json = await Obtener(ruta);

        Assert.Equal(JsonValueKind.Array, json.ValueKind);
    }

    [Fact]
    public async Task Inventario_CadaProductoTraeSusCampos()
    {
        JsonElement inventario = await Obtener("/api/inventario");

        Assert.True(inventario.GetArrayLength() > 0);
        for (int i = 0; i < inventario.GetArrayLength(); i++)
            ExigeCampos(inventario[i], "codigo", "nombre", "categoria", "precio", "cantidad");
    }

    [Fact]
    public async Task Catalogo_CadaProductoTraeLaDisponibilidadPorSede()
    {
        JsonElement catalogo = await Obtener("/api/catalogo");

        Assert.True(catalogo.GetArrayLength() > 0);
        for (int i = 0; i < catalogo.GetArrayLength(); i++)
        {
            ExigeCampos(catalogo[i], "codigo", "nombre", "descripcion", "categoria", "precio", "disponibilidad");

            JsonElement disponibilidad = catalogo[i].GetProperty("disponibilidad");
            Assert.True(disponibilidad.GetArrayLength() > 0);
            for (int s = 0; s < disponibilidad.GetArrayLength(); s++)
                ExigeCampos(disponibilidad[s], "sede", "cantidad");
        }
    }

    [Fact]
    public async Task PedidosPendientes_SoloIncluyeLosConfirmados()
    {
        JsonElement pendientes = await Obtener("/api/pedidos/pendientes");

        for (int i = 0; i < pendientes.GetArrayLength(); i++)
            Assert.Equal("Confirmado", pendientes[i].GetProperty("estado").GetString());
    }

    [Fact]
    public async Task PeoresPedidos_VienenOrdenadosDeMenorAMayorRendimiento()
    {
        JsonElement pedidos = await Obtener("/api/estadisticas/peores-pedidos");

        double anterior = double.MinValue;
        for (int i = 0; i < pedidos.GetArrayLength(); i++)
        {
            ExigeCampos(pedidos[i], "id", "destino", "montoTotal", "tiempoTrasladoMin", "rendimiento");

            double rendimiento = pedidos[i].GetProperty("rendimiento").GetDouble();
            Assert.True(rendimiento >= anterior, "Los pedidos no están ordenados por rendimiento.");
            anterior = rendimiento;
        }
    }

    [Fact]
    public async Task ProductoEstrella_TieneLaEstructuraDelContrato()
    {
        JsonElement estrella = await Obtener("/api/estadisticas/producto-estrella");

        if (estrella.GetProperty("hayVentas").GetBoolean())
            ExigeCampos(estrella, "codigo", "nombre", "unidadesVendidas", "precio", "ingresos");
        else
            ExigeCampos(estrella, "mensaje");
    }

    // ---------- Realizar pedido ----------

    [Fact]
    public async Task RealizarPedido_Valido_EsAceptado()
    {
        HttpResponseMessage respuesta = await _cliente.PostJson("/api/pedidos",
            """{"destino":"Heredia","items":[{"codigo":"P001","cantidad":2}]}""");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        JsonElement cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(cuerpo.GetProperty("aceptado").GetBoolean());
        Assert.False(string.IsNullOrWhiteSpace(cuerpo.GetProperty("idPedido").GetString()));
        Assert.True(cuerpo.GetProperty("montoTotal").GetInt64() > 0);
    }

    [Fact]
    public async Task RealizarPedido_InventarioInsuficiente_EsRechazadoConMotivo()
    {
        HttpResponseMessage respuesta = await _cliente.PostJson("/api/pedidos",
            """{"destino":"Heredia","items":[{"codigo":"P003","cantidad":1000000}]}""");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        JsonElement cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(cuerpo.GetProperty("aceptado").GetBoolean());
        Assert.False(string.IsNullOrWhiteSpace(cuerpo.GetProperty("motivo").GetString()));
    }

    [Fact]
    public async Task RealizarPedido_DestinoInexistente_EsRechazadoConMotivo()
    {
        HttpResponseMessage respuesta = await _cliente.PostJson("/api/pedidos",
            """{"destino":"Narnia","items":[{"codigo":"P001","cantidad":1}]}""");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        JsonElement cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(cuerpo.GetProperty("aceptado").GetBoolean());
        Assert.False(string.IsNullOrWhiteSpace(cuerpo.GetProperty("motivo").GetString()));
    }

    [Fact]
    public async Task RealizarPedido_DesdeAlajuelaHaciaGuanacaste_EsRechazadoPorInalcanzable()
    {
        HttpResponseMessage respuesta = await _cliente.PostJson("/api/pedidos",
            """{"destino":"Cañas","items":[{"codigo":"P001","cantidad":1}]}""");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        JsonElement cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(cuerpo.GetProperty("aceptado").GetBoolean());
        Assert.False(string.IsNullOrWhiteSpace(cuerpo.GetProperty("motivo").GetString()));
    }

    [Theory]
    [InlineData("""{"destino":"Heredia","items":[]}""")]
    [InlineData("""{"items":[{"codigo":"P001","cantidad":1}]}""")]
    [InlineData("""{"destino":"   ","items":[{"codigo":"P001","cantidad":1}]}""")]
    [InlineData("""{"destino":"Heredia","items":[{"codigo":"P001","cantidad":0}]}""")]
    [InlineData("""{"destino":"Heredia","items":[{"codigo":"P001","cantidad":-5}]}""")]
    [InlineData("""{"destino":"Heredia","items":[{"codigo":"ZZZ","cantidad":1}]}""")]
    [InlineData("""{"destino":"Heredia","items":[{"codigo":"P001","cantidad":1},{"codigo":"P001","cantidad":2}]}""")]
    public async Task RealizarPedido_DatosInvalidos_DevuelveError400(string cuerpoJson)
    {
        HttpResponseMessage respuesta = await _cliente.PostJson("/api/pedidos", cuerpoJson);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        JsonElement cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrWhiteSpace(cuerpo.GetProperty("error").GetString()));
    }

    // ---------- Siguiente entrega ----------

    [Fact]
    public async Task SiguienteEntrega_CumpleElContratoDeRuta()
    {
        HttpResponseMessage respuesta = await _cliente.PostAsync("/api/entregas/siguiente", null);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        JsonElement cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();

        if (!cuerpo.GetProperty("hayPendientes").GetBoolean())
        {
            ExigeCampos(cuerpo, "mensaje");
            return;
        }

        ExigeCampos(cuerpo, "ubicacionActual", "destino", "tiempoMinutos", "ruta", "pedidos");

        JsonElement ruta = cuerpo.GetProperty("ruta");
        int ultimo = ruta.GetArrayLength() - 1;
        Assert.True(ultimo >= 1, "La ruta debe tener al menos origen y destino.");
        Assert.Equal(cuerpo.GetProperty("ubicacionActual").GetString(), ruta[0].GetString());
        Assert.Equal(cuerpo.GetProperty("destino").GetString(), ruta[ultimo].GetString());
        Assert.True(cuerpo.GetProperty("tiempoMinutos").GetInt32() > 0);
        Assert.True(cuerpo.GetProperty("pedidos").GetArrayLength() > 0);
    }

    // ---------- Frontend y rutas desconocidas ----------

    [Fact]
    public async Task Raiz_EntregaLaPaginaDelFrontend()
    {
        HttpResponseMessage respuesta = await _cliente.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("text/html", respuesta.Content.Headers.ContentType?.MediaType);
        Assert.Contains("ConviTEC", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ArchivosJavaScript_SeSirvenComoEstaticos()
    {
        HttpResponseMessage respuesta = await _cliente.GetAsync("/js/app.js");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [Fact]
    public async Task RutaDeApiDesconocida_Devuelve404()
    {
        HttpResponseMessage respuesta = await _cliente.GetAsync("/api/no-existe");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }
}