using System.Diagnostics.CodeAnalysis;

namespace ConviTEC.Core.Estructuras;

// Árbol AVL: árbol binario de búsqueda que se auto-balancea.

public class ArbolAVL<TClave, TValor> where TClave : IComparable<TClave>
{
    private class Nodo
    {
        public TClave Clave;
        public TValor Valor;
        public int Altura = 1; // un nodo hoja tiene altura 1
        public Nodo? Izquierdo;
        public Nodo? Derecho;

        public Nodo(TClave clave, TValor valor)
        {
            Clave = clave;
            Valor = valor;
        }
    }

    private Nodo? _raiz;

    public int Cantidad { get; private set; }

    public bool EstaVacio => _raiz == null;


    public int Altura => AlturaDe(_raiz);

    /// Inserta una clave nueva. Devuelve false (sin modificar nada) si la clave ya existe.
  
    public bool Insertar(TClave clave, TValor valor)
    {
        ValidarClave(clave);

        bool insertado = false;
        _raiz = Insertar(_raiz, clave, valor, ref insertado);

        if (insertado)
            Cantidad++;

        return insertado;
    }

    private static Nodo Insertar(Nodo? nodo, TClave clave, TValor valor, ref bool insertado)
    {
        if (nodo == null)
        {
            insertado = true;
            return new Nodo(clave, valor);
        }

        int comparacion = clave.CompareTo(nodo.Clave);

        if (comparacion < 0)
            nodo.Izquierdo = Insertar(nodo.Izquierdo, clave, valor, ref insertado);
        else if (comparacion > 0)
            nodo.Derecho = Insertar(nodo.Derecho, clave, valor, ref insertado);
        else
            return nodo; // clave duplicada: no se inserta y no hay nada que rebalancear

        // Al regresar de la recursión se recalcula la altura y se rebalancea cada ancestro.
        return Rebalancear(nodo);
    }

    public bool Buscar(TClave clave, [MaybeNullWhen(false)] out TValor valor)
    {
        ValidarClave(clave);

        Nodo? nodo = BuscarNodo(clave);
        if (nodo == null)
        {
            valor = default!;
            return false;
        }

        valor = nodo.Valor;
        return true;
    }

    public bool Contiene(TClave clave)
    {
        ValidarClave(clave);
        return BuscarNodo(clave) != null;
    }

    //Reemplaza el valor de una clave existente. Devuelve false si la clave no existe.
    public bool Actualizar(TClave clave, TValor nuevoValor)
    {
        ValidarClave(clave);

        Nodo? nodo = BuscarNodo(clave);
        if (nodo == null)
            return false;

        nodo.Valor = nuevoValor;
        return true;
    }

    private Nodo? BuscarNodo(TClave clave)
    {
        Nodo? actual = _raiz;

        // Búsqueda iterativa: descartamos la mitad del árbol (aprox.) en cada paso.
        while (actual != null)
        {
            int comparacion = clave.CompareTo(actual.Clave);

            if (comparacion == 0)
                return actual;

            actual = comparacion < 0 ? actual.Izquierdo : actual.Derecho;
        }

        return null;
    }


    public bool Eliminar(TClave clave)
    {
        ValidarClave(clave);

        bool eliminado = false;
        _raiz = Eliminar(_raiz, clave, ref eliminado);

        if (eliminado)
            Cantidad--;

        return eliminado;
    }

    private static Nodo? Eliminar(Nodo? nodo, TClave clave, ref bool eliminado)
    {
        if (nodo == null)
            return null; // la clave no está en el árbol

        int comparacion = clave.CompareTo(nodo.Clave);

        if (comparacion < 0)
        {
            nodo.Izquierdo = Eliminar(nodo.Izquierdo, clave, ref eliminado);
        }
        else if (comparacion > 0)
        {
            nodo.Derecho = Eliminar(nodo.Derecho, clave, ref eliminado);
        }
        else
        {
            eliminado = true;

            if (nodo.Izquierdo == null)
                return nodo.Derecho;
            if (nodo.Derecho == null)
                return nodo.Izquierdo;

            Nodo sucesor = MenorNodo(nodo.Derecho);
            nodo.Clave = sucesor.Clave;
            nodo.Valor = sucesor.Valor;

            bool eliminadoAuxiliar = false;
            nodo.Derecho = Eliminar(nodo.Derecho, sucesor.Clave, ref eliminadoAuxiliar);
        }

        return Rebalancear(nodo);
    }

    private static Nodo MenorNodo(Nodo nodo)
    {
        while (nodo.Izquierdo != null)
            nodo = nodo.Izquierdo;

        return nodo;
    }

   
    public void RecorrerInOrden(Action<TClave, TValor> visitar)
    {
        ArgumentNullException.ThrowIfNull(visitar);
        RecorrerInOrden(_raiz, visitar);
    }

    private static void RecorrerInOrden(Nodo? nodo, Action<TClave, TValor> visitar)
    {
        if (nodo == null)
            return;

        RecorrerInOrden(nodo.Izquierdo, visitar);
        visitar(nodo.Clave, nodo.Valor);
        RecorrerInOrden(nodo.Derecho, visitar);
    }

    //Valores ordenados por clave ascendente, en un arreglo nativo.
    public TValor[] ObtenerValoresInOrden()
    {
        TValor[] resultado = new TValor[Cantidad];
        int indice = 0;
        LlenarValores(_raiz, resultado, ref indice);
        return resultado;
    }

    //Claves en orden ascendente, en un arreglo nativo.
    public TClave[] ObtenerClavesInOrden()
    {
        TClave[] resultado = new TClave[Cantidad];
        int indice = 0;
        LlenarClaves(_raiz, resultado, ref indice);
        return resultado;
    }

    private static void LlenarValores(Nodo? nodo, TValor[] destino, ref int indice)
    {
        if (nodo == null)
            return;

        LlenarValores(nodo.Izquierdo, destino, ref indice);
        destino[indice++] = nodo.Valor;
        LlenarValores(nodo.Derecho, destino, ref indice);
    }

    private static void LlenarClaves(Nodo? nodo, TClave[] destino, ref int indice)
    {
        if (nodo == null)
            return;

        LlenarClaves(nodo.Izquierdo, destino, ref indice);
        destino[indice++] = nodo.Clave;
        LlenarClaves(nodo.Derecho, destino, ref indice);
    }


    private static int AlturaDe(Nodo? nodo) => nodo == null ? 0 : nodo.Altura;

    private static void ActualizarAltura(Nodo nodo)
    {
        int alturaIzquierda = AlturaDe(nodo.Izquierdo);
        int alturaDerecha = AlturaDe(nodo.Derecho);
        nodo.Altura = 1 + (alturaIzquierda > alturaDerecha ? alturaIzquierda : alturaDerecha);
    }

    private static int FactorBalance(Nodo nodo) => AlturaDe(nodo.Izquierdo) - AlturaDe(nodo.Derecho);

    private static Nodo Rebalancear(Nodo nodo)
    {
        ActualizarAltura(nodo);
        int balance = FactorBalance(nodo);

        if (balance > 1) 
        {
        
            if (FactorBalance(nodo.Izquierdo!) < 0)
                nodo.Izquierdo = RotarIzquierda(nodo.Izquierdo!);

            return RotarDerecha(nodo);
        }

        if (balance < -1) // demasiado pesado a la derecha
        {
            // Caso derecha-izquierda: primero se rota el hijo derecho hacia la derecha.
            if (FactorBalance(nodo.Derecho!) > 0)
                nodo.Derecho = RotarDerecha(nodo.Derecho!);

            return RotarIzquierda(nodo);
        }

        return nodo;
    }

   
    private static Nodo RotarDerecha(Nodo y)
    {
        Nodo x = y.Izquierdo!;
        Nodo? b = x.Derecho;

        x.Derecho = y;
        y.Izquierdo = b;

        // Primero y (quedó abajo) y luego x (quedó arriba), porque la altura de x depende de la de y.
        ActualizarAltura(y);
        ActualizarAltura(x);
        return x;
    }

   
    private static Nodo RotarIzquierda(Nodo x)
    {
        Nodo y = x.Derecho!;
        Nodo? b = y.Izquierdo;

        y.Izquierdo = x;
        x.Derecho = b;

        ActualizarAltura(x);
        ActualizarAltura(y);
        return y;
    }

    
    public bool EsValido() => VerificarAltura(_raiz) >= 0;

    
    private static int VerificarAltura(Nodo? nodo)
    {
        if (nodo == null)
            return 0;

        int izquierda = VerificarAltura(nodo.Izquierdo);
        int derecha = VerificarAltura(nodo.Derecho);

        if (izquierda < 0 || derecha < 0)
            return -1;

        int diferencia = izquierda - derecha;
        if (diferencia > 1 || diferencia < -1)
            return -1;

        int alturaReal = 1 + (izquierda > derecha ? izquierda : derecha);
        return alturaReal == nodo.Altura ? alturaReal : -1;
    }

    private static void ValidarClave(TClave clave)
    {
        if (clave is null)
            throw new ArgumentNullException(nameof(clave), "La clave no puede ser nula.");
    }
}
