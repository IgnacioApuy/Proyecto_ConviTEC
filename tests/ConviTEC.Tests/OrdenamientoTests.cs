using ConviTEC.Core.Estructuras;
using Xunit;

namespace ConviTEC.Tests;

public class OrdenamientoTests
{
    private static int CompararEnteros(int a, int b) => a.CompareTo(b);

    [Fact]
    public void MergeSort_ArregloVacioOUnElemento_NoFalla()
    {
        int[] vacio = Array.Empty<int>();
        int[] unico = { 5 };

        Ordenamiento.MergeSort(vacio, CompararEnteros);
        Ordenamiento.MergeSort(unico, CompararEnteros);

        Assert.Empty(vacio);
        Assert.Equal(new[] { 5 }, unico);
    }

    [Fact]
    public void MergeSort_ArregloNulo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentNullException>(() => Ordenamiento.MergeSort<int>(null!, CompararEnteros));
    }

    [Fact]
    public void MergeSort_ConDuplicados_OrdenaCorrectamente()
    {
        int[] datos = { 3, 1, 3, 2, 1 };

        Ordenamiento.MergeSort(datos, CompararEnteros);

        Assert.Equal(new[] { 1, 1, 2, 3, 3 }, datos);
    }

    [Fact]
    public void MergeSort_YaOrdenadoEInverso_OrdenaCorrectamente()
    {
        int[] ordenado = { 1, 2, 3, 4, 5 };
        int[] inverso = { 5, 4, 3, 2, 1 };

        Ordenamiento.MergeSort(ordenado, CompararEnteros);
        Ordenamiento.MergeSort(inverso, CompararEnteros);

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, ordenado);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, inverso);
    }

    [Fact]
    public void MergeSort_ConComparadorInvertido_OrdenaDescendente()
    {
        int[] datos = { 2, 9, 4, 7 };

        Ordenamiento.MergeSort(datos, (a, b) => b.CompareTo(a));

        Assert.Equal(new[] { 9, 7, 4, 2 }, datos);
    }

    [Fact]
    public void MergeSort_EsEstable_ConservaElOrdenDeLosEmpates()
    {
        (int Clave, string Etiqueta)[] datos =
        {
            (2, "a"), (1, "b"), (2, "c"), (1, "d")
        };

        Ordenamiento.MergeSort(datos, (x, y) => x.Clave.CompareTo(y.Clave));

        Assert.Equal(new[] { "b", "d", "a", "c" }, new[] { datos[0].Etiqueta, datos[1].Etiqueta, datos[2].Etiqueta, datos[3].Etiqueta });
    }

    [Fact]
    public void MergeSort_ConMilElementosAleatorios_QuedaOrdenadoYConservaLosElementos()
    {
        var azar = new Random(42);
        int[] datos = new int[1000];
        long sumaOriginal = 0;

        for (int i = 0; i < datos.Length; i++)
        {
            datos[i] = azar.Next(0, 500); // rango corto para generar muchos duplicados
            sumaOriginal += datos[i];
        }

        Ordenamiento.MergeSort(datos, CompararEnteros);

        long sumaFinal = 0;
        for (int i = 0; i < datos.Length; i++)
        {
            sumaFinal += datos[i];
            if (i > 0)
                Assert.True(datos[i - 1] <= datos[i]);
        }

        Assert.Equal(sumaOriginal, sumaFinal);
    }

    [Fact]
    public void BusquedaBinaria_ArregloVacio_DevuelveMenosUno()
    {
        Assert.Equal(-1, Ordenamiento.BusquedaBinaria(Array.Empty<int>(), 1, CompararEnteros));
    }

    [Fact]
    public void BusquedaBinaria_ElementoExistenteEInexistente()
    {
        int[] datos = { 1, 3, 5, 7, 9 };

        Assert.Equal(0, Ordenamiento.BusquedaBinaria(datos, 1, CompararEnteros));
        Assert.Equal(2, Ordenamiento.BusquedaBinaria(datos, 5, CompararEnteros));
        Assert.Equal(4, Ordenamiento.BusquedaBinaria(datos, 9, CompararEnteros));
        Assert.Equal(-1, Ordenamiento.BusquedaBinaria(datos, 4, CompararEnteros));
        Assert.Equal(-1, Ordenamiento.BusquedaBinaria(datos, 100, CompararEnteros));
    }

    [Fact]
    public void BusquedaBinaria_ConDuplicados_DevuelveLaPrimeraPosicion()
    {
        int[] datos = { 1, 2, 2, 2, 3 };

        Assert.Equal(1, Ordenamiento.BusquedaBinaria(datos, 2, CompararEnteros));
    }
}
