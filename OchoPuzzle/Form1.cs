using OchoPuzzle.Core;
using SEL;
using System.Drawing;
using System.Windows.Forms;

namespace OchoPuzzle
{
    public partial class Form1 : Form
    {
        private Button[,] _buttons = new Button[SqNode.width, SqNode.width];
        private int[,] _board = new int[SqNode.width, SqNode.width];
        private Point _zeroPos;
        private Panel gamePanel = null!;
        private Label lblTitle = null!;

        public Form1()
        {
            InitializeComponent();
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            int val = 1;
            for (int i = 0; i < SqNode.width; i++)
            {
                for (int j = 0; j < SqNode.width; j++)
                {
                    _board[i, j] = val;
                    val++;
                }
            }
            _board[SqNode.width - 1, SqNode.width - 1] = 0;
            _zeroPos = new Point(SqNode.width - 1, SqNode.width - 1);
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            // --- 1. AJUSTE DE TAMAÑO ---
            this.Text = "8-Puzzle Solver";
            this.BackColor = Color.FromArgb(245, 247, 250);

            // Reduje la altura a 760 para asegurar que entre en pantallas de laptops
            this.ClientSize = new Size(540, 760);

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            // StartPosition aquí a veces falla si cambiamos el tamaño,
            // por eso usamos CenterToScreen() al final.
            this.StartPosition = FormStartPosition.CenterScreen;

            // --- 2. TÍTULO ---
            lblTitle = new Label
            {
                Text = "8-PUZZLE",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = false,
                Size = new Size(500, 70),
                Location = new Point(20, 15), // Subí un poco el título
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblTitle);

            // --- 3. PANEL DEL JUEGO ---
            int panelSize = 400;
            gamePanel = new Panel
            {
                Size = new Size(panelSize, panelSize),
                Location = new Point((this.ClientSize.Width - panelSize) / 2, 90), // Subí un poco el panel
                BackColor = Color.FromArgb(44, 62, 80),
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(gamePanel);

            // --- 4. BOTONES (FICHAS) ---
            int btnSize = 125;
            int gap = 5;
            int offset = 5;

            for (int i = 0; i < SqNode.width; i++)
            {
                for (int j = 0; j < SqNode.width; j++)
                {
                    Button btn = new Button
                    {
                        Size = new Size(btnSize, btnSize),
                        Location = new Point(j * (btnSize + gap) + offset, i * (btnSize + gap) + offset),
                        Font = new Font("Segoe UI", 48, FontStyle.Bold),
                        Tag = new Point(i, j),
                        BackColor = Color.White,
                        ForeColor = Color.FromArgb(52, 73, 94),
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand
                    };

                    btn.FlatAppearance.BorderSize = 0;
                    btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(236, 240, 241);
                    btn.Click += Button_Click;

                    gamePanel.Controls.Add(btn);
                    _buttons[i, j] = btn;
                }
            }

            // --- 5. CONTROLES INFERIORES ---

            // Label de estado
            this.lblStatus.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            this.lblStatus.ForeColor = Color.FromArgb(52, 73, 94);
            this.lblStatus.AutoSize = false;
            this.lblStatus.Size = new Size(500, 40);
            this.lblStatus.Location = new Point(20, 500);
            this.lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            this.lblStatus.Text = "¡Listo para jugar!";
            this.lblStatus.BackColor = Color.Transparent;

            // Botones de acción
            int btnActionWidth = 200;
            int btnHeight = 60;
            int spaceBetween = 40;
            int startX = (this.ClientSize.Width - (btnActionWidth * 2 + spaceBetween)) / 2;

            // Botón Desordenar
            this.btnShuffle.Text = "DESORDENAR";
            this.btnShuffle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            this.btnShuffle.Size = new Size(btnActionWidth, btnHeight);
            this.btnShuffle.Location = new Point(startX, 550); // Subido a 550
            this.btnShuffle.ForeColor = Color.White;
            this.btnShuffle.BackColor = Color.FromArgb(52, 152, 219);
            this.btnShuffle.FlatStyle = FlatStyle.Flat;
            this.btnShuffle.FlatAppearance.BorderSize = 0;
            this.btnShuffle.Cursor = Cursors.Hand;
            this.btnShuffle.Click += btnShuffle_Click;

            // Botón Resolver
            this.btnSolve.Text = "RESOLVER (A*)";
            this.btnSolve.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            this.btnSolve.Size = new Size(btnActionWidth, btnHeight);
            this.btnSolve.Location = new Point(startX + btnActionWidth + spaceBetween, 550); // Subido a 550
            this.btnSolve.ForeColor = Color.White;
            this.btnSolve.BackColor = Color.FromArgb(46, 204, 113);
            this.btnSolve.FlatStyle = FlatStyle.Flat;
            this.btnSolve.FlatAppearance.BorderSize = 0;
            this.btnSolve.Cursor = Cursors.Hand;
            this.btnSolve.Click += btnSolve_Click;

            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnShuffle);
            this.Controls.Add(this.btnSolve);

            UpdateUI();

            // === TRUCO IMPORTANTE ===
            // Esto obliga al formulario a recalcular su posición central
            // basándose en el tamaño final que acabamos de configurar.
            this.CenterToScreen();
        }

        // --- MÉTODOS DE LÓGICA SIN CAMBIOS ---

        private void Button_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not Point pos) return;

            if (Math.Abs(pos.X - _zeroPos.X) + Math.Abs(pos.Y - _zeroPos.Y) == 1)
            {
                _board[_zeroPos.X, _zeroPos.Y] = _board[pos.X, pos.Y];
                _board[pos.X, pos.Y] = 0;
                _zeroPos = pos;

                UpdateUI();

                if (IsSolved())
                {
                    lblStatus.Text = "🎉 ¡Felicidades, resolviste el puzzle!";
                    lblStatus.ForeColor = Color.FromArgb(46, 204, 113);
                    MessageBox.Show("¡Felicidades, has ganado!", "¡Victoria!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private async void btnSolve_Click(object? sender, EventArgs e)
        {
            if (IsSolved())
            {
                MessageBox.Show("El puzzle ya está resuelto.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            this.btnSolve.Enabled = false;
            this.btnShuffle.Enabled = false;
            lblStatus.Text = "🤖 Calculando solución con A*...";
            lblStatus.ForeColor = Color.FromArgb(243, 156, 18);

            SqPuzzle solver = new SqPuzzle((int[,])_board.Clone(), _zeroPos);

            List<SqNode> solutionPath = await Task.Run(() => solver.solve());

            if (solutionPath.Count == 0)
            {
                lblStatus.Text = "❌ No se encontró solución";
                lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
                MessageBox.Show("No se pudo encontrar una solución con el algoritmo A*.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.btnSolve.Enabled = true;
                this.btnShuffle.Enabled = true;
                return;
            }

            lblStatus.Text = $"✓ Solución en {solutionPath.Count - 1} movimientos";
            lblStatus.ForeColor = Color.FromArgb(46, 204, 113);

            await Task.Delay(500);

            foreach (var node in solutionPath)
            {
                _board = node.position;
                _zeroPos = node.zero;
                UpdateUI();
                await Task.Delay(300);
            }

            MessageBox.Show($"¡Solución A* completada!\n\nMovimientos realizados: {solutionPath.Count - 1}",
                "Solución Completada", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.btnSolve.Enabled = true;
            this.btnShuffle.Enabled = true;
        }

        private void btnShuffle_Click(object? sender, EventArgs e)
        {
            Random rand = new Random();
            for (int i = 0; i < 100; i++)
            {
                SqNode tempNode = new SqNode(_board, null, _zeroPos);
                List<Point> moves = tempNode.generateMoves();
                Point randomMove = moves[rand.Next(moves.Count)];

                _board = tempNode.makeMove(randomMove);
                _zeroPos = randomMove;
            }
            UpdateUI();
            lblStatus.Text = "🎲 Tablero desordenado. ¡A jugar!";
            lblStatus.ForeColor = Color.FromArgb(52, 152, 219);
        }

        private void UpdateUI()
        {
            for (int i = 0; i < SqNode.width; i++)
            {
                for (int j = 0; j < SqNode.width; j++)
                {
                    int val = _board[i, j];
                    _buttons[i, j].Text = val == 0 ? "" : val.ToString();
                    _buttons[i, j].Visible = (val != 0);

                    if (val != 0)
                    {
                        _buttons[i, j].BackColor = Color.White;
                        _buttons[i, j].ForeColor = Color.FromArgb(52, 73, 94);
                    }
                }
            }
        }

        private bool IsSolved()
        {
            int val = 1;
            for (int i = 0; i < SqNode.width; i++)
            {
                for (int j = 0; j < SqNode.width; j++)
                {
                    if (i == SqNode.width - 1 && j == SqNode.width - 1)
                        return _board[i, j] == 0;
                    if (_board[i, j] != val)
                        return false;
                    val++;
                }
            }
            return true;
        }
    }
}