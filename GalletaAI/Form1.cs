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
        private const int MARGIN = 50;
        private int _cellSize = 80;

        // Para resaltar la línea sobre la que está el mouse
        private Move? _hoveredMove = null;

        public Form1()
        {
            InitializeComponent();
            StartNewGame();

            // Hacer el formulario de doble buffer para evitar parpadeo
            this.DoubleBuffered = true;
            this.Paint += Form1_Paint;
        }

        private void StartNewGame()
        {
            _gameState = new GameState();
            _aiLogic = new AILogic(depthBound: 6); // Puedes ajustar la profundidad aquí
            lblStatus.Text = $"Humano: {_gameState.HumanScore} | IA: {_gameState.AIScore}";
            this.Invalidate(); // Redibujar el tablero
        }

        private void btnNewGame_Click(object? sender, EventArgs e)
        {
            StartNewGame();
        }

        // Dibuja el tablero completo
        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Dibujar los puntos (dots)
            for (int r = 0; r <= GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c <= GameState.BOARD_SIZE; c++)
                {
                    int x = MARGIN + c * _cellSize;
                    int y = MARGIN + r * _cellSize;
                    g.FillEllipse(Brushes.Black, x - DOT_RADIUS, y - DOT_RADIUS, DOT_RADIUS * 2, DOT_RADIUS * 2);
                }
            }

            // Dibujar las líneas horizontales
            for (int r = 0; r <= GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c < GameState.BOARD_SIZE; c++)
                {
                    if (_gameState.HorizontalLines[r, c])
                    {
                        int x1 = MARGIN + c * _cellSize;
                        int y1 = MARGIN + r * _cellSize;
                        int x2 = MARGIN + (c + 1) * _cellSize;
                        int y2 = y1;
                        g.DrawLine(new Pen(Color.Blue, LINE_WIDTH), x1, y1, x2, y2);
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
                        int x1 = MARGIN + c * _cellSize;
                        int y1 = MARGIN + r * _cellSize;
                        int x2 = x1;
                        int y2 = MARGIN + (r + 1) * _cellSize;
                        g.DrawLine(new Pen(Color.Blue, LINE_WIDTH), x1, y1, x2, y2);
                    }
                }
            }

            // Resaltar la línea sobre la que está el mouse
            if (_hoveredMove != null)
            {
                DrawMove(g, _hoveredMove, new Pen(Color.LightGray, LINE_WIDTH));
            }

            // Dibujar las iniciales de los dueños de los cuadros
            for (int r = 0; r < GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c < GameState.BOARD_SIZE; c++)
                {
                    Player owner = _gameState.SquareOwners[r, c];
                    if (owner != Player.None)
                    {
                        int x = MARGIN + c * _cellSize + _cellSize / 2;
                        int y = MARGIN + r * _cellSize + _cellSize / 2;

                        string text = (owner == Player.Human) ? "H" : "IA";
                        Color color = (owner == Player.Human) ? Color.Green : Color.Red;

                        using (Font font = new Font("Arial", 24, FontStyle.Bold))
                        {
                            SizeF size = g.MeasureString(text, font);
                            g.DrawString(text, font, new SolidBrush(color), x - size.Width / 2, y - size.Height / 2);
                        }
                    }
                }
            }
        }

        private void DrawMove(Graphics g, Move move, Pen pen)
        {
            if (move.IsHorizontal)
            {
                int x1 = MARGIN + move.Col * _cellSize;
                int y1 = MARGIN + move.Row * _cellSize;
                int x2 = MARGIN + (move.Col + 1) * _cellSize;
                int y2 = y1;
                g.DrawLine(pen, x1, y1, x2, y2);
            }
            else
            {
                int x1 = MARGIN + move.Col * _cellSize;
                int y1 = MARGIN + move.Row * _cellSize;
                int x2 = x1;
                int y2 = MARGIN + (move.Row + 1) * _cellSize;
                g.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        // Detecta sobre qué línea está el mouse
        private Move? GetMoveAtPosition(int mouseX, int mouseY)
        {
            const int TOLERANCE = 10; // Distancia en píxeles para detectar una línea

            // Revisar líneas horizontales
            for (int r = 0; r <= GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c < GameState.BOARD_SIZE; c++)
                {
                    if (_gameState.HorizontalLines[r, c]) continue; // Ya está dibujada

                    int x1 = MARGIN + c * _cellSize;
                    int y1 = MARGIN + r * _cellSize;
                    int x2 = MARGIN + (c + 1) * _cellSize;

                    if (mouseY >= y1 - TOLERANCE && mouseY <= y1 + TOLERANCE &&
                        mouseX >= x1 && mouseX <= x2)
                    {
                        return new Move(true, r, c);
                    }
                }
            }

            // Revisar líneas verticales
            for (int r = 0; r < GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c <= GameState.BOARD_SIZE; c++)
                {
                    if (_gameState.VerticalLines[r, c]) continue; // Ya está dibujada

                    int x1 = MARGIN + c * _cellSize;
                    int y1 = MARGIN + r * _cellSize;
                    int y2 = MARGIN + (r + 1) * _cellSize;

                    if (mouseX >= x1 - TOLERANCE && mouseX <= x1 + TOLERANCE &&
                        mouseY >= y1 && mouseY <= y2)
                    {
                        return new Move(false, r, c);
                    }
                }
            }

            return null;
        }

        private void Form1_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_gameState.CurrentPlayer != Player.Human || _gameState.IsGameOver())
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
            if (_gameState.CurrentPlayer != Player.Human || _gameState.IsGameOver())
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

            // Turno de la IA (puede hacer múltiples movimientos si completa cuadros)
            await Task.Delay(300); // Pequeña pausa para que el usuario vea el movimiento
            await AITurn();
        }

        private async Task AITurn()
        {
            while (_gameState.CurrentPlayer == Player.AI && !_gameState.IsGameOver())
            {
                lblStatus.Text = "IA está pensando...";
                this.Invalidate();
                await Task.Delay(100);

                Move? bestMove = await Task.Run(() => _aiLogic.FindBestMove(_gameState));

                if (bestMove == null) break; // No hay movimiento válido

                Player nextPlayer = _gameState.ApplyMove(bestMove);
                UpdateStatus();
                this.Invalidate();

                await Task.Delay(300); // Pausa para visualizar

                if (nextPlayer != Player.AI)
                    break; // El turno pasa al humano
            }

            if (_gameState.IsGameOver())
            {
                ShowGameOver();
            }
        }

        private void UpdateStatus()
        {
            lblStatus.Text = $"Humano: {_gameState.HumanScore} | IA: {_gameState.AIScore}";
        }

        private void ShowGameOver()
        {
            string winner;
            if (_gameState.HumanScore > _gameState.AIScore)
                winner = "¡Ganaste! 🎉";
            else if (_gameState.AIScore > _gameState.HumanScore)
                winner = "La IA ganó. 🤖";
            else
                winner = "¡Empate! 🤝";

            MessageBox.Show($"{winner}\n\nPuntuación final:\nHumano: {_gameState.HumanScore}\nIA: {_gameState.AIScore}",
                            "Juego Terminado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}