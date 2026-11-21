using ConviTEC.Core.Contratos;

namespace ConviTEC.Core.Persistencia;

public enum EstadoCarga
{
    Cargado,
    SinArchivo
}

public record ResultadoCarga(string Archivo, EstadoCarga Estado);

/// <summary>
/// Coordina la persistencia de todos los módulos de una sede:
/// carga y valida al iniciar, y guarda tras cada cambio.
/// </summary>
public class ServicioPersistencia
{
    private readonly GestorArchivos _gestor;
    private readonly object _candado = new();
    private IPersistible[] _modulos = new IPersistible[4];
    private int _cantidad;

    public ServicioPersistencia(GestorArchivos gestor)
    {
        ArgumentNullException.ThrowIfNull(gestor);
        _gestor = gestor;
    }

    public int Cantidad
    {
        get { lock (_candado) return _cantidad; }
    }

    /// <summary>
    /// Registra un módulo. El orden de registro es el orden de carga:
    /// primero los módulos de los que dependen otros.
    /// </summary>
    public void Registrar(IPersistible modulo)
    {
        ArgumentNullException.ThrowIfNull(modulo);

        if (string.IsNullOrWhiteSpace(modulo.NombreArchivo))
            throw new ArgumentException("El módulo no define un nombre de archivo.", nameof(modulo));

        lock (_candado)
        {
            for (int i = 0; i < _cantidad; i++)
            {
                if (string.Equals(_modulos[i].NombreArchivo, modulo.NombreArchivo, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException(
                        $"Ya hay un módulo registrado con el archivo '{modulo.NombreArchivo}'.", nameof(modulo));
            }

            if (_cantidad == _modulos.Length)
            {
                var ampliado = new IPersistible[_modulos.Length * 2];
                Array.Copy(_modulos, ampliado, _cantidad);
                _modulos = ampliado;
            }

            _modulos[_cantidad++] = modulo;
        }
    }

    /// <summary>
    /// Carga todos los módulos en orden de registro y luego valida la consistencia.
    /// Lanza InvalidDataException si un archivo es inválido o si falla una validación.
    /// Nunca sobrescribe los archivos.
    /// </summary>
    public ResultadoCarga[] CargarTodo()
    {
        IPersistible[] modulos = CopiaDeModulos();
        var resultados = new ResultadoCarga[modulos.Length];

        for (int i = 0; i < modulos.Length; i++)
        {
            bool cargado = _gestor.Cargar(modulos[i]);
            resultados[i] = new ResultadoCarga(
                modulos[i].NombreArchivo,
                cargado ? EstadoCarga.Cargado : EstadoCarga.SinArchivo);
        }

        // La validación va después de cargar todo, para poder revisar referencias entre módulos.
        for (int i = 0; i < modulos.Length; i++)
        {
            if (modulos[i] is not IValidable validable)
                continue;

            try
            {
                validable.Validar();
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(
                    $"Inconsistencia en los datos de '{modulos[i].NombreArchivo}': {ex.Message}", ex);
            }
        }

        return resultados;
    }

    /// <summary>Guarda un módulo. Se llama tras cada cambio en sus datos.</summary>
    public void Guardar(IPersistible modulo)
    {
        ArgumentNullException.ThrowIfNull(modulo);

        if (!EstaRegistrado(modulo))
            throw new InvalidOperationException(
                $"El módulo '{modulo.NombreArchivo}' no está registrado en el servicio de persistencia: " +
                "sus datos no se recuperarían al reiniciar.");

        _gestor.Guardar(modulo);
    }

    /// <summary>Guarda todos los módulos registrados (por ejemplo, al apagar la sede).</summary>
    public void GuardarTodo()
    {
        IPersistible[] modulos = CopiaDeModulos();
        for (int i = 0; i < modulos.Length; i++)
            _gestor.Guardar(modulos[i]);
    }

    private bool EstaRegistrado(IPersistible modulo)
    {
        lock (_candado)
        {
            for (int i = 0; i < _cantidad; i++)
            {
                if (ReferenceEquals(_modulos[i], modulo))
                    return true;
            }
            return false;
        }
    }

    private IPersistible[] CopiaDeModulos()
    {
        lock (_candado)
        {
            var copia = new IPersistible[_cantidad];
            Array.Copy(_modulos, copia, _cantidad);
            return copia;
        }
    }
}