namespace TareaABB
{
    // Un elemento del árbol. Cada nodo se crea en memoria dinámica con "new".
    public class Nodo
    {
        public int Dato;
        public Nodo Izq;   // subárbol izquierdo (claves menores)
        public Nodo Der;   // subárbol derecho   (claves mayores)

        public Nodo(int dato)
        {
            Dato = dato;
            Izq = null;
            Der = null;
        }
    }
}
