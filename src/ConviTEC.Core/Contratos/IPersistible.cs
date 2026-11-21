namespace ConviTEC.Core.Contratos;

/// <summary>
/// Contrato de persistencia. Todo módulo que deba sobrevivir al reinicio
/// (inventario, pedidos, ventas, colas, etc.) lo implementa.
/// </summary>
public interface IPersistible
{
    /// <summary>Nombre del archivo JSON en la carpeta de datos de la sede (ej. "inventario.json").</summary>
    string NombreArchivo { get; }

    /// <summary>Convierte el estado actual del módulo a texto JSON.</summary>
    string Serializar();

    /// <summary>
    /// Reconstruye el estado del módulo desde JSON, validando consistencia.
    /// Debe lanzar una excepción clara si el contenido es inválido.
    /// </summary>
    void Reconstruir(string json);
}