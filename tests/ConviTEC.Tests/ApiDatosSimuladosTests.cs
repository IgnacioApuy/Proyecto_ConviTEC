using System.Net.Http.Json;
using System.Text.Json;

namespace ConviTEC.Tests;

/// <summary>
/// Dependen de los valores de ConviTEC.Api/Simulacion.
/// BORRAR este archivo cuando los módulos reales reemplacen los datos simulados.
/// </summary>
public class ApiDatosSimuladosTests : IClassFixture<ApiAlajuela>
{
    private readonly HttpClient _cliente;

    public ApiDatosSimuladosTests(ApiAlajuela api) => _cliente = api.CreateClient();

    [Fact]
    public async Task ProductoEstrella_EsElCafeConSusIngresos()
    {
        JsonElement estrella = await _cliente.GetFromJsonAsync<JsonElement>("/api/estadisticas/producto-estrella");

        Assert.True(estrella.GetProperty("hayVentas").GetBoolean());
        Assert.Equal("P003", estrella.GetProperty("codigo").GetString());
        Assert.Equal(2, estrella.GetProperty("unidadesVendidas").GetInt32());
        Assert.Equal(7000, estrella.GetProperty("ingresos").GetInt64());
    }

    [Fact]
    public async Task PeoresPedidos_TienenElOrdenEsperadoDeLosDatosSimulados()
    {
        JsonElement pedidos = await _cliente.GetFromJsonAsync<JsonElement>("/api/estadisticas/peores-pedidos");

        Assert.Equal(3, pedidos.GetArrayLength());
        Assert.Equal("PED-002", pedidos[0].GetProperty("id").GetString());
        Assert.Equal("PED-003", pedidos[1].GetProperty("id").GetString());
        Assert.Equal("PED-001", pedidos[2].GetProperty("id").GetString());
    }
}