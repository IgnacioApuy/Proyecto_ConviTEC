using System.Collections;

namespace ConviTEC.Core.Estructuras;

//Lista dinámica genérica implementada sobre un arreglo nativo.
// Cuando el arreglo se llena, se crea uno del doble de tamaño y se copian los elementos
public class Lista<T> : IEnumerable<T>
{
    private const int CapacidadInicial = 4;

    private T[] _elementos;

    //Cantidad de elementos que hay realmente (no el tamaño del arreglo interno)
    public int Cantidad { get; private set; }

    public bool EstaVacia => Cantidad == 0;

    public Lista(int capacidadInicial = CapacidadInicial)
    {
        if (capacidadInicial < 1)
            throw new ArgumentOutOfRangeException(nameof(capacidadInicial), "La capacidad inicial debe ser al menos 1.");

        _elementos = new T[capacidadInicial];
    }

    //Acceso por posición, igual que en un arreglo.
    public T this[int indice]
    {
        get
        {
            ValidarIndice(indice);
            return _elementos[indice];
        }
        set
        {
            ValidarIndice(indice);
            _elementos[indice] = value;
        }
    }

    //Agrega al final. O(1) amortizado.
    public void Agregar(T elemento)
    {
        AsegurarCapacidad(Cantidad + 1);
        _elementos[Cantidad] = elemento;
        Cantidad++;
    }

    //Inserta en una posición, desplazando a la derecha los elementos siguientes.
    public void Insertar(int indice, T elemento)
    {
        // Se permite indice == Cantidad (equivale a agregar al final).
        if (indice < 0 || indice > Cantidad)
            throw new ArgumentOutOfRangeException(nameof(indice), "Índice fuera de rango.");

        AsegurarCapacidad(Cantidad + 1);

        for (int i = Cantidad; i > indice; i--)
            _elementos[i] = _elementos[i - 1];

        _elementos[indice] = elemento;
        Cantidad++;
    }

    //Devuelve la posición de la primera coincidencia, o -1 si no existe.
    public int IndiceDe(T elemento)
    {
        EqualityComparer<T> comparador = EqualityComparer<T>.Default;

        for (int i = 0; i < Cantidad; i++)
        {
            if (comparador.Equals(_elementos[i], elemento))
                return i;
        }

        return -1;
    }

    public bool Contiene(T elemento) => IndiceDe(elemento) >= 0;

    //Elimina el elemento de una posición y devuelve su valor. 
    public T EliminarEn(int indice)
    {
        ValidarIndice(indice);

        T eliminado = _elementos[indice];

        for (int i = indice; i < Cantidad - 1; i++)
            _elementos[i] = _elementos[i + 1];

        Cantidad--;
        _elementos[Cantidad] = default!; 
        return eliminado;
    }

    //Elimina la primera coincidencia. Devuelve false si el elemento no existe
    public bool Eliminar(T elemento)
    {
        int indice = IndiceDe(elemento);
        if (indice < 0)
            return false;

        EliminarEn(indice);
        return true;
    }

    public void Limpiar()
    {
        for (int i = 0; i < Cantidad; i++)
            _elementos[i] = default!;

        Cantidad = 0;
    }

    //Copia los elementos a un arreglo nativo del tamaño exacto (útil para ordenar y serializar).
    public T[] ToArray()
    {
        T[] copia = new T[Cantidad];

        for (int i = 0; i < Cantidad; i++)
            copia[i] = _elementos[i];

        return copia;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Cantidad; i++)
            yield return _elementos[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void ValidarIndice(int indice)
    {
        if (indice < 0 || indice >= Cantidad)
            throw new ArgumentOutOfRangeException(nameof(indice), "Índice fuera de rango.");
    }

    private void AsegurarCapacidad(int capacidadMinima)
    {
        if (capacidadMinima <= _elementos.Length)
            return;

        int nuevaCapacidad = _elementos.Length * 2;
        if (nuevaCapacidad < capacidadMinima)
            nuevaCapacidad = capacidadMinima;

        T[] nuevo = new T[nuevaCapacidad];

        for (int i = 0; i < Cantidad; i++)
            nuevo[i] = _elementos[i];

        _elementos = nuevo;
    }
}
