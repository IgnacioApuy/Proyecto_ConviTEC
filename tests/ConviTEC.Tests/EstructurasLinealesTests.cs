using ConviTEC.Core.Estructuras;
using Xunit;

namespace ConviTEC.Tests;

public class ListaTests
{
    [Fact]
    public void ListaNueva_EstaVacia()
    {
        var lista = new Lista<int>();

        Assert.True(lista.EstaVacia);
        Assert.Equal(0, lista.Cantidad);
        Assert.Empty(lista.ToArray());
    }

    [Fact]
    public void Agregar_MasAllaDeLaCapacidadInicial_ConservaTodosLosElementos()
    {
        var lista = new Lista<int>();

        for (int i = 0; i < 100; i++)
            lista.Agregar(i);

        Assert.Equal(100, lista.Cantidad);
        Assert.Equal(0, lista[0]);
        Assert.Equal(99, lista[99]);
    }

    [Fact]
    public void Indexador_ConIndiceInvalido_LanzaExcepcion()
    {
        var lista = new Lista<int>();
        lista.Agregar(1);

        Assert.Throws<ArgumentOutOfRangeException>(() => { _ = lista[-1]; });
        Assert.Throws<ArgumentOutOfRangeException>(() => { _ = lista[1]; });
    }

    [Fact]
    public void Insertar_AlInicioEnMedioYAlFinal_MantieneElOrden()
    {
        var lista = new Lista<int>();
        lista.Agregar(2);
        lista.Agregar(4);

        lista.Insertar(0, 1);
        lista.Insertar(2, 3);
        lista.Insertar(4, 5);

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, lista.ToArray());
    }

    [Fact]
    public void Insertar_FueraDeRango_LanzaExcepcion()
    {
        var lista = new Lista<int>();

        Assert.Throws<ArgumentOutOfRangeException>(() => lista.Insertar(1, 10));
    }

    [Fact]
    public void IndiceDe_ElementoInexistente_DevuelveMenosUno()
    {
        var lista = new Lista<string>();
        lista.Agregar("a");

        Assert.Equal(-1, lista.IndiceDe("z"));
        Assert.False(lista.Contiene("z"));
    }

    [Fact]
    public void Eliminar_ElementoInexistente_DevuelveFalseYNoCambiaLaLista()
    {
        var lista = new Lista<int>();
        lista.Agregar(1);

        Assert.False(lista.Eliminar(99));
        Assert.Equal(1, lista.Cantidad);
    }

    [Fact]
    public void Eliminar_ConDuplicados_QuitaSoloLaPrimeraCoincidencia()
    {
        var lista = new Lista<int>();
        lista.Agregar(7);
        lista.Agregar(5);
        lista.Agregar(7);

        Assert.True(lista.Eliminar(7));

        Assert.Equal(new[] { 5, 7 }, lista.ToArray());
    }

    [Fact]
    public void EliminarEn_DesplazaLosElementosSiguientes()
    {
        var lista = new Lista<int>();
        for (int i = 1; i <= 4; i++)
            lista.Agregar(i);

        int eliminado = lista.EliminarEn(1);

        Assert.Equal(2, eliminado);
        Assert.Equal(new[] { 1, 3, 4 }, lista.ToArray());
    }

    [Fact]
    public void EliminarEn_ListaVacia_LanzaExcepcion()
    {
        var lista = new Lista<int>();

        Assert.Throws<ArgumentOutOfRangeException>(() => lista.EliminarEn(0));
    }

    [Fact]
    public void Limpiar_DejaLaListaVaciaYReutilizable()
    {
        var lista = new Lista<int>();
        lista.Agregar(1);
        lista.Agregar(2);

        lista.Limpiar();
        lista.Agregar(9);

        Assert.Equal(new[] { 9 }, lista.ToArray());
    }

    [Fact]
    public void Foreach_RecorreSoloLosElementosReales()
    {
        var lista = new Lista<int>();
        lista.Agregar(1);
        lista.Agregar(2);
        lista.Agregar(3);

        int suma = 0;
        foreach (int valor in lista)
            suma += valor;

        Assert.Equal(6, suma);
    }
}

public class ColaTests
{
    [Fact]
    public void Desencolar_RespetaElOrdenFifo()
    {
        var cola = new Cola<int>();
        cola.Encolar(1);
        cola.Encolar(2);
        cola.Encolar(3);

        Assert.Equal(1, cola.Desencolar());
        Assert.Equal(2, cola.Desencolar());
        Assert.Equal(3, cola.Desencolar());
        Assert.True(cola.EstaVacia);
    }

    [Fact]
    public void ColaVacia_DesencolarYVerFrente_LanzanExcepcion()
    {
        var cola = new Cola<int>();

        Assert.Throws<InvalidOperationException>(() => cola.Desencolar());
        Assert.Throws<InvalidOperationException>(() => cola.VerFrente());
    }

    [Fact]
    public void VerFrente_NoRemueveElElemento()
    {
        var cola = new Cola<string>();
        cola.Encolar("primero");

        Assert.Equal("primero", cola.VerFrente());
        Assert.Equal(1, cola.Cantidad);
    }

    [Fact]
    public void ArregloCircular_ConservaElOrdenAlDarLaVueltaYAlRedimensionar()
    {
        var cola = new Cola<int>(4);

        for (int i = 1; i <= 4; i++)
            cola.Encolar(i);

        cola.Desencolar(); // sale el 1
        cola.Desencolar(); // sale el 2
        cola.Encolar(5);   // ocupa la posición 0 del arreglo (da la vuelta)
        cola.Encolar(6);   // ocupa la posición 1
        cola.Encolar(7);   // arreglo lleno: fuerza redimensionar con el frente "en medio"

        Assert.Equal(new[] { 3, 4, 5, 6, 7 }, cola.ToArray());
        Assert.Equal(3, cola.Desencolar());
    }

    [Fact]
    public void Limpiar_DejaLaColaUtilizable()
    {
        var cola = new Cola<int>();
        cola.Encolar(1);
        cola.Encolar(2);

        cola.Limpiar();
        cola.Encolar(8);

        Assert.Equal(new[] { 8 }, cola.ToArray());
    }
}

public class PilaTests
{
    [Fact]
    public void Desapilar_RespetaElOrdenLifo()
    {
        var pila = new Pila<int>();
        pila.Apilar(1);
        pila.Apilar(2);
        pila.Apilar(3);

        Assert.Equal(3, pila.Desapilar());
        Assert.Equal(2, pila.Desapilar());
        Assert.Equal(1, pila.Desapilar());
        Assert.True(pila.EstaVacia);
    }

    [Fact]
    public void PilaVacia_DesapilarYVerCima_LanzanExcepcion()
    {
        var pila = new Pila<int>();

        Assert.Throws<InvalidOperationException>(() => pila.Desapilar());
        Assert.Throws<InvalidOperationException>(() => pila.VerCima());
    }

    [Fact]
    public void VerCima_NoRemueveElElemento()
    {
        var pila = new Pila<int>();
        pila.Apilar(4);

        Assert.Equal(4, pila.VerCima());
        Assert.Equal(1, pila.Cantidad);
    }

    [Fact]
    public void Apilar_MasAllaDeLaCapacidadInicial_ConservaElOrden()
    {
        var pila = new Pila<int>();

        for (int i = 0; i < 50; i++)
            pila.Apilar(i);

        Assert.Equal(50, pila.Cantidad);
        Assert.Equal(49, pila.VerCima());
        Assert.Equal(0, pila.ToArray()[0]); // la base queda en la posición 0
    }
}
