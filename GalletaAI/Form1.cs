// Archivo: GalletaAI/Form1.cs
using GalletaAI.Core;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GalletaAI
{
    public partial class Form1 : Form
    {
        private GameState _gameState = null!;
        private AILogic _aiLogic = null!;

        // Configuración visual
        private const int DOT_RADIUS = 8;
        private const int LINE_WIDTH = 5;
        private const int GRID_SIZE = 60; // Tamaño de la cuadrícula del background
        private int _cellSize = 120; // 2 cuadros de la cuadrícula = 1 celda del juego

        // Para resaltar la línea sobre la que está el mouse
        private Move? _hoveredMove = null;

        // Bandera para bloquear interacción durante turno de IA
        private bool _aiThinking = false;

        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.ClientSize = new Size(700, 750);

            this.Paint += Form1_Paint;
            this.MouseMove += Form1_MouseMove;
            this.MouseClick += Form1_MouseClick;
            btnNewGame.Click += btnNewGame_Click;

            StartNewGame();
        }

        private void StartNewGame()
        {
            _gameState = new GameState();
            _aiLogic = new AILogic(depthBound: 6);
            _aiThinking = false;
            lblStatus.Text = $"Humano: {_gameState.HumanScore} | IA: {_gameState.AIScore}";
            this.Invalidate();
        }

        private void btnNewGame_Click(object? sender, EventArgs e)
        {
            StartNewGame();
        }

        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2 + 20;

            // Dibujar la cuadrícula de fondo
            DrawGridBackground(g);

            // Dibujar el diamante pixelado grande
            DrawPixelatedDiamond(g, centerX, centerY);

            // Dibujar las líneas del juego
            DrawGameLines(g, centerX, centerY);

            // Dibujar la línea resaltada (hover)
            if (_hoveredMove != null && !_aiThinking)
            {
                DrawMove(g, _hoveredMove, new Pen(Color.FromArgb(180, 100, 149, 237), LINE_WIDTH + 3), centerX, centerY);
            }

            // Dibujar los dueños de los cuadros
            DrawSquareOwners(g, centerX, centerY);
        }

        private void DrawGridBackground(Graphics g)
        {
            // Cuadrícula de fondo
            Pen gridPen = new Pen(Color.FromArgb(200, 220, 220, 220), 1);

            for (int x = 0; x < this.ClientSize.Width; x += GRID_SIZE)
                g.DrawLine(gridPen, x, 0, x, this.ClientSize.Height);
            for (int y = 0; y < this.ClientSize.Height; y += GRID_SIZE)
                g.DrawLine(gridPen, 0, y, this.ClientSize.Width, y);
        }

        private void DrawPixelatedDiamond(Graphics g, int centerX, int centerY)
        {
            // Tamaño del pixel en la cuadrícula
            int pixelSize = GRID_SIZE;

            // Definir el patrón del diamante (MUCHO MÁS GRANDE)
            // Cada fila representa cuántos píxeles a cada lado del centro
            int[][] diamondPattern = new int[][]
            {
                new int[] {0, 1},      // Fila 0: 1 pixel (esquina superior)
                new int[] {-1, 2},     // Fila 1: 3 pixels
                new int[] {-2, 3},     // Fila 2: 5 pixels
                new int[] {-3, 4},     // Fila 3: 7 pixels
                new int[] {-4, 5},     // Fila 4: 9 pixels
                new int[] {-5, 6},     // Fila 5: 11 pixels
                new int[] {-6, 7},     // Fila 6: 13 pixels
                new int[] {-7, 8},     // Fila 7: 15 pixels
                new int[] {-8, 9},     // Fila 8: 17 pixels (centro)
                new int[] {-7, 8},     // Fila 9: 15 pixels
                new int[] {-6, 7},     // Fila 10: 13 pixels
                new int[] {-5, 6},     // Fila 11: 11 pixels
                new int[] {-4, 5},     // Fila 12: 9 pixels
                new int[] {-3, 4},     // Fila 13: 7 pixels
                new int[] {-2, 3},     // Fila 14: 5 pixels
                new int[] {-1, 2},     // Fila 15: 3 pixels
                new int[] {0, 1}       // Fila 16: 1 pixel (esquina inferior)
            };

            // Dibujar el diamante pixel por pixel
            int startY = centerY - (diamondPattern.Length * pixelSize / 2);

            for (int row = 0; row < diamondPattern.Length; row++)
            {
                int startCol = diamondPattern[row][0];
                int endCol = diamondPattern[row][1];

                int y = startY + (row * pixelSize);

                for (int col = startCol; col < endCol; col++)
                {
                    int x = centerX + (col * pixelSize);

                    // Verificar si es una esquina del rombo
                    bool isCorner = (row == 0 && col == 0) ||                    // Esquina superior
                                    (row == 8 && col == -8) ||                    // Esquina izquierda
                                    (row == 8 && col == 8) ||                     // Esquina derecha
                                    (row == 16 && col == 0);                      // Esquina inferior

                    if (isCorner)
                    {
                        // Pintar las esquinas de negro
                        using (SolidBrush brush = new SolidBrush(Color.Black))
                        {
                            g.FillRectangle(brush, x, y, pixelSize, pixelSize);
                        }
                    }

                    // Dibujar borde del pixel
                    using (Pen borderPen = new Pen(Color.Black, 4))
                    {
                        g.DrawRectangle(borderPen, x, y, pixelSize, pixelSize);
                    }
                }
            }
        }

        private Point GetDotPosition(int row, int col, int centerX, int centerY)
        {
            int x = centerX + (col - 2) * _cellSize;
            int y = centerY + (row - 2) * _cellSize;

            // Redondear a la cuadrícula más cercana
            x = (int)Math.Round((double)x / GRID_SIZE) * GRID_SIZE;
            y = (int)Math.Round((double)y / GRID_SIZE) * GRID_SIZE;

            return new Point(x, y);
        }

        private void DrawDots(Graphics g, int centerX, int centerY)
        {
            for (int r = 0; r <= GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c <= GameState.BOARD_SIZE; c++)
                {
                    Point pos = GetDotPosition(r, c, centerX, centerY);

                    // Sombra
                    g.FillEllipse(new SolidBrush(Color.FromArgb(50, 0, 0, 0)),
                        pos.X - DOT_RADIUS + 2, pos.Y - DOT_RADIUS + 2,
                        DOT_RADIUS * 2, DOT_RADIUS * 2);

                    // Punto
                    g.FillEllipse(Brushes.DarkSlateGray,
                        pos.X - DOT_RADIUS, pos.Y - DOT_RADIUS,
                        DOT_RADIUS * 2, DOT_RADIUS * 2);

                    // Borde
                    g.DrawEllipse(new Pen(Color.White, 2),
                        pos.X - DOT_RADIUS, pos.Y - DOT_RADIUS,
                        DOT_RADIUS * 2, DOT_RADIUS * 2);
                }
            }
        }

        private void DrawGameLines(Graphics g, int centerX, int centerY)
        {
            // Líneas horizontales
            for (int r = 0; r <= GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c < GameState.BOARD_SIZE; c++)
                {
                    if (_gameState.HorizontalLines[r, c])
                    {
                        Point p1 = GetDotPosition(r, c, centerX, centerY);
                        Point p2 = GetDotPosition(r, c + 1, centerX, centerY);

                        // Sombra
                        g.DrawLine(new Pen(Color.FromArgb(50, 0, 0, 0), LINE_WIDTH),
                            p1.X + 2, p1.Y + 2, p2.X + 2, p2.Y + 2);

                        // Línea
                        g.DrawLine(new Pen(Color.CornflowerBlue, LINE_WIDTH), p1, p2);
                    }
                }
            }

            // Líneas verticales
            for (int r = 0; r < GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c <= GameState.BOARD_SIZE; c++)
                {
                    if (_gameState.VerticalLines[r, c])
                    {
                        Point p1 = GetDotPosition(r, c, centerX, centerY);
                        Point p2 = GetDotPosition(r + 1, c, centerX, centerY);

                        // Sombra
                        g.DrawLine(new Pen(Color.FromArgb(50, 0, 0, 0), LINE_WIDTH),
                            p1.X + 2, p1.Y + 2, p2.X + 2, p2.Y + 2);

                        // Línea
                        g.DrawLine(new Pen(Color.CornflowerBlue, LINE_WIDTH), p1, p2);
                    }
                }
            }
        }

        private void DrawSquareOwners(Graphics g, int centerX, int centerY)
        {
            for (int r = 0; r < GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c < GameState.BOARD_SIZE; c++)
                {
                    Player owner = _gameState.SquareOwners[r, c];
                    if (owner != Player.None)
                    {
                        Point p1 = GetDotPosition(r, c, centerX, centerY);
                        Point p2 = GetDotPosition(r, c + 1, centerX, centerY);
                        Point p3 = GetDotPosition(r + 1, c + 1, centerX, centerY);
                        Point p4 = GetDotPosition(r + 1, c, centerX, centerY);

                        int centerSquareX = (p1.X + p2.X + p3.X + p4.X) / 4;
                        int centerSquareY = (p1.Y + p2.Y + p3.Y + p4.Y) / 4;

                        // Fondo
                        Point[] square = { p1, p2, p3, p4 };
                        Color fillColor = (owner == Player.Human)
                            ? Color.FromArgb(80, 34, 139, 34)
                            : Color.FromArgb(80, 220, 20, 60);
                        g.FillPolygon(new SolidBrush(fillColor), square);

                        // Texto
                        string text = (owner == Player.Human) ? "H" : "IA";
                        Color color = (owner == Player.Human) ? Color.ForestGreen : Color.Crimson;

                        using (Font font = new Font("Arial", 24, FontStyle.Bold))
                        {
                            SizeF size = g.MeasureString(text, font);

                            g.DrawString(text, font, new SolidBrush(Color.FromArgb(100, 0, 0, 0)),
                                centerSquareX - size.Width / 2 + 2,
                                centerSquareY - size.Height / 2 + 2);

                            g.DrawString(text, font, new SolidBrush(color),
                                centerSquareX - size.Width / 2,
                                centerSquareY - size.Height / 2);
                        }
                    }
                }
            }
        }

        private void DrawMove(Graphics g, Move move, Pen pen, int centerX, int centerY)
        {
            if (move.IsHorizontal)
            {
                Point p1 = GetDotPosition(move.Row, move.Col, centerX, centerY);
                Point p2 = GetDotPosition(move.Row, move.Col + 1, centerX, centerY);
                g.DrawLine(pen, p1, p2);
            }
            else
            {
                Point p1 = GetDotPosition(move.Row, move.Col, centerX, centerY);
                Point p2 = GetDotPosition(move.Row + 1, move.Col, centerX, centerY);
                g.DrawLine(pen, p1, p2);
            }
        }

        private Move? GetMoveAtPosition(int mouseX, int mouseY)
        {
            const int TOLERANCE = 15;
            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2 + 20;

            // Líneas horizontales
            for (int r = 0; r <= GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c < GameState.BOARD_SIZE; c++)
                {
                    if (_gameState.HorizontalLines[r, c]) continue;

                    Point p1 = GetDotPosition(r, c, centerX, centerY);
                    Point p2 = GetDotPosition(r, c + 1, centerX, centerY);

                    if (IsPointNearLine(mouseX, mouseY, p1, p2, TOLERANCE))
                        return new Move(true, r, c);
                }
            }

            // Líneas verticales
            for (int r = 0; r < GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c <= GameState.BOARD_SIZE; c++)
                {
                    if (_gameState.VerticalLines[r, c]) continue;

                    Point p1 = GetDotPosition(r, c, centerX, centerY);
                    Point p2 = GetDotPosition(r + 1, c, centerX, centerY);

                    if (IsPointNearLine(mouseX, mouseY, p1, p2, TOLERANCE))
                        return new Move(false, r, c);
                }
            }

            return null;
        }

        private bool IsPointNearLine(int px, int py, Point p1, Point p2, int tolerance)
        {
            double dist = DistanceFromPointToLine(px, py, p1.X, p1.Y, p2.X, p2.Y);

            double minX = Math.Min(p1.X, p2.X) - tolerance;
            double maxX = Math.Max(p1.X, p2.X) + tolerance;
            double minY = Math.Min(p1.Y, p2.Y) - tolerance;
            double maxY = Math.Max(p1.Y, p2.Y) + tolerance;

            return dist <= tolerance && px >= minX && px <= maxX && py >= minY && py <= maxY;
        }

        private double DistanceFromPointToLine(int px, int py, int x1, int y1, int x2, int y2)
        {
            double A = px - x1;
            double B = py - y1;
            double C = x2 - x1;
            double D = y2 - y1;

            double dot = A * C + B * D;
            double lenSq = C * C + D * D;
            double param = (lenSq != 0) ? dot / lenSq : -1;

            double xx, yy;

            if (param < 0) { xx = x1; yy = y1; }
            else if (param > 1) { xx = x2; yy = y2; }
            else { xx = x1 + param * C; yy = y1 + param * D; }

            double dx = px - xx;
            double dy = py - yy;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void Form1_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_aiThinking || _gameState.CurrentPlayer != Player.Human || _gameState.IsGameOver())
            {
                _hoveredMove = null;
                this.Invalidate();
                return;
            }

            Move? move = GetMoveAtPosition(e.X, e.Y);
            if (move != _hoveredMove)
            {
                _hoveredMove = move;
                this.Invalidate();
            }
        }

        private async void Form1_MouseClick(object? sender, MouseEventArgs e)
        {
            if (_aiThinking || _gameState.CurrentPlayer != Player.Human || _gameState.IsGameOver())
                return;

            Move? move = GetMoveAtPosition(e.X, e.Y);
            if (move == null) return;

            Player nextPlayer = _gameState.ApplyMove(move);
            UpdateStatus();
            this.Invalidate();

            if (nextPlayer == Player.Human)
                return;

            if (_gameState.IsGameOver())
            {
                ShowGameOver();
                return;
            }

            await Task.Delay(300);
            await AITurn();
        }

        private async Task AITurn()
        {
            _aiThinking = true;
            this.Cursor = Cursors.WaitCursor;

            while (_gameState.CurrentPlayer == Player.AI && !_gameState.IsGameOver())
            {
                lblStatus.Text = "🤖 IA está pensando...";
                lblStatus.ForeColor = Color.OrangeRed;
                this.Invalidate();
                await Task.Delay(100);

                Move? bestMove = await Task.Run(() => _aiLogic.FindBestMove(_gameState));

                if (bestMove == null) break;

                Player nextPlayer = _gameState.ApplyMove(bestMove);
                UpdateStatus();
                this.Invalidate();

                await Task.Delay(500);

                if (nextPlayer != Player.AI)
                    break;
            }

            _aiThinking = false;
            this.Cursor = Cursors.Default;
            UpdateStatus();

            if (_gameState.IsGameOver())
            {
                ShowGameOver();
            }
        }

        private void UpdateStatus()
        {
            lblStatus.Text = $"Humano: {_gameState.HumanScore} | IA: {_gameState.AIScore}";
            lblStatus.ForeColor = Color.Black;
        }

        private void ShowGameOver()
        {
            string winner;
            if (_gameState.HumanScore > _gameState.AIScore)
                winner = "¡Ganaste! 🎉";
            else if (_gameState.AIScore > _gameState.HumanScore)
                winner = "La IA ganó 🤖";
            else
                winner = "¡Empate! 🤝";

            MessageBox.Show($"{winner}\n\nPuntuación final:\nHumano: {_gameState.HumanScore}\nIA: {_gameState.AIScore}",
                            "Juego Terminado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}