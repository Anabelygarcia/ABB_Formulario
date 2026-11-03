using System;
using System.Collections.Generic;

namespace TareaABB
{
    // Árbol Binario de Búsqueda. Invariante: izquierdo < nodo < derecho (sin repetidos).
    public class ArbolABB
    {
        private Nodo raiz;
        private int cantidad;

        public Nodo Raiz { get { return raiz; } }
        public int Tamano { get { return cantidad; } }
        public bool Vacio { get { return raiz == null; } }

        public ArbolABB()
        {
            raiz = null;
            cantidad = 0;
        }

        // ------------------------------------------------------------ INSERTAR
        // Devuelve true si se insertó, false si el valor ya existía.
        public bool Insertar(int valor)
        {
            bool insertado = false;
            raiz = Insertar(raiz, valor, ref insertado);
            if (insertado) cantidad++;
            return insertado;
        }

        private Nodo Insertar(Nodo n, int valor, ref bool insertado)
        {
            if (n == null)
            {
                insertado = true;
                return new Nodo(valor);          // se reserva memoria para el nuevo nodo
            }
            if (valor < n.Dato) n.Izq = Insertar(n.Izq, valor, ref insertado);
            else if (valor > n.Dato) n.Der = Insertar(n.Der, valor, ref insertado);
            // si es igual: duplicado, no se inserta
            return n;
        }

        // ------------------------------------------------------------- BUSCAR
        public bool Buscar(int valor)
        {
            List<int> camino = new List<int>();
            return Buscar(valor, camino);
        }

        // Busca y devuelve en "camino" las claves visitadas desde la raíz.
        public bool Buscar(int valor, List<int> camino)
        {
            Nodo n = raiz;
            while (n != null)
            {
                camino.Add(n.Dato);
                if (valor == n.Dato) return true;
                n = (valor < n.Dato) ? n.Izq : n.Der;
            }
            return false;
        }

        // ----------------------------------------------------------- ELIMINAR
        // Devuelve true si se eliminó, false si el valor no existía.
        public bool Eliminar(int valor)
        {
            bool eliminado = false;
            raiz = Eliminar(raiz, valor, ref eliminado);
            if (eliminado) cantidad--;
            return eliminado;
        }

        private Nodo Eliminar(Nodo n, int valor, ref bool eliminado)
        {
            if (n == null) return null;

            if (valor < n.Dato)
            {
                n.Izq = Eliminar(n.Izq, valor, ref eliminado);
            }
            else if (valor > n.Dato)
            {
                n.Der = Eliminar(n.Der, valor, ref eliminado);
            }
            else
            {
                eliminado = true;
                if (n.Izq == null) return n.Der;      // caso 1 (hoja) y caso 2 (solo hijo derecho)
                if (n.Der == null) return n.Izq;      // caso 2 (solo hijo izquierdo)

                // caso 3: dos hijos -> se reemplaza por el sucesor inorden
                Nodo suc = n.Der;
                while (suc.Izq != null) suc = suc.Izq;
                n.Dato = suc.Dato;
                bool aux = false;
                n.Der = Eliminar(n.Der, suc.Dato, ref aux);
            }
            return n;
        }

        // ---------------------------------------------------------- RECORRIDOS
        public List<int> Preorden()
        {
            List<int> v = new List<int>();
            Preorden(raiz, v);
            return v;
        }

        private void Preorden(Nodo n, List<int> v)
        {
            if (n == null) return;
            v.Add(n.Dato);          // RAÍZ
            Preorden(n.Izq, v);     // izquierda
            Preorden(n.Der, v);     // derecha
        }

        public List<int> Inorden()
        {
            List<int> v = new List<int>();
            Inorden(raiz, v);
            return v;
        }

        private void Inorden(Nodo n, List<int> v)
        {
            if (n == null) return;
            Inorden(n.Izq, v);      // izquierda
            v.Add(n.Dato);          // RAÍZ
            Inorden(n.Der, v);      // derecha
        }

        public List<int> Postorden()
        {
            List<int> v = new List<int>();
            Postorden(raiz, v);
            return v;
        }

        private void Postorden(Nodo n, List<int> v)
        {
            if (n == null) return;
            Postorden(n.Izq, v);    // izquierda
            Postorden(n.Der, v);    // derecha
            v.Add(n.Dato);          // RAÍZ
        }

        // ----------------------------------------------------------- UTILIDADES
        public int Altura()
        {
            return Altura(raiz);
        }

        private int Altura(Nodo n)
        {
            if (n == null) return -1;                 // vacío = -1, hoja = 0
            return 1 + Math.Max(Altura(n.Izq), Altura(n.Der));
        }

        public int Minimo()
        {
            Nodo n = raiz;
            while (n.Izq != null) n = n.Izq;          // el árbol no debe estar vacío
            return n.Dato;
        }

        public int Maximo()
        {
            Nodo n = raiz;
            while (n.Der != null) n = n.Der;          // el árbol no debe estar vacío
            return n.Dato;
        }

        // Comprueba que todo el árbol cumple izquierdo < nodo < derecho.
        public bool EsABBValido()
        {
            return EsValido(raiz, null, null);
        }

        private bool EsValido(Nodo n, int? min, int? max)
        {
            if (n == null) return true;
            if (min.HasValue && n.Dato <= min.Value) return false;
            if (max.HasValue && n.Dato >= max.Value) return false;
            return EsValido(n.Izq, min, n.Dato) && EsValido(n.Der, n.Dato, max);
        }

        // Copia profunda del árbol.
        public ArbolABB Clonar()
        {
            ArbolABB copia = new ArbolABB();
            copia.raiz = Copiar(raiz);
            copia.cantidad = cantidad;
            return copia;
        }

        private Nodo Copiar(Nodo n)
        {
            if (n == null) return null;
            Nodo c = new Nodo(n.Dato);
            c.Izq = Copiar(n.Izq);
            c.Der = Copiar(n.Der);
            return c;
        }
    }
}
