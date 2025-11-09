using GalletaAI.Core;
using SEL;
using System.Drawing.Drawing2D;

namespace GalletaAI
{
    public partial class Form1 : Form
    {
        private GameState _gameState;
        private AILogic _aiLogic;

        // Configuración de gráficos
        private const int DOT_SIZE = 10;
        private const int LINE_WIDTH = 6;
        private const int CELL_SIZE = 80;
        private const int PADDING = 40;

        private Move? _hoverMove = null;

        public Form1()
        {
            InitializeComponent();

            // Usar doble búfer para evitar parpadeo
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);

            StartNewGame();
        }

        private void StartNewGame()
        {
            _gameState = new GameState();
            _aiLogic = new AILogic(depthBound: 7); // Profundidad de 7
            this.Text = "Juego de la Galleta - Turno: Humano";
            this.lblStatus.Text = "Humano: 0  |  IA: 0";
            this.Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.WhiteSmoke);

            DrawBoard(g);
        }

        private void DrawBoard(Graphics g)
        {
            int boardPixelSize = GameState.BOARD_SIZE * CELL_SIZE;

            // Dibujar cuadros completados
            for (int r = 0; r < GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c < GameState.BOARD_SIZE; c++)
                {
                    if (_gameState.SquareOwners[r, c] != Player.None)
                    {
                        Color color = (_gameState.SquareOwners[r, c] == Player.Human) ?
                            Color.FromArgb(150, 200, 255) : Color.FromArgb(150, 255, 200);
                        g.FillRectangle(new SolidBrush(color),
                            PADDING + c * CELL_SIZE,
                            PADDING + r * CELL_SIZE,
                            CELL_SIZE, CELL_SIZE);
                    }
                }
            }

            // Dibujar líneas
            DrawLines(g, _gameState.HorizontalLines, true);
            DrawLines(g, _gameState.VerticalLines, false);

            // Dibujar línea "hover"
            if (_hoverMove != null && _gameState.CurrentPlayer == Player.Human)
            {
                DrawLine(g, _hoverMove, Color.FromArgb(100, Color.LightGray));
            }

            // Dibujar puntos (como en la imagen que subiste)
            for (int r = 0; r <= GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c <= GameState.BOARD_SIZE; c++)
                {
                    g.FillEllipse(Brushes.Black,
                        PADDING + c * CELL_SIZE - DOT_SIZE / 2,
                        PADDING + r * CELL_SIZE - DOT_SIZE / 2,
                        DOT_SIZE, DOT_SIZE);
                }
            }
        }

        private void DrawLines(Graphics g, bool[,] lines, bool isHorizontal)
        {
            using (Pen pen = new Pen(Color.Black, LINE_WIDTH))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                int rows = lines.GetLength(0);
                int cols = lines.GetLength(1);

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (lines[r, c])
                        {
                            DrawLine(g, new Move(isHorizontal, r, c), pen);
                        }
                    }
                }
            }
        }

        private void DrawLine(Graphics g, Move move, Pen pen)
        {
            int x1, y1, x2, y2;
            if (move.IsHorizontal)
            {
                x1 = PADDING + move.Col * CELL_SIZE;
                y1 = PADDING + move.Row * CELL_SIZE;
                x2 = x1 + CELL_SIZE;
                y2 = y1;
            }
            else
            {
                x1 = PADDING + move.Col * CELL_SIZE;
                y1 = PADDING + move.Row * CELL_SIZE;
                x2 = x1;
                y2 = y1 + CELL_SIZE;
            }
            g.DrawLine(pen, x1, y1, x2, y2);
        }
        private void DrawLine(Graphics g, Move move, Color color)
        {
            using (Pen pen = new Pen(color, LINE_WIDTH))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                DrawLine(g, move, pen);
            }
        }


        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (_gameState.CurrentPlayer != Player.Human || _gameState.IsGameOver())
                return;

            Move? clickedMove = GetMoveFromMousePos(e.Location);

            if (clickedMove != null)
            {
                // El jugador hizo un movimiento válido
                _hoverMove = null;
                ExecuteMove(clickedMove);
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_gameState.CurrentPlayer != Player.Human)
            {
                _hoverMove = null;
                return;
            }

            Move? newHoverMove = GetMoveFromMousePos(e.Location);

            if (_hoverMove == null || !_hoverMove.Equals(newHoverMove))
            {
                _hoverMove = newHoverMove;
                this.Refresh();
            }
        }

        // Lógica para determinar en qué línea se hizo clic
        private Move? GetMoveFromMousePos(Point mousePos)
        {
            int tolerance = 10;

            // Revisar líneas horizontales
            for (int r = 0; r <= GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c < GameState.BOARD_SIZE; c++)
                {
                    if (_gameState.HorizontalLines[r, c]) continue; // Ya dibujada

                    int y = PADDING + r * CELL_SIZE;
                    int x1 = PADDING + c * CELL_SIZE;
                    int x2 = x1 + CELL_SIZE;

                    if (mousePos.Y >= y - tolerance && mousePos.Y <= y + tolerance &&
                        mousePos.X >= x1 && mousePos.X <= x2)
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
                    if (_gameState.VerticalLines[r, c]) continue; // Ya dibujada

                    int x = PADDING + c * CELL_SIZE;
                    int y1 = PADDING + r * CELL_SIZE;
                    int y2 = y1 + CELL_SIZE;

                    if (mousePos.X >= x - tolerance && mousePos.X <= x + tolerance &&
                        mousePos.Y >= y1 && mousePos.Y <= y2)
                    {
                        return new Move(false, r, c);
                    }
                }
            }
            return null;
        }

        private async void ExecuteMove(Move move)
        {
            Player nextPlayer = _gameState.ApplyMove(move);
            UpdateUI();

            if (nextPlayer == Player.Human)
            {
                // Humano tiene turno extra
                return;
            }

            // Es turno de la IA
            this.Text = "Juego de la Galleta - Turno: IA (Pensando...)";
            this.Cursor = Cursors.WaitCursor;

            while (nextPlayer == Player.AI && !_gameState.IsGameOver())
            {
                // La IA piensa en un hilo separado para no congelar la UI
                Move? aiMove = await Task.Run(() => _aiLogic.FindBestMove(new GameState(_gameState)));

                if (aiMove != null)
                {
                    nextPlayer = _gameState.ApplyMove(aiMove);
                    UpdateUI();
                    // Pequeña pausa para ver el movimiento de la IA
                    await Task.Delay(500);
                }
                else
                {
                    break; // No hay más movimientos
                }
            }

            this.Cursor = Cursors.Default;

            if (_gameState.IsGameOver())
            {
                string winner = (_gameState.HumanScore > _gameState.AIScore) ? "Humano" : "IA";
                if (_gameState.HumanScore == _gameState.AIScore) winner = "Empate";
                this.Text = $"Juego Terminado - Ganador: {winner}";
            }
            else
            {
                this.Text = "Juego de la Galleta - Turno: Humano";
            }
        }

        private void UpdateUI()
        {
            lblStatus.Text = $"Humano: {_gameState.HumanScore}  |  IA: {_gameState.AIScore}";
            this.Refresh();
        }

        private void btnNewGame_Click(object sender, EventArgs e)
        {
            StartNewGame();
        }
    }
}
