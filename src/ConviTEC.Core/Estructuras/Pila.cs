namespace ConviTEC.Core.Estructuras;
// Pila LIFO (último en entrar, primero en salir) implementada sobre un arreglo.
// La cima es la última posición ocupada, por lo que Apilar y Desapilar cuestan O(1).

public class Pila<T>
{
    private const int CapacidadInicial = 4;

    private T[] _elementos;

    public int Cantidad { get; private set; }

    public bool EstaVacia => Cantidad == 0;

    public Pila(int capacidadInicial = CapacidadInicial)
    {
        if (capacidadInicial < 1)
            throw new ArgumentOutOfRangeException(nameof(capacidadInicial), "La capacidad inicial debe ser al menos 1.");

        _elementos = new T[capacidadInicial];
    }

    public void Apilar(T elemento)
    {
        if (Cantidad == _elementos.Length)
            Redimensionar();

        _elementos[Cantidad] = elemento;
        Cantidad++;
    }

    public T Desapilar()
    {
        if (EstaVacia)
            throw new InvalidOperationException("La pila está vacía.");

        Cantidad--;
        T elemento = _elementos[Cantidad];
        _elementos[Cantidad] = default!;
        return elemento;
    }

    //Devuelve el elemento de la cima sin sacarlo.
    public T VerCima()
    {
        if (EstaVacia)
            throw new InvalidOperationException("La pila está vacía.");

        return _elementos[Cantidad - 1];
    }

    public void Limpiar()
    {
        for (int i = 0; i < Cantidad; i++)
            _elementos[i] = default!;

        Cantidad = 0;
    }

    //Devuelve los elementos desde la base hasta la cima (la cima queda al final).
    public T[] ToArray()
    {
        T[] copia = new T[Cantidad];

        for (int i = 0; i < Cantidad; i++)
            copia[i] = _elementos[i];

        return copia;
    }

    private void Redimensionar()
    {
        T[] nuevo = new T[_elementos.Length * 2];

        for (int i = 0; i < Cantidad; i++)
            nuevo[i] = _elementos[i];

        _elementos = nuevo;
    }
}
