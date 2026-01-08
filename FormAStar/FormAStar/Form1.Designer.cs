using System;
using System.Collections.Generic;
using System.Drawing; // Necesario para dibujar
using System.Linq;
using System.Threading.Tasks; // Necesario para la animación
using System.Windows.Forms;

namespace FormAStar
{
    public partial class Form1 : Form
    {
        // --- CONFIGURACIÓN DEL JUEGO ---
        private const int CELL_SIZE = 30; // Tamaño de cada cuadro (sprites)
        private const int GRID_W = 20;    // Ancho del mapa
        private const int GRID_H = 15;    // Alto del mapa

        // --- COLORES (ESTILO JUEGO) ---
        private Brush colorMuro = Brushes.DimGray;
        private Brush colorSuelo = Brushes.WhiteSmoke;
        private Brush colorInicio = Brushes.GreenYellow;
        private Brush colorMeta = Brushes.OrangeRed;
        private Brush colorCamino = Brushes.DeepSkyBlue;
        private Brush colorOpen = Brushes.LightGreen; // Nodos siendo evaluados
        private Brush colorClosed = Brushes.LightPink; // Nodos ya visitados

        // --- VARIABLES DEL ALGORITMO ---
        private Nodo[,] grid;
        private Nodo inicio;
        private Nodo meta;
        private bool ejecutando = false;

        public Form1()
        {
            // Configuración básica de la ventana
            this.Text = "A* Pathfinding Demo - Haz clic para poner muros";
            this.Size = new Size(GRID_W * CELL_SIZE + 40, GRID_H * CELL_SIZE + 80);
            this.DoubleBuffered = true; // Evita parpadeos en la animación

            // Crear botón de inicio
            Button btnStart = new Button();
            btnStart.Text = "INICIAR BÚSQUEDA";
            btnStart.Location = new Point(10, GRID_H * CELL_SIZE + 10);
            btnStart.Size = new Size(150, 30);
            btnStart.Click += (s, e) => { if (!ejecutando) RunAStar(); };
            this.Controls.Add(btnStart);

            // Botón de limpiar
            Button btnClear = new Button();
            btnClear.Text = "Reiniciar";
            btnClear.Location = new Point(170, GRID_H * CELL_SIZE + 10);
            btnClear.Click += (s, e) => { InicializarGrid(); this.Invalidate(); };
            this.Controls.Add(btnClear);

            // Eventos del Mouse
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;

            InicializarGrid();
        }

        private void InicializarGrid()
        {
            grid = new Nodo[GRID_W, GRID_H];
            for (int x = 0; x < GRID_W; x++)
                for (int y = 0; y < GRID_H; y++)
                    grid[x, y] = new Nodo(x, y);

            // Posiciones por defecto (estilo RPG)
            inicio = grid[2, 7];
            meta = grid[17, 7];
            ejecutando = false;
        }

        // --- DIBUJADO (RENDER LOOP) ---
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            for (int x = 0; x < GRID_W; x++)
            {
                for (int y = 0; y < GRID_H; y++)
                {
                    Nodo n = grid[x, y];
                    Rectangle rect = new Rectangle(x * CELL_SIZE, y * CELL_SIZE, CELL_SIZE - 1, CELL_SIZE - 1);

                    Brush b = colorSuelo;

                    if (n.EsMuro) b = colorMuro;
                    else if (n == inicio) b = colorInicio;
                    else if (n == meta) b = colorMeta;
                    else if (n.EsCamino) b = colorCamino;
                    else if (n.EnOpen) b = colorOpen;     // Animación
                    else if (n.EnClosed) b = colorClosed; // Animación

                    g.FillRectangle(b, rect);
                    g.DrawRectangle(Pens.LightGray, rect); // Bordes de cuadrícula
                }
            }
        }

        // --- INTERACCIÓN CON MOUSE ---
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            ManipularMapa(e);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
                ManipularMapa(e);
        }

        private void ManipularMapa(MouseEventArgs e)
        {
            if (ejecutando) return;

            int x = e.X / CELL_SIZE;
            int y = e.Y / CELL_SIZE;

            if (x >= 0 && x < GRID_W && y >= 0 && y < GRID_H)
            {
                Nodo n = grid[x, y];
                if (n != inicio && n != meta)
                {
                    // Izquierdo: Muro, Derecho: Borrar
                    n.EsMuro = (e.Button == MouseButtons.Left);
                    this.Invalidate(); // Redibujar
                }
            }
        }

        // --- EL ALGORITMO A* (VERSIÓN ASYNC PARA ANIMACIÓN) ---
        private async void RunAStar()
        {
            ejecutando = true;

            // Limpiar datos previos
            foreach (var n in grid) { n.G = n.H = 0; n.Padre = null; n.EnOpen = n.EnClosed = n.EsCamino = false; }

            List<Nodo> openSet = new List<Nodo>();
            HashSet<Nodo> closedSet = new HashSet<Nodo>();

            openSet.Add(inicio);
            inicio.EnOpen = true;

            while (openSet.Count > 0)
            {
                // Ordenar por costo F (simulando Priority Queue)
                Nodo actual = openSet.OrderBy(n => n.F).First();

                if (actual == meta)
                {
                    ReconstruirCamino(actual);
                    ejecutando = false;
                    return;
                }

                openSet.Remove(actual);
                closedSet.Add(actual);

                actual.EnOpen = false;
                actual.EnClosed = true; // Marcar como "visitado" (Color Rosa)

                // --- ANIMACIÓN: Pausa para ver el proceso ---
                this.Invalidate(); // Forzar repintado
                await Task.Delay(20); // Retraso de 20ms (ajusta para velocidad)
                // --------------------------------------------

                foreach (var vecino in ObtenerVecinos(actual))
                {
                    if (vecino.EsMuro || closedSet.Contains(vecino)) continue;

                    int nuevoCosto = actual.G + 1;

                    if (nuevoCosto < vecino.G || !openSet.Contains(vecino))
                    {
                        vecino.G = nuevoCosto;
                        vecino.H = Math.Abs(vecino.X - meta.X) + Math.Abs(vecino.Y - meta.Y); // Manhattan
                        vecino.Padre = actual;

                        if (!openSet.Contains(vecino))
                        {
                            openSet.Add(vecino);
                            vecino.EnOpen = true; // Marcar como "por visitar" (Color Verde)
                        }
                    }
                }
            }
            MessageBox.Show("No se encontró camino");
            ejecutando = false;
        }

        private List<Nodo> ObtenerVecinos(Nodo n)
        {
            List<Nodo> lista = new List<Nodo>();
            int[] dx = { 0, 0, 1, -1 }; // Arriba, Abajo, Der, Izq
            int[] dy = { 1, -1, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                int nx = n.X + dx[i];
                int ny = n.Y + dy[i];

                if (nx >= 0 && nx < GRID_W && ny >= 0 && ny < GRID_H)
                    lista.Add(grid[nx, ny]);
            }
            return lista;
        }

        private void ReconstruirCamino(Nodo fin)
        {
            Nodo temp = fin;
            while (temp != null)
            {
                temp.EsCamino = true; // Color Azul
                temp = temp.Padre;
            }
            this.Invalidate();
        }
    }

    // --- CLASE DE DATOS ---
    public class Nodo
    {
        public int X, Y;
        public bool EsMuro = false;

        // Variables visuales
        public bool EnOpen = false;
        public bool EnClosed = false;
        public bool EsCamino = false;

        // Variables A*
        public int G, H;
        public int F => G + H;
        public Nodo Padre;

        public Nodo(int x, int y) { X = x; Y = y; }
    }
}