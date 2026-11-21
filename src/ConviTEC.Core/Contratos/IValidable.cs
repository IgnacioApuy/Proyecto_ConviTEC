namespace ConviTEC.Core.Contratos;

/// <summary>
/// Opcional. Un módulo persistible que implemente esta interfaz es validado al iniciar,
/// después de que TODOS los módulos de la sede fueron cargados. Sirve para revisar
/// referencias cruzadas (ej. que un pedido apunte a productos que existen).
/// Debe lanzar una excepción con un mensaje claro si encuentra una inconsistencia.
/// </summary>
public interface IValidable
{
    void Validar();
}