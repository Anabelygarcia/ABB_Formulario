using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TareaABB
{
    // Panel con doble búfer para que el dibujo del árbol no parpadee.
    public class PanelDoble : Panel
    {
        public PanelDoble()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }

    // Formulario principal: captura de datos, operaciones del ABB y dibujo del árbol.
    public class FormPrincipal : Form
    {
        private const int SepX = 46;      // separación horizontal entre nodos
        private const int SepY = 70;      // separación vertical entre niveles
        private const int Margen = 40;
        private const int Radio = 19;

        private ArbolABB arbol = new ArbolABB();

        private TextBox txtValor;
        private TextBox txtSalida;
        private Label lblInfo;
        private PanelDoble panelArbol;

        private HashSet<int> resaltados = new HashSet<int>();   // nodos iluminados (búsqueda / recorrido)
        private int valorEncontrado = int.MinValue;             // nodo hallado en la última búsqueda
        private bool hayEncontrado = false;

        private System.Windows.Forms.Timer timer;
        private List<int> recorridoActual = new List<int>();
        private int paso = 0;

        public FormPrincipal()
        {
            Text = "Tarea 4.2 - Árbol Binario de Búsqueda (ABB)";
            Size = new Size(1100, 760);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);

            ConstruirInterfaz();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 600;
            timer.Tick += Timer_Tick;

            Actualizar();
            Log("Escribe uno o varios enteros separados por coma (ej. 50, 30, 70) y pulsa Insertar.");
        }

        // ------------------------------------------------------------ INTERFAZ
        private void ConstruirInterfaz()
        {
            // --- barra superior con controles ---
            FlowLayoutPanel barra = new FlowLayoutPanel();
            barra.Dock = DockStyle.Top;
            barra.Height = 96;
            barra.Padding = new Padding(8, 8, 8, 0);
            barra.WrapContents = true;

            Label lbl = new Label();
            lbl.Text = "Valor(es):";
            lbl.AutoSize = true;
            lbl.Margin = new Padding(3, 8, 3, 0);
            barra.Controls.Add(lbl);

            txtValor = new TextBox();
            txtValor.Width = 180;
            barra.Controls.Add(txtValor);

            Button btnInsertar = CrearBoton("Insertar", BtnInsertar_Click);
            Button btnBuscar = CrearBoton("Buscar", BtnBuscar_Click);
            Button btnEliminar = CrearBoton("Eliminar", BtnEliminar_Click);
            Button btnPre = CrearBoton("Preorden", delegate (object s, EventArgs ev) { Recorrer("Preorden"); });
            Button btnIno = CrearBoton("Inorden", delegate (object s, EventArgs ev) { Recorrer("Inorden"); });
            Button btnPos = CrearBoton("Postorden", delegate (object s, EventArgs ev) { Recorrer("Postorden"); });
            Button btnEjemplo = CrearBoton("Cargar ejemplo", BtnEjemplo_Click);
            Button btnLimpiar = CrearBoton("Limpiar árbol", BtnLimpiar_Click);
            Button btnPruebas = CrearBoton("Ejecutar pruebas", BtnPruebas_Click);
            Button btnGuardar = CrearBoton("Guardar reporte", BtnGuardar_Click);

            barra.Controls.Add(btnInsertar);
            barra.Controls.Add(btnBuscar);
            barra.Controls.Add(btnEliminar);
            barra.Controls.Add(btnPre);
            barra.Controls.Add(btnIno);
            barra.Controls.Add(btnPos);
            barra.Controls.Add(btnEjemplo);
            barra.Controls.Add(btnLimpiar);
            barra.Controls.Add(btnPruebas);
            barra.Controls.Add(btnGuardar);

            AcceptButton = btnInsertar;     // Enter inserta

            // --- línea de información ---
            lblInfo = new Label();
            lblInfo.Dock = DockStyle.Top;
            lblInfo.Height = 28;
            lblInfo.TextAlign = ContentAlignment.MiddleLeft;
            lblInfo.Padding = new Padding(10, 0, 0, 0);
            lblInfo.BackColor = Color.FromArgb(31, 58, 95);
            lblInfo.ForeColor = Color.White;

            // --- salida de texto (bitácora y reporte) ---
            txtSalida = new TextBox();
            txtSalida.Multiline = true;
            txtSalida.ReadOnly = true;
            txtSalida.ScrollBars = ScrollBars.Vertical;
            txtSalida.Dock = DockStyle.Bottom;
            txtSalida.Height = 190;
            txtSalida.Font = new Font("Consolas", 9.5F);
            txtSalida.BackColor = Color.White;

            // --- panel donde se dibuja el árbol ---
            panelArbol = new PanelDoble();
            panelArbol.Dock = DockStyle.Fill;
            panelArbol.BackColor = Color.White;
            panelArbol.AutoScroll = true;
            panelArbol.Paint += PanelArbol_Paint;

            // El orden de alta importa para el acople (Dock): primero Fill, al final los bordes.
            Controls.Add(panelArbol);
            Controls.Add(txtSalida);
            Controls.Add(lblInfo);
            Controls.Add(barra);
        }

        private Button CrearBoton(string texto, EventHandler accion)
        {
            Button b = new Button();
            b.Text = texto;
            b.AutoSize = true;
            b.Padding = new Padding(6, 2, 6, 2);
            b.Click += accion;
            return b;
        }

        // ------------------------------------------------------ BITÁCORA / ESTADO
        private void Log(string texto)
        {
            txtSalida.AppendText(texto + Environment.NewLine);
        }

        private void Actualizar()
        {
            if (arbol.Vacio)
                lblInfo.Text = "Árbol vacío  |  Nodos: 0  |  Altura: -1";
            else
                lblInfo.Text = "Nodos: " + arbol.Tamano + "   |   Altura: " + arbol.Altura()
                    + "   |   Mínimo: " + arbol.Minimo() + "   |   Máximo: " + arbol.Maximo()
                    + "   |   Raíz: " + arbol.Raiz.Dato;

            int ancho = Math.Max(arbol.Tamano, 1) * SepX + 2 * Margen;
            int alto = (arbol.Altura() + 1) * SepY + 2 * Margen;
            panelArbol.AutoScrollMinSize = new Size(ancho, alto);
            panelArbol.Invalidate();
        }

        private void LimpiarResaltado()
        {
            timer.Stop();
            resaltados.Clear();
            hayEncontrado = false;
        }

        // Lee los enteros escritos en la caja de texto; devuelve null si hay error.
        private List<int> LeerValores()
        {
            List<int> lista = new List<int>();
            string[] partes = txtValor.Text.Split(new char[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string p in partes)
            {
                int v;
                if (!int.TryParse(p, out v))
                {
                    MessageBox.Show("«" + p + "» no es un número entero válido.", "Dato incorrecto",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }
                lista.Add(v);
            }
            if (lista.Count == 0)
            {
                MessageBox.Show("Escribe al menos un número entero.", "Falta el dato",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
            return lista;
        }

        // ---------------------------------------------------------- OPERACIONES
        private void BtnInsertar_Click(object sender, EventArgs e)
        {
            List<int> valores = LeerValores();
            if (valores == null) return;
            LimpiarResaltado();
            foreach (int v in valores)
            {
                if (arbol.Insertar(v)) Log("Insertar " + v + ": listo.");
                else Log("Insertar " + v + ": rechazado (ya existe, no se permiten duplicados).");
            }
            txtValor.Clear();
            txtValor.Focus();
            Actualizar();
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            List<int> valores = LeerValores();
            if (valores == null) return;
            int v = valores[0];
            LimpiarResaltado();

            List<int> camino = new List<int>();
            bool hallado = arbol.Buscar(v, camino);
            foreach (int c in camino) resaltados.Add(c);
            if (hallado)
            {
                hayEncontrado = true;
                valorEncontrado = v;
                Log("Buscar " + v + ": ENCONTRADO. Camino: " + string.Join(" -> ", camino));
            }
            else
            {
                Log("Buscar " + v + ": NO existe. Camino recorrido: "
                    + (camino.Count == 0 ? "(árbol vacío)" : string.Join(" -> ", camino)));
            }
            Actualizar();
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            List<int> valores = LeerValores();
            if (valores == null) return;
            LimpiarResaltado();
            foreach (int v in valores)
            {
                if (arbol.Eliminar(v)) Log("Eliminar " + v + ": listo.");
                else Log("Eliminar " + v + ": no existe en el árbol.");
            }
            txtValor.Clear();
            Actualizar();
        }

        private void Recorrer(string tipo)
        {
            if (arbol.Vacio)
            {
                Log(tipo + ": el árbol está vacío.");
                return;
            }
            LimpiarResaltado();
            List<int> v;
            if (tipo == "Preorden") v = arbol.Preorden();
            else if (tipo == "Inorden") v = arbol.Inorden();
            else v = arbol.Postorden();

            Log(tipo + ": " + string.Join(" ", v));
            recorridoActual = v;
            paso = 0;
            timer.Start();          // ilumina los nodos uno por uno, en el orden del recorrido
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (paso < recorridoActual.Count)
            {
                resaltados.Add(recorridoActual[paso]);
                paso++;
                panelArbol.Invalidate();
            }
            else
            {
                timer.Stop();
            }
        }

        private void BtnEjemplo_Click(object sender, EventArgs e)
        {
            LimpiarResaltado();
            arbol = new ArbolABB();
            foreach (int v in new int[] { 50, 30, 70, 20, 40, 60, 80 }) arbol.Insertar(v);
            Log("Se cargó el árbol de ejemplo: 50, 30, 70, 20, 40, 60, 80.");
            Actualizar();
        }

        private void BtnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarResaltado();
            arbol = new ArbolABB();      // el árbol anterior queda sin referencias y el recolector libera su memoria
            Log("Árbol vaciado.");
            Actualizar();
        }

        private void BtnPruebas_Click(object sender, EventArgs e)
        {
            bool ok;
            string reporte = Pruebas.Ejecutar(out ok);
            txtSalida.Text = reporte.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
            txtSalida.SelectionStart = 0;
            txtSalida.ScrollToCaret();
            MessageBox.Show(ok ? "Todas las pruebas pasaron." : "Hay pruebas fallidas; revisa el reporte.",
                "Reporte de pruebas", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "Texto (*.txt)|*.txt";
                dlg.FileName = "reporte_pruebas.txt";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(dlg.FileName, txtSalida.Text, Encoding.UTF8);
                    Log("Reporte guardado en: " + dlg.FileName);
                }
            }
        }

        // -------------------------------------------------------------- DIBUJO
        // Posición en rejilla: x = orden inorden del nodo, y = nivel.
        private void Posicionar(Nodo n, int nivel, ref int contador, Dictionary<Nodo, Point> pos)
        {
            if (n == null) return;
            Posicionar(n.Izq, nivel + 1, ref contador, pos);
            pos[n] = new Point(contador, nivel);
            contador++;
            Posicionar(n.Der, nivel + 1, ref contador, pos);
        }

        private Point Centro(Point rejilla)
        {
            return new Point(Margen + rejilla.X * SepX, Margen + rejilla.Y * SepY);
        }

        private void PanelArbol_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(panelArbol.AutoScrollPosition.X, panelArbol.AutoScrollPosition.Y);

            if (arbol.Vacio)
            {
                g.DrawString("El árbol está vacío. Inserta valores para comenzar.", Font, Brushes.Gray, 20, 20);
                return;
            }

            Dictionary<Nodo, Point> pos = new Dictionary<Nodo, Point>();
            int contador = 0;
            Posicionar(arbol.Raiz, 0, ref contador, pos);

            // 1) aristas
            using (Pen lapiz = new Pen(Color.FromArgb(90, 90, 90), 2f))
            {
                foreach (KeyValuePair<Nodo, Point> kv in pos)
                {
                    Point p = Centro(kv.Value);
                    if (kv.Key.Izq != null) g.DrawLine(lapiz, p, Centro(pos[kv.Key.Izq]));
                    if (kv.Key.Der != null) g.DrawLine(lapiz, p, Centro(pos[kv.Key.Der]));
                }
            }

            // 2) nodos
            StringFormat centrado = new StringFormat();
            centrado.Alignment = StringAlignment.Center;
            centrado.LineAlignment = StringAlignment.Center;

            foreach (KeyValuePair<Nodo, Point> kv in pos)
            {
                Point p = Centro(kv.Value);
                int dato = kv.Key.Dato;

                Color relleno = Color.FromArgb(46, 111, 191);              // azul normal
                if (kv.Key == arbol.Raiz) relleno = Color.FromArgb(31, 58, 95);   // raíz
                if (resaltados.Contains(dato)) relleno = Color.FromArgb(217, 116, 26);   // iluminado
                if (hayEncontrado && dato == valorEncontrado) relleno = Color.FromArgb(46, 158, 91);  // hallado

                using (SolidBrush pincel = new SolidBrush(relleno))
                {
                    g.FillEllipse(pincel, p.X - Radio, p.Y - Radio, 2 * Radio, 2 * Radio);
                }
                g.DrawEllipse(Pens.White, p.X - Radio, p.Y - Radio, 2 * Radio, 2 * Radio);
                RectangleF caja = new RectangleF(p.X - Radio, p.Y - Radio, 2 * Radio, 2 * Radio);
                g.DrawString(dato.ToString(), Font, Brushes.White, caja, centrado);
            }
            centrado.Dispose();
        }
    }
}
