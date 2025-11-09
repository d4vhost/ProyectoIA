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
        private const int DOT_RADIUS = 6;
        private const int LINE_WIDTH = 4;
        private const int MARGIN = 80;
        private int _cellSize = 60;

        // Para resaltar la línea sobre la que está el mouse
        private Move? _hoveredMove = null;

        // Bandera para bloquear interacción durante turno de IA
        private bool _aiThinking = false;

        public Form1()
        {
            InitializeComponent();
            StartNewGame();

            // Hacer el formulario de doble buffer para evitar parpadeo
            this.DoubleBuffered = true;
            this.Paint += Form1_Paint;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.ClientSize = new Size(600, 650);
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

        // Dibuja el tablero completo en forma de rombo escalonado
        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Dibujar fondo cuadriculado
            DrawGridBackground(g);

            // Dibujar la forma de rombo escalonado
            DrawDiamondShape(g);

            // Dibujar las líneas del juego
            DrawGameLines(g);

            // Resaltar la línea sobre la que está el mouse (solo si no está bloqueado)
            if (_hoveredMove != null && !_aiThinking)
            {
                DrawMove(g, _hoveredMove, new Pen(Color.FromArgb(150, 100, 149, 237), LINE_WIDTH + 2));
            }

            // Dibujar las iniciales de los dueños de los cuadros
            DrawSquareOwners(g);
        }

        private void DrawGridBackground(Graphics g)
        {
            // Dibujar cuadrícula de fondo
            Pen gridPen = new Pen(Color.FromArgb(50, 200, 200, 200), 1);
            int gridSize = 20;

            for (int x = 0; x < this.ClientSize.Width; x += gridSize)
            {
                g.DrawLine(gridPen, x, 0, x, this.ClientSize.Height);
            }
            for (int y = 0; y < this.ClientSize.Height; y += gridSize)
            {
                g.DrawLine(gridPen, 0, y, this.ClientSize.Width, y);
            }
        }

        private void DrawDiamondShape(Graphics g)
        {
            // Dibujar el rombo escalonado con esquinas pintadas
            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2 - 20;

            // Calcular posiciones para el rombo escalonado
            for (int r = 0; r <= GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c <= GameState.BOARD_SIZE; c++)
                {
                    Point pos = GetDotPosition(r, c, centerX, centerY);

                    // Dibujar cuadros completos (relleno para las esquinas negras)
                    if (r < GameState.BOARD_SIZE && c < GameState.BOARD_SIZE)
                    {
                        Point p1 = GetDotPosition(r, c, centerX, centerY);
                        Point p2 = GetDotPosition(r, c + 1, centerX, centerY);
                        Point p3 = GetDotPosition(r + 1, c + 1, centerX, centerY);
                        Point p4 = GetDotPosition(r + 1, c, centerX, centerY);

                        // Pintar esquinas en negro
                        if (IsCornerSquare(r, c))
                        {
                            Point[] square = { p1, p2, p3, p4 };
                            g.FillPolygon(Brushes.Black, square);
                        }
                        else
                        {
                            // Dibujar borde suave para otros cuadros
                            Point[] square = { p1, p2, p3, p4 };
                            g.DrawPolygon(new Pen(Color.FromArgb(30, 0, 0, 0), 1), square);
                        }
                    }

                    // Dibujar los puntos (dots)
                    g.FillEllipse(Brushes.DarkSlateGray,
                        pos.X - DOT_RADIUS, pos.Y - DOT_RADIUS,
                        DOT_RADIUS * 2, DOT_RADIUS * 2);
                }
            }
        }

        private bool IsCornerSquare(int r, int c)
        {
            // Esquinas del tablero 4x4: (0,0), (0,3), (3,0), (3,3)
            return (r == 0 && c == 0) || (r == 0 && c == 3) ||
                   (r == 3 && c == 0) || (r == 3 && c == 3);
        }

        private Point GetDotPosition(int row, int col, int centerX, int centerY)
        {
            // Crear efecto de rombo escalonado
            int offset = Math.Abs(2 - row) * (_cellSize / 4);
            int x = centerX + (col - 2) * _cellSize + offset;
            int y = centerY + (row - 2) * _cellSize;
            return new Point(x, y);
        }

        private void DrawGameLines(Graphics g)
        {
            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2 - 20;

            // Dibujar las líneas horizontales
            for (int r = 0; r <= GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c < GameState.BOARD_SIZE; c++)
                {
                    if (_gameState.HorizontalLines[r, c])
                    {
                        Point p1 = GetDotPosition(r, c, centerX, centerY);
                        Point p2 = GetDotPosition(r, c + 1, centerX, centerY);
                        g.DrawLine(new Pen(Color.CornflowerBlue, LINE_WIDTH), p1, p2);
                    }
                }
            }

            // Dibujar las líneas verticales
            for (int r = 0; r < GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c <= GameState.BOARD_SIZE; c++)
                {
                    if (_gameState.VerticalLines[r, c])
                    {
                        Point p1 = GetDotPosition(r, c, centerX, centerY);
                        Point p2 = GetDotPosition(r + 1, c, centerX, centerY);
                        g.DrawLine(new Pen(Color.CornflowerBlue, LINE_WIDTH), p1, p2);
                    }
                }
            }
        }

        private void DrawSquareOwners(Graphics g)
        {
            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2 - 20;

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

                        string text = (owner == Player.Human) ? "H" : "IA";
                        Color color = (owner == Player.Human) ? Color.ForestGreen : Color.Crimson;

                        using (Font font = new Font("Arial", 20, FontStyle.Bold))
                        {
                            SizeF size = g.MeasureString(text, font);
                            g.DrawString(text, font, new SolidBrush(color),
                                centerSquareX - size.Width / 2,
                                centerSquareY - size.Height / 2);
                        }
                    }
                }
            }
        }

        private void DrawMove(Graphics g, Move move, Pen pen)
        {
            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2 - 20;

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
            const int TOLERANCE = 12;
            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2 - 20;

            // Revisar líneas horizontales
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

            // Revisar líneas verticales
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
            // No permitir hover si la IA está pensando o no es turno del humano
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
            // BLOQUEAR CLICS durante turno de IA
            if (_aiThinking || _gameState.CurrentPlayer != Player.Human || _gameState.IsGameOver())
                return;

            Move? move = GetMoveAtPosition(e.X, e.Y);
            if (move == null) return;

            // El humano hace su movimiento
            Player nextPlayer = _gameState.ApplyMove(move);
            UpdateStatus();
            this.Invalidate();

            // Si el humano completó un cuadro, sigue su turno
            if (nextPlayer == Player.Human)
                return;

            // Verificar si el juego terminó
            if (_gameState.IsGameOver())
            {
                ShowGameOver();
                return;
            }

            // Turno de la IA - BLOQUEAR INTERACCIÓN
            await Task.Delay(300);
            await AITurn();
        }

        private async Task AITurn()
        {
            _aiThinking = true; // ACTIVAR BLOQUEO
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

            _aiThinking = false; // DESACTIVAR BLOQUEO
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
    }
}