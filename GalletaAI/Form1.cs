// Archivo: GalletaAI/Form1.cs
using GalletaAI.Core;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GalletaAI
{
    public partial class Form1 : Form
    {
        private GameState _gameState = null!;
        private AILogic _aiLogic = null!;

        private const int LINE_WIDTH = 4;
        private const int GRID_SIZE = 40;
        private const int DOT_SIZE = 6;

        private Move? _hoveredMove = null;
        private bool _aiThinking = false;

        private Pen _humanPen = new Pen(Color.CornflowerBlue, LINE_WIDTH);
        private Pen _aiPen = new Pen(Color.Crimson, LINE_WIDTH);
        private Pen _hoverPen = new Pen(Color.FromArgb(180, 100, 149, 237), LINE_WIDTH + 2);
        private Pen _initialPatternPen = new Pen(Color.FromArgb(60, 60, 60), LINE_WIDTH);

        private Dictionary<string, Player> _lineOwners = new Dictionary<string, Player>();
        private HashSet<string> _initialPatternLines = new HashSet<string>();

        // IMPORTANTE: HashSet estático compartido con GameState
        public static HashSet<string> PlayableLines = new HashSet<string>();

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
            _aiLogic = new AILogic(depthBound: 3);
            _aiThinking = false;
            _lineOwners.Clear();
            _initialPatternLines.Clear();
            PlayableLines.Clear();

            DefinePlayableArea();
            DrawInitialPattern();

            UpdateStatus();
            this.Invalidate();
        }

        private void DefinePlayableArea()
        {
            var playableSquares = new List<(int row, int col)>
            {
                (1, 6),
                (2, 5), (2, 6), (2, 7),
                (3, 4), (3, 5), (3, 6), (3, 7), (3, 8),
                (4, 3), (4, 4), (4, 5), (4, 6), (4, 7), (4, 8), (4, 9),
                (5, 2), (5, 3), (5, 4), (5, 5), (5, 6), (5, 7), (5, 8), (5, 9), (5, 10),
                (6, 1), (6, 2), (6, 3), (6, 4), (6, 5), (6, 6), (6, 7), (6, 8), (6, 9), (6, 10), (6, 11),
                (7, 2), (7, 3), (7, 4), (7, 5), (7, 6), (7, 7), (7, 8), (7, 9), (7, 10),
                (8, 3), (8, 4), (8, 5), (8, 6), (8, 7), (8, 8), (8, 9),
                (9, 4), (9, 5), (9, 6), (9, 7), (9, 8),
                (10, 5), (10, 6), (10, 7),
                (11, 6)
            };

            foreach (var (row, col) in playableSquares)
            {
                PlayableLines.Add($"H_{row}_{col}");
                PlayableLines.Add($"H_{row + 1}_{col}");
                PlayableLines.Add($"V_{row}_{col}");
                PlayableLines.Add($"V_{row}_{col + 1}");
            }
        }

        private bool IsLinePlayable(Move move)
        {
            string key = move.IsHorizontal ? $"H_{move.Row}_{move.Col}" : $"V_{move.Row}_{move.Col}";
            return PlayableLines.Contains(key);
        }

        private void DrawInitialPattern()
        {
            // ✅ Marcar las 4 esquinas como NO JUGABLES antes de dibujar el patrón
            var cornerSquares = new HashSet<(int, int)> { (1, 6), (6, 1), (6, 11), (11, 6) };

            foreach (var (row, col) in cornerSquares)
            {
                _gameState.MarkCornerAsNonPlayable(row, col);
            }

            ApplyInitialMove(new Move(false, 1, 7));
            ApplyInitialMove(new Move(true, 2, 7));
            ApplyInitialMove(new Move(false, 2, 8));
            ApplyInitialMove(new Move(true, 3, 8));
            ApplyInitialMove(new Move(false, 3, 9));
            ApplyInitialMove(new Move(true, 4, 9));
            ApplyInitialMove(new Move(false, 4, 10));
            ApplyInitialMove(new Move(true, 5, 10));
            ApplyInitialMove(new Move(false, 5, 11));
            ApplyInitialMove(new Move(true, 6, 11));
            ApplyInitialMove(new Move(false, 6, 12));

            ApplyInitialMove(new Move(true, 7, 11));
            ApplyInitialMove(new Move(false, 7, 11));
            ApplyInitialMove(new Move(true, 8, 10));
            ApplyInitialMove(new Move(false, 8, 10));
            ApplyInitialMove(new Move(true, 9, 9));
            ApplyInitialMove(new Move(false, 9, 9));
            ApplyInitialMove(new Move(true, 10, 8));
            ApplyInitialMove(new Move(false, 10, 8));
            ApplyInitialMove(new Move(true, 11, 7));
            ApplyInitialMove(new Move(false, 11, 7));
            ApplyInitialMove(new Move(true, 12, 6));

            ApplyInitialMove(new Move(false, 11, 6));
            ApplyInitialMove(new Move(true, 11, 5));
            ApplyInitialMove(new Move(false, 10, 5));
            ApplyInitialMove(new Move(true, 10, 4));
            ApplyInitialMove(new Move(false, 9, 4));
            ApplyInitialMove(new Move(true, 9, 3));
            ApplyInitialMove(new Move(false, 8, 3));
            ApplyInitialMove(new Move(true, 8, 2));
            ApplyInitialMove(new Move(false, 7, 2));
            ApplyInitialMove(new Move(true, 7, 1));
            ApplyInitialMove(new Move(false, 6, 1));

            ApplyInitialMove(new Move(true, 6, 1));
            ApplyInitialMove(new Move(false, 5, 2));
            ApplyInitialMove(new Move(true, 5, 2));
            ApplyInitialMove(new Move(false, 4, 3));
            ApplyInitialMove(new Move(true, 4, 3));
            ApplyInitialMove(new Move(false, 3, 4));
            ApplyInitialMove(new Move(true, 3, 4));
            ApplyInitialMove(new Move(false, 2, 5));
            ApplyInitialMove(new Move(true, 2, 5));
            ApplyInitialMove(new Move(false, 1, 6));
            ApplyInitialMove(new Move(true, 1, 6));

            ApplyInitialMove(new Move(true, 2, 6));
            ApplyInitialMove(new Move(false, 1, 6));
            ApplyInitialMove(new Move(false, 6, 11));
            ApplyInitialMove(new Move(true, 11, 6));
            ApplyInitialMove(new Move(false, 11, 7));
            ApplyInitialMove(new Move(false, 6, 2));
        }

        private void ApplyInitialMove(Move move)
        {
            _gameState.ApplyInitialMove(move);
            string key = move.IsHorizontal ? $"H_{move.Row}_{move.Col}" : $"V_{move.Row}_{move.Col}";
            _initialPatternLines.Add(key);
            _lineOwners[key] = Player.Human;
        }

        private void btnNewGame_Click(object? sender, EventArgs e)
        {
            StartNewGame();
        }

        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int totalWidth = 13 * GRID_SIZE;
            int totalHeight = 13 * GRID_SIZE;
            int offsetX = (this.ClientSize.Width - totalWidth) / 2;
            int offsetY = (this.ClientSize.Height - totalHeight) / 2 + 20;

            DrawGridBackground(g, offsetX, offsetY);
            DrawGameDots(g, offsetX, offsetY);
            DrawGameLines(g, offsetX, offsetY);

            if (_hoveredMove != null && !_aiThinking && _gameState.CurrentPlayer == Player.Human && IsLinePlayable(_hoveredMove))
            {
                DrawMove(g, _hoveredMove, _hoverPen, offsetX, offsetY);
            }

            DrawSquareOwners(g, offsetX, offsetY);
        }

        private void DrawGridBackground(Graphics g, int offsetX, int offsetY)
        {
            Pen gridPen = new Pen(Color.FromArgb(200, 220, 220, 220), 1);

            for (int i = 0; i < 14; i++)
            {
                int x = offsetX + i * GRID_SIZE;
                g.DrawLine(gridPen, x, offsetY, x, offsetY + 13 * GRID_SIZE);

                int y = offsetY + i * GRID_SIZE;
                g.DrawLine(gridPen, offsetX, y, offsetX + 13 * GRID_SIZE, y);
            }

            Brush dotBrush = new SolidBrush(Color.FromArgb(180, 200, 200, 200));
            int smallDotSize = 3;

            for (int row = 0; row < 14; row++)
            {
                for (int col = 0; col < 14; col++)
                {
                    int x = offsetX + col * GRID_SIZE;
                    int y = offsetY + row * GRID_SIZE;
                    g.FillEllipse(dotBrush, x - smallDotSize / 2, y - smallDotSize / 2, smallDotSize, smallDotSize);
                }
            }
        }

        private void DrawGameDots(Graphics g, int offsetX, int offsetY)
        {
            Brush gameDotBrush = new SolidBrush(Color.FromArgb(100, 100, 100));

            for (int row = 0; row <= GameState.BOARD_SIZE; row++)
            {
                for (int col = 0; col <= GameState.BOARD_SIZE; col++)
                {
                    Point p = GetDotPosition(row, col, offsetX, offsetY);
                    g.FillEllipse(gameDotBrush, p.X - DOT_SIZE / 2, p.Y - DOT_SIZE / 2, DOT_SIZE, DOT_SIZE);
                }
            }
        }

        private Point GetDotPosition(int row, int col, int offsetX, int offsetY)
        {
            int x = offsetX + col * GRID_SIZE;
            int y = offsetY + row * GRID_SIZE;
            return new Point(x, y);
        }

        private void DrawGameLines(Graphics g, int offsetX, int offsetY)
        {
            for (int r = 0; r <= GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c < GameState.BOARD_SIZE; c++)
                {
                    if (_gameState.HorizontalLines[r, c])
                    {
                        Point p1 = GetDotPosition(r, c, offsetX, offsetY);
                        Point p2 = GetDotPosition(r, c + 1, offsetX, offsetY);

                        string key = $"H_{r}_{c}";

                        Pen penToUse;
                        if (_initialPatternLines.Contains(key))
                        {
                            penToUse = _initialPatternPen;
                        }
                        else if (_lineOwners.ContainsKey(key) && _lineOwners[key] == Player.AI)
                        {
                            penToUse = _aiPen;
                        }
                        else
                        {
                            penToUse = _humanPen;
                        }

                        g.DrawLine(penToUse, p1, p2);
                    }
                }
            }

            for (int r = 0; r < GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c <= GameState.BOARD_SIZE; c++)
                {
                    if (_gameState.VerticalLines[r, c])
                    {
                        Point p1 = GetDotPosition(r, c, offsetX, offsetY);
                        Point p2 = GetDotPosition(r + 1, c, offsetX, offsetY);

                        string key = $"V_{r}_{c}";

                        Pen penToUse;
                        if (_initialPatternLines.Contains(key))
                        {
                            penToUse = _initialPatternPen;
                        }
                        else if (_lineOwners.ContainsKey(key) && _lineOwners[key] == Player.AI)
                        {
                            penToUse = _aiPen;
                        }
                        else
                        {
                            penToUse = _humanPen;
                        }

                        g.DrawLine(penToUse, p1, p2);
                    }
                }
            }
        }

        private void DrawSquareOwners(Graphics g, int offsetX, int offsetY)
        {
            var initialCornerSquares = new HashSet<(int, int)>
            {
                (1, 6), (6, 1), (6, 11), (11, 6)
            };

            for (int r = 0; r < GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c < GameState.BOARD_SIZE; c++)
                {
                    Player owner = _gameState.SquareOwners[r, c];
                    if (owner != Player.None)
                    {
                        Point p1 = GetDotPosition(r, c, offsetX, offsetY);
                        Point p2 = GetDotPosition(r, c + 1, offsetX, offsetY);
                        Point p3 = GetDotPosition(r + 1, c + 1, offsetX, offsetY);
                        Point p4 = GetDotPosition(r + 1, c, offsetX, offsetY);

                        Point[] square = { p1, p2, p3, p4 };

                        bool isInitialCorner = initialCornerSquares.Contains((r, c));

                        if (isInitialCorner)
                        {
                            Color fillColor = Color.FromArgb(60, 60, 60);
                            g.FillPolygon(new SolidBrush(fillColor), square);
                        }
                        else
                        {
                            Color fillColor;
                            string letter;

                            if (owner == Player.Human)
                            {
                                fillColor = Color.FromArgb(173, 216, 230);
                                letter = "H";
                            }
                            else
                            {
                                fillColor = Color.FromArgb(255, 160, 160);
                                letter = "IA";
                            }

                            g.FillPolygon(new SolidBrush(fillColor), square);

                            int centerX = (p1.X + p3.X) / 2;
                            int centerY = (p1.Y + p3.Y) / 2;

                            Font font = new Font("Arial", owner == Player.AI ? 12 : 16, FontStyle.Bold);
                            SizeF textSize = g.MeasureString(letter, font);

                            float textX = centerX - textSize.Width / 2;
                            float textY = centerY - textSize.Height / 2;

                            g.DrawString(letter, font, Brushes.White, textX, textY);
                        }
                    }
                }
            }
        }

        private void DrawMove(Graphics g, Move move, Pen pen, int offsetX, int offsetY)
        {
            if (move.IsHorizontal)
            {
                Point p1 = GetDotPosition(move.Row, move.Col, offsetX, offsetY);
                Point p2 = GetDotPosition(move.Row, move.Col + 1, offsetX, offsetY);
                g.DrawLine(pen, p1, p2);
            }
            else
            {
                Point p1 = GetDotPosition(move.Row, move.Col, offsetX, offsetY);
                Point p2 = GetDotPosition(move.Row + 1, move.Col, offsetX, offsetY);
                g.DrawLine(pen, p1, p2);
            }
        }

        private Move? GetMoveAtPosition(int mouseX, int mouseY)
        {
            const int TOLERANCE = 15;

            int totalWidth = 13 * GRID_SIZE;
            int totalHeight = 13 * GRID_SIZE;
            int offsetX = (this.ClientSize.Width - totalWidth) / 2;
            int offsetY = (this.ClientSize.Height - totalHeight) / 2 + 20;

            for (int r = 0; r <= GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c < GameState.BOARD_SIZE; c++)
                {
                    if (_gameState.HorizontalLines[r, c]) continue;

                    Move move = new Move(true, r, c);
                    if (!IsLinePlayable(move)) continue;

                    Point p1 = GetDotPosition(r, c, offsetX, offsetY);
                    Point p2 = GetDotPosition(r, c + 1, offsetX, offsetY);

                    if (IsPointNearLine(mouseX, mouseY, p1, p2, TOLERANCE))
                        return move;
                }
            }

            for (int r = 0; r < GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c <= GameState.BOARD_SIZE; c++)
                {
                    if (_gameState.VerticalLines[r, c]) continue;

                    Move move = new Move(false, r, c);
                    if (!IsLinePlayable(move)) continue;

                    Point p1 = GetDotPosition(r, c, offsetX, offsetY);
                    Point p2 = GetDotPosition(r + 1, c, offsetX, offsetY);

                    if (IsPointNearLine(mouseX, mouseY, p1, p2, TOLERANCE))
                        return move;
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
            if (_gameState.IsGameOver() || _aiThinking || _gameState.CurrentPlayer != Player.Human)
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
            if (_gameState.IsGameOver() || _aiThinking || _gameState.CurrentPlayer != Player.Human)
                return;

            Move? move = GetMoveAtPosition(e.X, e.Y);
            if (move == null) return;

            if (!IsLinePlayable(move))
                return;

            string key = move.IsHorizontal ? $"H_{move.Row}_{move.Col}" : $"V_{move.Row}_{move.Col}";
            _lineOwners[key] = Player.Human;

            _gameState.ApplyMove(move);

            UpdateStatus();
            this.Invalidate();

            if (_gameState.IsGameOver())
            {
                ShowGameOver();
                return;
            }

            if (_gameState.CurrentPlayer == Player.AI && !_gameState.IsGameOver())
            {
                await AITurn();
            }
        }

        private async Task AITurn()
        {
            _aiThinking = true;
            UpdateStatus();
            this.Invalidate();

            await Task.Delay(300);

            Move? aiMove = _aiLogic.GetBestMove(_gameState);

            if (aiMove != null)
            {
                string key = aiMove.IsHorizontal ? $"H_{aiMove.Row}_{aiMove.Col}" : $"V_{aiMove.Row}_{aiMove.Col}";
                _lineOwners[key] = Player.AI;

                _gameState.ApplyMove(aiMove);
            }

            _aiThinking = false;
            UpdateStatus();
            this.Invalidate();

            if (_gameState.IsGameOver())
            {
                ShowGameOver();
            }
        }

        private void UpdateStatus()
        {
            if (_aiThinking)
            {
                lblStatus.Text = "La IA está pensando...";
                lblStatus.ForeColor = Color.DarkOrange;
            }
            else if (_gameState.IsGameOver())
            {
                lblStatus.Text = $"¡Juego terminado! - H: {_gameState.HumanScore} | IA: {_gameState.AIScore}";
                lblStatus.ForeColor = Color.Black;
            }
            else if (_gameState.CurrentPlayer == Player.Human)
            {
                lblStatus.Text = $"Tu turno - H: {_gameState.HumanScore} | IA: {_gameState.AIScore}";
                lblStatus.ForeColor = Color.ForestGreen;
            }
            else
            {
                lblStatus.Text = $"Turno de la IA - H: {_gameState.HumanScore} | IA: {_gameState.AIScore}";
                lblStatus.ForeColor = Color.Crimson;
            }
        }

        private void ShowGameOver()
        {
            int totalSquares = 57;

            string winner;
            if (_gameState.HumanScore > _gameState.AIScore)
                winner = "¡Ganaste!";
            else if (_gameState.AIScore > _gameState.HumanScore)
                winner = "La IA ganó";
            else
                winner = "¡Empate!";

            MessageBox.Show($"{winner}\n\nPuntuación final:\nTú: {_gameState.HumanScore} cuadros\nIA: {_gameState.AIScore} cuadros\n\nTotal: {totalSquares} cuadros jugables",
                "Juego Terminado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
    }
}