using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TareaABB
{
    // Pruebas de integridad del ABB. Devuelve el reporte como texto.
    public static class Pruebas
    {
        private static int total;
        private static int aprobadas;
        private static StringBuilder sb;

        private static void Verificar(string nombre, bool ok, string detalle)
        {
            total++;
            if (ok) aprobadas++;
            sb.Append("  [" + (ok ? "PASS" : "FAIL") + "] " + nombre);
            if (!string.IsNullOrEmpty(detalle)) sb.Append("  ->  " + detalle);
            sb.AppendLine();
        }

        private static void Verificar(string nombre, bool ok)
        {
            Verificar(nombre, ok, "");
        }

        private static void Titulo(string t)
        {
            sb.AppendLine();
            sb.AppendLine("== " + t + " ==");
        }

        private static string Texto(List<int> v)
        {
            return "[" + string.Join(" ", v) + "]";
        }

        private static bool Igual(List<int> a, params int[] b)
        {
            return a.SequenceEqual(b);
        }

        public static string Ejecutar(out bool todoOk)
        {
            total = 0;
            aprobadas = 0;
            sb = new StringBuilder();
            sb.AppendLine("REPORTE DE PRUEBAS - ARBOL BINARIO DE BUSQUEDA (ABB)");
            sb.AppendLine("====================================================");

            int[] datos = new int[] { 50, 30, 70, 20, 40, 60, 80 };
            ArbolABB t = new ArbolABB();

            Titulo("1. Arbol vacio");
            Verificar("Vacio es verdadero", t.Vacio);
            Verificar("Tamano == 0", t.Tamano == 0);
            Verificar("Altura == -1", t.Altura() == -1);
            Verificar("Buscar(10) en arbol vacio es falso", !t.Buscar(10));
            Verificar("Eliminar(10) en arbol vacio es falso", !t.Eliminar(10));
            Verificar("recorridos vacios", t.Preorden().Count == 0 && t.Inorden().Count == 0 && t.Postorden().Count == 0);

            Titulo("2. Insercion");
            bool todas = true;
            foreach (int x in datos) todas = t.Insertar(x) && todas;
            Verificar("se insertan 7 claves distintas", todas, "50 30 70 20 40 60 80");
            Verificar("Tamano == 7", t.Tamano == 7);
            Verificar("Altura == 2", t.Altura() == 2);
            Verificar("Minimo == 20 y Maximo == 80", t.Minimo() == 20 && t.Maximo() == 80);
            Verificar("insertar duplicado (40) es rechazado", !t.Insertar(40));
            Verificar("Tamano sigue en 7 tras el duplicado", t.Tamano == 7);
            Verificar("se cumple la propiedad del ABB", t.EsABBValido());

            Titulo("3. Busqueda");
            bool halladas = true;
            foreach (int x in datos) halladas = t.Buscar(x) && halladas;
            Verificar("se encuentran las 7 claves insertadas", halladas);
            Verificar("Buscar(65) (no existe) es falso", !t.Buscar(65));
            Verificar("Buscar(0) (menor que todas) es falso", !t.Buscar(0));
            Verificar("Buscar(999) (mayor que todas) es falso", !t.Buscar(999));
            List<int> camino = new List<int>();
            t.Buscar(40, camino);
            Verificar("camino de busqueda de 40 = 50 -> 30 -> 40", Igual(camino, 50, 30, 40), Texto(camino));

            Titulo("4. Recorridos en profundidad (arbol de referencia)");
            List<int> pre = t.Preorden();
            List<int> ino = t.Inorden();
            List<int> pos = t.Postorden();
            sb.AppendLine("  Preorden : " + Texto(pre));
            sb.AppendLine("  Inorden  : " + Texto(ino));
            sb.AppendLine("  Postorden: " + Texto(pos));
            Verificar("Preorden  (raiz, izq, der) correcto", Igual(pre, 50, 30, 20, 40, 70, 60, 80));
            Verificar("Inorden   (izq, raiz, der) correcto", Igual(ino, 20, 30, 40, 50, 60, 70, 80));
            Verificar("Postorden (izq, der, raiz) correcto", Igual(pos, 20, 40, 30, 60, 80, 70, 50));
            Verificar("Inorden esta ordenado ascendentemente", ino.SequenceEqual(ino.OrderBy(v => v)));
            Verificar("los 3 recorridos visitan los mismos 7 nodos",
                pre.Count == 7 && ino.Count == 7 && pos.Count == 7 &&
                new SortedSet<int>(pre).SetEquals(ino) && new SortedSet<int>(pos).SetEquals(ino));
            Verificar("la raiz es el primero del preorden y el ultimo del postorden", pre[0] == 50 && pos[pos.Count - 1] == 50);

            Titulo("5. Eliminacion");
            ArbolABB e = t.Clonar();
            Verificar("la copia tiene los mismos recorridos", e.Preorden().SequenceEqual(pre) && e.Inorden().SequenceEqual(ino));

            Verificar("eliminar hoja (20)", e.Eliminar(20) && !e.Buscar(20));
            sb.AppendLine("    Inorden: " + Texto(e.Inorden()) + "  Preorden: " + Texto(e.Preorden()));
            Verificar("inorden tras borrar hoja", Igual(e.Inorden(), 30, 40, 50, 60, 70, 80));

            Verificar("eliminar nodo con un hijo (30, hijo 40)", e.Eliminar(30) && !e.Buscar(30));
            sb.AppendLine("    Inorden: " + Texto(e.Inorden()) + "  Preorden: " + Texto(e.Preorden()));
            Verificar("preorden tras borrar nodo de un hijo", Igual(e.Preorden(), 50, 40, 70, 60, 80));

            Verificar("eliminar nodo con dos hijos (70)", e.Eliminar(70) && !e.Buscar(70));
            sb.AppendLine("    Inorden: " + Texto(e.Inorden()) + "  Preorden: " + Texto(e.Preorden()));
            Verificar("preorden tras borrar nodo de dos hijos", Igual(e.Preorden(), 50, 40, 80, 60));

            Verificar("eliminar la raiz (50)", e.Eliminar(50) && !e.Buscar(50));
            sb.AppendLine("    Inorden: " + Texto(e.Inorden()) + "  Preorden: " + Texto(e.Preorden()));
            Verificar("inorden tras borrar la raiz", Igual(e.Inorden(), 40, 60, 80));

            Verificar("eliminar un valor inexistente (999) es falso", !e.Eliminar(999));
            Verificar("Tamano == 3 tras 4 eliminaciones", e.Tamano == 3);
            Verificar("el ABB sigue siendo valido", e.EsABBValido());
            Verificar("el arbol de referencia no cambio (copia independiente)", t.Inorden().SequenceEqual(ino) && t.Tamano == 7);
            Verificar("vaciar el arbol elemento por elemento", e.Eliminar(40) && e.Eliminar(60) && e.Eliminar(80) && e.Vacio);
            Verificar("arbol vacio: Tamano 0 y Altura -1", e.Tamano == 0 && e.Altura() == -1);
            Verificar("se puede insertar de nuevo tras vaciar", e.Insertar(5) && e.Buscar(5) && e.Tamano == 1);

            Titulo("6. Casos limite");
            ArbolABB d = new ArbolABB();
            for (int i = 1; i <= 10; i++) d.Insertar(i);
            Verificar("arbol degenerado: Altura == 9", d.Altura() == 9);
            Verificar("degenerado: preorden == inorden", d.Preorden().SequenceEqual(d.Inorden()));
            Verificar("degenerado: postorden en orden inverso", Igual(d.Postorden(), 10, 9, 8, 7, 6, 5, 4, 3, 2, 1));
            ArbolABB u = new ArbolABB();
            u.Insertar(42);
            Verificar("un solo nodo: los 3 recorridos son [42]",
                Igual(u.Preorden(), 42) && Igual(u.Inorden(), 42) && Igual(u.Postorden(), 42));
            Verificar("un solo nodo: eliminar la raiz deja el arbol vacio", u.Eliminar(42) && u.Vacio);
            ArbolABB n = new ArbolABB();
            foreach (int x in new int[] { 0, -5, 5, -10, 10, -1 }) n.Insertar(x);
            Verificar("claves negativas: inorden ordenado", Igual(n.Inorden(), -10, -5, -1, 0, 5, 10));

            Titulo("7. Prueba de integridad con datos aleatorios (contra SortedSet)");
            Random rnd = new Random(12345);            // semilla fija: resultados reproducibles
            ArbolABB r = new ArbolABB();
            SortedSet<int> referencia = new SortedSet<int>();
            bool coincideIns = true;
            for (int i = 0; i < 2000; i++)
            {
                int x = rnd.Next(1000);
                bool a = r.Insertar(x);
                bool b = referencia.Add(x);
                coincideIns = coincideIns && (a == b);
            }
            Verificar("2000 inserciones: coincide con SortedSet (duplicados incluidos)", coincideIns);
            Verificar("Tamano == tamano del SortedSet", r.Tamano == referencia.Count, r.Tamano + " claves");
            Verificar("inorden == contenido ordenado del SortedSet", r.Inorden().SequenceEqual(referencia));
            Verificar("preorden y postorden tienen el mismo numero de nodos",
                r.Preorden().Count == referencia.Count && r.Postorden().Count == referencia.Count);
            bool elimOk = true;
            bool valido = true;
            for (int i = 0; i < 1500; i++)
            {
                int x = rnd.Next(1000);
                bool a = r.Eliminar(x);
                bool b = referencia.Remove(x);
                elimOk = elimOk && (a == b);
                if (i % 100 == 0) valido = valido && r.EsABBValido();
            }
            Verificar("1500 eliminaciones: resultados coinciden con SortedSet", elimOk);
            Verificar("el ABB fue valido durante las eliminaciones", valido);
            Verificar("tras eliminar: inorden == SortedSet", r.Inorden().SequenceEqual(referencia) && r.Tamano == referencia.Count);
            bool busq = true;
            for (int x = 0; x < 1000; x++) busq = busq && (r.Buscar(x) == referencia.Contains(x));
            Verificar("Buscar coincide con SortedSet para 0..999", busq);

            Titulo("8. Reconstruccion desde el preorden");
            ArbolABB q = new ArbolABB();
            foreach (int x in pre) q.Insertar(x);
            Verificar("reinsertar el preorden genera el mismo arbol",
                q.Preorden().SequenceEqual(pre) && q.Inorden().SequenceEqual(ino) && q.Postorden().SequenceEqual(pos));

            sb.AppendLine();
            sb.AppendLine("====================================================");
            sb.AppendLine("RESUMEN: " + aprobadas + " de " + total + " pruebas aprobadas ("
                + (100.0 * aprobadas / total).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "%)");
            todoOk = (aprobadas == total);
            sb.AppendLine(todoOk ? "RESULTADO FINAL: TODAS LAS PRUEBAS PASARON" : "RESULTADO FINAL: HAY PRUEBAS FALLIDAS");
            return sb.ToString();
        }
    }
}
