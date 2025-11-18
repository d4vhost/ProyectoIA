// Archivo: Sudoku/Form1.cs
using System;
using System.Drawing;
using System.Windows.Forms;
using Sudoku.Core;
using System.Threading.Tasks;

namespace Sudoku
{
    public partial class Form1 : Form
    {
        private TextBox[,] cells = new TextBox[6, 6];
        private Button btnSolve = null!;
        private Button btnNew = null!;
        private Label lblTitle = null!;
        private Panel gridPanel = null!;
        private SudokuSolver solver = null!;
        private bool isSolving = false;

        public Form1()
        {
            InitializeComponent();
            solver = new SudokuSolver();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Sudoku 6x6 Solver";
            this.Size = new Size(620, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Título
            lblTitle = new Label
            {
                Text = "SUDOKU",
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = false,
                Size = new Size(540, 60),
                Location = new Point(20, 15),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            // Panel contenedor del grid
            gridPanel = new Panel
            {
                Size = new Size(480, 480),
                Location = new Point(50, 85),
                BackColor = Color.FromArgb(44, 62, 80),
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(gridPanel);

            CreateSudokuGrid();

            // Botón RESOLVER (izquierda)
            btnSolve = new Button
            {
                Text = "RESOLVER",
                Size = new Size(250, 55),
                Location = new Point(50, 590),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(46, 204, 113),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSolve.FlatAppearance.BorderSize = 0;
            btnSolve.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 174, 96);
            btnSolve.Click += BtnSolve_Click;
            this.Controls.Add(btnSolve);

            // Botón NUEVO (derecha)
            btnNew = new Button
            {
                Text = "NUEVO",
                Size = new Size(250, 55),
                Location = new Point(310, 590),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(52, 152, 219),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnNew.FlatAppearance.BorderSize = 0;
            btnNew.FlatAppearance.MouseOverBackColor = Color.FromArgb(41, 128, 185);
            btnNew.Click += BtnNew_Click;
            this.Controls.Add(btnNew);

            LoadInitialBoard();

            // Quitar el foco de todos los TextBox al iniciar
            this.ActiveControl = null;
        }

        private void CreateSudokuGrid()
        {
            int cellSize = 78;
            int gap = 1;
            int bigGap = 3;

            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    int x = col * (cellSize + gap) + (col / 3) * (bigGap - gap) + 5;
                    int y = row * (cellSize + gap) + (row / 2) * (bigGap - gap) + 5;

                    TextBox cell = new TextBox
                    {
                        Size = new Size(cellSize, cellSize),
                        Location = new Point(x, y),
                        Font = new Font("Segoe UI", 32, FontStyle.Bold),
                        TextAlign = HorizontalAlignment.Center,
                        MaxLength = 1,
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.White,
                        ForeColor = Color.FromArgb(44, 62, 80),
                        TabStop = false
                    };

                    cell.KeyPress += (s, e) =>
                    {
                        if (!char.IsControl(e.KeyChar) && (e.KeyChar < '1' || e.KeyChar > '6'))
                            e.Handled = true;
                    };

                    cells[row, col] = cell;
                    gridPanel.Controls.Add(cell);
                }
            }
        }

        private void LoadInitialBoard()
        {
            int[,] board = solver.InitialBoard;

            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 6; col++)
                {
                    if (board[row, col] != 0)
                    {
                        cells[row, col].Text = board[row, col].ToString();
                        cells[row, col].ReadOnly = true;
                        cells[row, col].BackColor = Color.FromArgb(236, 240, 241);
                        cells[row, col].ForeColor = Color.FromArgb(52, 73, 94);
                    }
                    else
                    {
                        cells[row, col].Text = "";
                        cells[row, col].ReadOnly = false;
                        cells[row, col].BackColor = Color.White;
                        cells[row, col].ForeColor = Color.FromArgb(41, 128, 185);
                    }
                }
            }
        }

        private async void BtnSolve_Click(object? sender, EventArgs e)
        {
            if (isSolving) return;

            isSolving = true;
            btnSolve.Enabled = false;
            btnNew.Enabled = false;

            SudokuNode.OnCellUpdate = (row, col, value) =>
            {
                if (this.IsDisposed) return;

                try
                {
                    if (this.InvokeRequired)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            if (!this.IsDisposed && !cells[row, col].ReadOnly)
                            {
                                cells[row, col].Text = value > 0 ? value.ToString() : "";
                                Application.DoEvents();
                            }
                        }));
                    }
                }
                catch { }
            };

            try
            {
                SudokuNode? solution = await Task.Run(() => solver.Solve());

                if (this.IsDisposed) return;

                SudokuNode.OnCellUpdate = null;

                if (solution != null)
                {
                    for (int r = 0; r < 6; r++)
                    {
                        for (int c = 0; c < 6; c++)
                        {
                            if (!cells[r, c].ReadOnly)
                            {
                                cells[r, c].Text = solution.Board[r, c].ToString();
                                cells[r, c].BackColor = Color.FromArgb(212, 239, 223);
                            }
                        }
                    }

                    await Task.Delay(300);

                    for (int r = 0; r < 6; r++)
                    {
                        for (int c = 0; c < 6; c++)
                        {
                            if (!cells[r, c].ReadOnly)
                            {
                                cells[r, c].BackColor = Color.White;
                            }
                        }
                    }

                    MessageBox.Show("¡Sudoku resuelto correctamente!", "Completado",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Limpiar solo las celdas editables después del mensaje
                    for (int r = 0; r < 6; r++)
                    {
                        for (int c = 0; c < 6; c++)
                        {
                            if (!cells[r, c].ReadOnly)
                            {
                                cells[r, c].Text = "";
                                cells[r, c].BackColor = Color.White;
                            }
                        }
                    }

                    this.ActiveControl = null;
                }
            }
            catch { }
            finally
            {
                if (!this.IsDisposed)
                {
                    isSolving = false;
                    btnSolve.Enabled = true;
                    btnNew.Enabled = true;
                    SudokuNode.OnCellUpdate = null;
                }
            }
        }

        private void BtnNew_Click(object? sender, EventArgs e)
        {
            if (isSolving) return;

            solver = new SudokuSolver();
            LoadInitialBoard();

            // Quitar el foco después de generar nuevo juego
            this.ActiveControl = null;
        }

        private void Form1_Load(object? sender, EventArgs e) { }
    }
}