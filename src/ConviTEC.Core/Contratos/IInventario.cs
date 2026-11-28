using ConviTEC.Core.Modelo;

namespace ConviTEC.Core.Contratos;

/// <summary>Define las operaciones para consultar y reservar existencias.</summary>
public interface IInventario
{
    Producto? BuscarProducto(string codigo);
    int Disponibilidad(string codigo);
    bool Reservar(string idPedido, string codigo, int cantidad);
    void Liberar(string idPedido);
    bool Confirmar(string idPedido);
}
