namespace ConviTEC.Core.Estructuras;

//
// Cola FIFO (primero en entrar, primero en salir) implementada con un arreglo circular.
// Al desencolar no se desplazan elementos: solo avanza el índice del frente, así que
// Encolar y Desencolar cuestan 
public class Cola<T>
{
    private const int CapacidadInicial = 4;

    private T[] _elementos;
    private int _frente; 

    public int Cantidad { get; private set; }

    public bool EstaVacia => Cantidad == 0;

    public Cola(int capacidadInicial = CapacidadInicial)
    {
        if (capacidadInicial < 1)
            throw new ArgumentOutOfRangeException(nameof(capacidadInicial), "La capacidad inicial debe ser al menos 1.");

        _elementos = new T[capacidadInicial];
    }

    public void Encolar(T elemento)
    {
        if (Cantidad == _elementos.Length)
            Redimensionar();

        // El % hace que al llegar al final del arreglo se continúe desde el inicio (circular).
        int posicion = (_frente + Cantidad) % _elementos.Length;
        _elementos[posicion] = elemento;
        Cantidad++;
    }

    public T Desencolar()
    {
        if (EstaVacia)
            throw new InvalidOperationException("La cola está vacía.");

        T elemento = _elementos[_frente];
        _elementos[_frente] = default!;
        _frente = (_frente + 1) % _elementos.Length;
        Cantidad--;
        return elemento;
    }

    //Devuelve el primer elemento sin sacarlo.
    public T VerFrente()
    {
        if (EstaVacia)
            throw new InvalidOperationException("La cola está vacía.");

        return _elementos[_frente];
    }

    public void Limpiar()
    {
        for (int i = 0; i < Cantidad; i++)
            _elementos[(_frente + i) % _elementos.Length] = default!;

        _frente = 0;
        Cantidad = 0;
    }

    //Devuelve los elementos en orden de salida (el primero en salir va en la posición 0).
    public T[] ToArray()
    {
        T[] copia = new T[Cantidad];

        for (int i = 0; i < Cantidad; i++)
            copia[i] = _elementos[(_frente + i) % _elementos.Length];

        return copia;
    }

    private void Redimensionar()
    {
        T[] nuevo = new T[_elementos.Length * 2];

        // Se copian en orden lógico y el frente vuelve a la posición 0.
        for (int i = 0; i < Cantidad; i++)
            nuevo[i] = _elementos[(_frente + i) % _elementos.Length];

        _elementos = nuevo;
        _frente = 0;
    }
}
