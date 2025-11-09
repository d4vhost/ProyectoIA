// Archivo: OchoPuzzle/Form1.cs
using OchoPuzzle.Core;
using SEL;
using System.Drawing;

namespace OchoPuzzle
{
    public partial class Form1 : Form
    {
        private Button[,] _buttons = new Button[SqNode.width, SqNode.width];
        private int[,] _board = new int[SqNode.width, SqNode.width];
        private Point _zeroPos;

        public Form1()
        {
            InitializeComponent();
            InitializeBoard();
            UpdateUI();
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

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < SqNode.width; i++)
            {
                for (int j = 0; j < SqNode.width; j++)
                {
                    Button btn = new Button();
                    btn.Size = new Size(80, 80);
                    btn.Location = new Point(j * 80 + 20, i * 80 + 20);
                    btn.Font = new Font("Arial", 24, FontStyle.Bold);
                    btn.Tag = new Point(i, j);
                    btn.Click += Button_Click;

                    this.Controls.Add(btn);
                    _buttons[i, j] = btn;
                }
            }

            // Añadir los botones de control
            this.btnSolve.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnSolve.Location = new System.Drawing.Point(20, 270);
            this.btnSolve.Name = "btnSolve";
            this.btnSolve.Size = new System.Drawing.Size(115, 40);
            this.btnSolve.Text = "Resolver (A*)";
            this.btnSolve.UseVisualStyleBackColor = true;
            this.btnSolve.Click += new System.EventHandler(this.btnSolve_Click);

            this.btnShuffle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnShuffle.Location = new System.Drawing.Point(145, 270);
            this.btnShuffle.Name = "btnShuffle";
            this.btnShuffle.Size = new System.Drawing.Size(115, 40);
            this.btnShuffle.Text = "Desordenar";
            this.btnShuffle.UseVisualStyleBackColor = true;
            this.btnShuffle.Click += new System.EventHandler(this.btnShuffle_Click);

            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(20, 325);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Text = "Listo.";

            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnShuffle);
            this.Controls.Add(this.btnSolve);

            this.ClientSize = new System.Drawing.Size(284, 361);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Text = "8-Puzzle Solver";

            UpdateUI();
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            Point pos = (Point)btn.Tag;

            if (Math.Abs(pos.X - _zeroPos.X) + Math.Abs(pos.Y - _zeroPos.Y) == 1)
            {
                _board[_zeroPos.X, _zeroPos.Y] = _board[pos.X, pos.Y];
                _board[pos.X, pos.Y] = 0;
                _zeroPos = pos;

                UpdateUI();

                if (IsSolved())
                {
                    MessageBox.Show("¡Felicidades, has ganado!");
                }
            }
        }

        private async void btnSolve_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            lblStatus.Text = "Calculando solución A*...";

            SqPuzzle solver = new SqPuzzle((int[,])_board.Clone(), _zeroPos);

            List<SqNode> solutionPath = await Task.Run(() => solver.solve());

            if (solutionPath.Count == 0)
            {
                lblStatus.Text = "No se encontró solución.";
                MessageBox.Show("No se pudo encontrar una solución con el algoritmo A*.");
                this.Enabled = true;
                return;
            }

            lblStatus.Text = $"Solución en {solutionPath.Count - 1} movimientos. (Nodos: {solver.nodesSearched})";

            foreach (var node in solutionPath)
            {
                _board = node.position;
                _zeroPos = node.zero;
                UpdateUI();
                await Task.Delay(300);
            }

            MessageBox.Show("¡Solución A* completada!");
            this.Enabled = true;
        }

        private void btnShuffle_Click(object sender, EventArgs e)
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
            lblStatus.Text = "Tablero desordenado. ¡Listo!";
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