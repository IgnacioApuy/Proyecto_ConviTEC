using ConviTEC.Core.Modelo;
using Xunit;

namespace ConviTEC.Tests;

public class ModeloTests
{
    //Producto

    [Fact]
    public void Producto_ConDatosValidos_GuardaLosDatosSinEspaciosSobrantes()
    {
        var p = new Producto("  P001 ", " Leche ", "Leche entera 1L", "Lácteos", 1200m);

        Assert.Equal("P001", p.Codigo);
        Assert.Equal("Leche", p.Nombre);
        Assert.Equal("Leche entera 1L", p.Descripcion);
        Assert.Equal("Lácteos", p.Categoria);
        Assert.Equal(1200m, p.Precio);
    }

    [Fact]
    public void Producto_DescripcionNulaOVacia_SeAcepta()
    {
        Assert.Equal("", new Producto("P1", "A", null!, "Cat", 1m).Descripcion);
        Assert.Equal("", new Producto("P1", "A", "", "Cat", 1m).Descripcion);
    }

    [Fact]
    public void Producto_PrecioCero_SeAcepta()
    {
        Assert.Equal(0m, new Producto("P1", "Muestra", "", "Cat", 0m).Precio);
    }

    [Fact]
    public void Producto_CodigoNombreOCategoriaVacios_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Producto("", "A", "", "Cat", 1m));
        Assert.Throws<ArgumentException>(() => new Producto("   ", "A", "", "Cat", 1m));
        Assert.Throws<ArgumentException>(() => new Producto(null!, "A", "", "Cat", 1m));
        Assert.Throws<ArgumentException>(() => new Producto("P1", "", "", "Cat", 1m));
        Assert.Throws<ArgumentException>(() => new Producto("P1", "A", "", " ", 1m));
    }

    [Fact]
    public void Producto_PrecioNegativo_LanzaArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Producto("P1", "A", "", "Cat", -1m));
    }

    // ----------------------------------------------------------- Sede

    [Fact]
    public void Sede_ConDatosValidos_GuardaLosDatos()
    {
        var s = new Sede("ALA", "Alajuela", "Alajuela");

        Assert.Equal("ALA", s.Codigo);
        Assert.Equal("Alajuela", s.Nombre);
        Assert.Equal("Alajuela", s.Ubicacion);
    }

    [Fact]
    public void Sede_ConDatosVacios_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Sede("", "Alajuela", "Alajuela"));
        Assert.Throws<ArgumentException>(() => new Sede("ALA", " ", "Alajuela"));
        Assert.Throws<ArgumentException>(() => new Sede("ALA", "Alajuela", null!));
    }
}
