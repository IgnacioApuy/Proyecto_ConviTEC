using ConviTEC.Core.Estructuras;
using Xunit;

namespace ConviTEC.Tests;

public class ArbolAVLTests
{
    [Fact]
    public void ArbolVacio_BuscarYEliminar_DevuelvenFalse()
    {
        var arbol = new ArbolAVL<int, string>();

        Assert.True(arbol.EstaVacio);
        Assert.Equal(0, arbol.Altura);
        Assert.False(arbol.Buscar(1, out _));
        Assert.False(arbol.Eliminar(1));
        Assert.Empty(arbol.ObtenerValoresInOrden());
        Assert.True(arbol.EsValido());
    }

    [Fact]
    public void Insertar_ClaveDuplicada_SeRechazaYConservaElValorOriginal()
    {
        var arbol = new ArbolAVL<int, string>();

        Assert.True(arbol.Insertar(5, "original"));
        Assert.False(arbol.Insertar(5, "duplicado"));

        Assert.Equal(1, arbol.Cantidad);
        Assert.True(arbol.Buscar(5, out string? valor));
        Assert.Equal("original", valor);
    }

    [Fact]
    public void Buscar_ClaveInexistente_DevuelveFalse()
    {
        var arbol = new ArbolAVL<int, string>();
        arbol.Insertar(1, "uno");

        Assert.False(arbol.Buscar(2, out _));
        Assert.False(arbol.Contiene(2));
    }

    [Fact]
    public void ClaveNula_LanzaExcepcion()
    {
        var arbol = new ArbolAVL<string, int>();

        Assert.Throws<ArgumentNullException>(() => arbol.Insertar(null!, 1));
        Assert.Throws<ArgumentNullException>(() => arbol.Contiene(null!));
    }

    [Fact]
    public void Actualizar_ClaveExistente_CambiaElValor()
    {
        var arbol = new ArbolAVL<int, string>();
        arbol.Insertar(1, "viejo");

        Assert.True(arbol.Actualizar(1, "nuevo"));
        Assert.False(arbol.Actualizar(2, "x"));

        arbol.Buscar(1, out string? valor);
        Assert.Equal("nuevo", valor);
    }

    [Theory]
    [InlineData(new[] { 30, 20, 10 })] 
    [InlineData(new[] { 10, 20, 30 })] 
    [InlineData(new[] { 30, 10, 20 })] 
    [InlineData(new[] { 10, 30, 20 })] 
    public void Insertar_TresClaves_CualquierOrdenQuedaConAlturaDos(int[] claves)
    {
        var arbol = new ArbolAVL<int, int>();

        foreach (int clave in claves)
            arbol.Insertar(clave, clave);

        Assert.Equal(2, arbol.Altura);
        Assert.True(arbol.EsValido());
        Assert.Equal(new[] { 10, 20, 30 }, arbol.ObtenerClavesInOrden());
    }

    [Fact]
    public void Insertar_ClavesAscendentes_MantieneElArbolBalanceado()
    {
        var arbol = new ArbolAVL<int, int>();

        for (int i = 1; i <= 1000; i++)
            arbol.Insertar(i, i);

        Assert.Equal(1000, arbol.Cantidad);
        Assert.True(arbol.EsValido());
        
        Assert.True(arbol.Altura <= 14);
    }

    [Fact]
    public void RecorridoInOrden_DevuelveLasClavesOrdenadas()
    {
        var arbol = new ArbolAVL<int, string>();
        int[] claves = { 50, 20, 70, 10, 30, 60, 80, 25, 35 };

        foreach (int clave in claves)
            arbol.Insertar(clave, "v" + clave);

        Assert.Equal(new[] { 10, 20, 25, 30, 35, 50, 60, 70, 80 }, arbol.ObtenerClavesInOrden());
        Assert.Equal("v10", arbol.ObtenerValoresInOrden()[0]);
    }

    [Fact]
    public void RecorrerInOrden_VisitaCadaNodoUnaVez()
    {
        var arbol = new ArbolAVL<int, int>();
        for (int i = 1; i <= 5; i++)
            arbol.Insertar(i, i * 10);

        int suma = 0;
        arbol.RecorrerInOrden((clave, valor) => suma += valor);

        Assert.Equal(150, suma);
    }

    [Fact]
    public void Eliminar_Hoja_UnHijoYDosHijos_ConservanElArbolValido()
    {
        var arbol = new ArbolAVL<int, int>();
        int[] claves = { 50, 30, 70, 20, 40, 60, 80, 10 };
        foreach (int clave in claves)
            arbol.Insertar(clave, clave);

        Assert.True(arbol.Eliminar(10)); 
        Assert.True(arbol.Eliminar(20)); 
        Assert.True(arbol.Eliminar(50)); 

        Assert.True(arbol.EsValido());
        Assert.Equal(5, arbol.Cantidad);
        Assert.Equal(new[] { 30, 40, 60, 70, 80 }, arbol.ObtenerClavesInOrden());
        Assert.False(arbol.Contiene(50));
    }

    [Fact]
    public void Eliminar_ClaveInexistente_DevuelveFalseYNoCambiaElArbol()
    {
        var arbol = new ArbolAVL<int, int>();
        arbol.Insertar(1, 1);

        Assert.False(arbol.Eliminar(2));
        Assert.Equal(1, arbol.Cantidad);
    }

    [Fact]
    public void Eliminar_MuchasClaves_MantieneElBalanceoYLasClavesRestantes()
    {
        var arbol = new ArbolAVL<int, int>();
        for (int i = 1; i <= 200; i++)
            arbol.Insertar(i, i);

        for (int i = 2; i <= 200; i += 2) 
        {
            Assert.True(arbol.Eliminar(i));
            Assert.True(arbol.EsValido());
        }

        Assert.Equal(100, arbol.Cantidad);
        Assert.True(arbol.Contiene(1));
        Assert.True(arbol.Contiene(199));
        Assert.False(arbol.Contiene(2));
    }

    [Fact]
    public void Eliminar_TodasLasClaves_DejaElArbolVacio()
    {
        var arbol = new ArbolAVL<int, int>();
        for (int i = 1; i <= 10; i++)
            arbol.Insertar(i, i);

        for (int i = 1; i <= 10; i++)
            arbol.Eliminar(i);

        Assert.True(arbol.EstaVacio);
        Assert.Equal(0, arbol.Cantidad);
    }
}
