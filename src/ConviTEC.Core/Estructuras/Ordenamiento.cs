namespace ConviTEC.Core.Estructuras;

// Algoritmos propios de ordenamiento y búsqueda sobre arreglos nativos.

public static class Ordenamiento
{
    /// Ordena el arreglo (en el mismo arreglo) con MergeSort.

    public static void MergeSort<T>(T[] arreglo, Comparison<T> comparar)
    {
        ArgumentNullException.ThrowIfNull(arreglo);
        ArgumentNullException.ThrowIfNull(comparar);

        if (arreglo.Length < 2)
            return; 

        T[] auxiliar = new T[arreglo.Length];
        MergeSort(arreglo, auxiliar, 0, arreglo.Length - 1, comparar);
    }

    private static void MergeSort<T>(T[] arreglo, T[] auxiliar, int izquierda, int derecha, Comparison<T> comparar)
    {
        if (izquierda >= derecha)
            return;

        int medio = izquierda + (derecha - izquierda) / 2; 

        MergeSort(arreglo, auxiliar, izquierda, medio, comparar);
        MergeSort(arreglo, auxiliar, medio + 1, derecha, comparar);
        Mezclar(arreglo, auxiliar, izquierda, medio, derecha, comparar);
    }

    private static void Mezclar<T>(T[] arreglo, T[] auxiliar, int izquierda, int medio, int derecha, Comparison<T> comparar)
    {
        for (int k = izquierda; k <= derecha; k++)
            auxiliar[k] = arreglo[k];

        int i = izquierda;   
        int j = medio + 1;   

        for (int k = izquierda; k <= derecha; k++)
        {
            if (i > medio)
                arreglo[k] = auxiliar[j++];          
            else if (j > derecha)
                arreglo[k] = auxiliar[i++];          
            else if (comparar(auxiliar[j], auxiliar[i]) < 0)
                arreglo[k] = auxiliar[j++];          
            else
                arreglo[k] = auxiliar[i++];          
        }
    }

    public static int BusquedaBinaria<T>(T[] arreglo, T objetivo, Comparison<T> comparar)
    {
        ArgumentNullException.ThrowIfNull(arreglo);
        ArgumentNullException.ThrowIfNull(comparar);

        int izquierda = 0;
        int derecha = arreglo.Length - 1;
        int resultado = -1;

        while (izquierda <= derecha)
        {
            int medio = izquierda + (derecha - izquierda) / 2;
            int comparacion = comparar(arreglo[medio], objetivo);

            if (comparacion == 0)
            {
                resultado = medio;
                derecha = medio - 1; 
            }
            else if (comparacion < 0)
            {
                izquierda = medio + 1;
            }
            else
            {
                derecha = medio - 1;
            }
        }

        return resultado;
    }
}
