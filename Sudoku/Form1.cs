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
        private TextBox[,] cells = new TextBox[9, 9];
        private Button btnSolve = null!;
        private Button btnNew = null!;
        private Label lblTitle = null!;
        private Panel gridPanel = null!;
        private SudokuSolver solver = null!;
        private bool isSolving = false;
        private bool isLoadingBoard = false;

        // CONFIGURACIÓN
        private const int CELL_SIZE = 54;
        private const int PANEL_PADDING = 3; // Aumenté a 3 para asegurar borde visible
        private const int GAP = 1;
        private const int BLOCK_GAP = 2;

        public Form1()
        {
            InitializeComponent();
            solver = new SudokuSolver();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Sudoku 9x9 Solver";
            this.Size = new Size(620, 780);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Título
            lblTitle = new Label
            {
                Text = "SUDOKU",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = false,
                Size = new Size(600, 70),
                Location = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            // --- CORRECCIÓN DEL BORDE ---
            // Calculamos el ancho exacto del contenido interno
            int contentSize = (CELL_SIZE * 9) + (GAP * 6) + (BLOCK_GAP * 2);

            // El tamaño del panel será: Contenido + Padding arriba + Padding abajo
            int panelWidth = contentSize + (PANEL_PADDING * 2);

            // AQUÍ EL TRUCO: Le sumamos 4 píxeles extra a la altura para garantizar 
            // que el borde inferior se vea grueso y no se corte.
            int panelHeight = panelWidth + 4;

            gridPanel = new Panel
            {
                Size = new Size(panelWidth, panelHeight),
                // Centramos el panel
                Location = new Point((620 - panelWidth) / 2 - 8, 90),
                BackColor = Color.FromArgb(44, 62, 80), // Color del borde oscuro
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(gridPanel);

            CreateSudokuGrid();

            // Botones alineados al ancho del grid
            int btnWidth = (panelWidth - 10) / 2;
            int startX = gridPanel.Location.X;
            int btnY = gridPanel.Location.Y + panelHeight + 15; // Separación respecto al panel

            btnSolve = new Button
            {
                Text = "RESOLVER",
                Size = new Size(btnWidth, 55),
                Location = new Point(startX, btnY),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(46, 204, 113),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSolve.FlatAppearance.BorderSize = 0;
            btnSolve.Click += BtnSolve_Click;
            this.Controls.Add(btnSolve);

            btnNew = new Button
            {
                Text = "NUEVO",
                Size = new Size(btnWidth, 55),
                Location = new Point(startX + btnWidth + 10, btnY),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(52, 152, 219),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnNew.FlatAppearance.BorderSize = 0;
            btnNew.Click += BtnNew_Click;
            this.Controls.Add(btnNew);

            LoadInitialBoard();
            this.ActiveControl = null;
        }

        private void CreateSudokuGrid()
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    int x = PANEL_PADDING + (col * CELL_SIZE) + (col * GAP) + ((col / 3) * (BLOCK_GAP - GAP));
                    int y = PANEL_PADDING + (row * CELL_SIZE) + (row * GAP) + ((row / 3) * (BLOCK_GAP - GAP));

                    TextBox cell = new TextBox
                    {
                        Size = new Size(CELL_SIZE, CELL_SIZE),
                        Location = new Point(x, y),
                        Font = new Font("Segoe UI", 24, FontStyle.Bold),
                        TextAlign = HorizontalAlignment.Center,
                        MaxLength = 1,
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.White,
                        ForeColor = Color.FromArgb(44, 62, 80),
                        TabStop = false,
                        Tag = new Point(row, col)
                    };

                    cell.KeyPress += (s, e) =>
                    {
                        if (!char.IsControl(e.KeyChar) && (e.KeyChar < '1' || e.KeyChar > '9'))
                            e.Handled = true;
                    };

                    cell.TextChanged += Cell_TextChanged;

                    cells[row, col] = cell;
                    gridPanel.Controls.Add(cell);
                }
            }
        }

        private void LoadInitialBoard()
        {
            isLoadingBoard = true;
            int[,] board = solver.InitialBoard;

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
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
            isLoadingBoard = false;
        }

        private void Cell_TextChanged(object? sender, EventArgs e)
        {
            if (isLoadingBoard || isSolving) return;
            CheckIfUserFinished();
        }

        // --- LÓGICA CORREGIDA: ANALIZAR SOLO AL LLENAR TODO ---
        private void CheckIfUserFinished()
        {
            int[,] currentBoard = new int[9, 9];
            bool isFull = true;

            // 1. Verificar primero si el tablero está LLENO
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    string val = cells[r, c].Text;

                    // Si hay al menos una celda vacía, NO hacemos nada, el usuario sigue jugando
                    if (string.IsNullOrEmpty(val))
                    {
                        isFull = false;
                        break; // Salimos del loop interno
                    }

                    if (int.TryParse(val, out int num))
                        currentBoard[r, c] = num;
                    else
                        return; // Error de parseo raro
                }
                if (!isFull) break; // Salimos del loop externo
            }

            // Si no está lleno, retornamos y dejamos jugar
            if (!isFull) return;

            // 2. Si está lleno, validamos si es correcto o incorrecto
            if (IsValidSudoku(currentBoard))
            {
                MessageBox.Show("¡Felicidades! Has completado el Sudoku correctamente.",
                                "¡ÉXITO!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Mensaje suave indicando que está lleno pero algo está mal
                MessageBox.Show("El tablero está completo, pero algunas casillas no son correctas.\n\nRevisa filas, columnas o cuadrantes repetidos.",
                                "Revisar Sudoku", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool IsValidSudoku(int[,] board)
        {
            for (int i = 0; i < 9; i++)
            {
                if (!IsValidGroup(board, i, 0, 0, 1)) return false;
                if (!IsValidGroup(board, 0, i, 1, 0)) return false;
            }
            for (int r = 0; r < 9; r += 3)
            {
                for (int c = 0; c < 9; c += 3)
                {
                    if (!IsValidBlock(board, r, c)) return false;
                }
            }
            return true;
        }

        private bool IsValidGroup(int[,] board, int startRow, int startCol, int dRow, int dCol)
        {
            bool[] seen = new bool[10];
            for (int k = 0; k < 9; k++)
            {
                int val = board[startRow + k * dRow, startCol + k * dCol];
                if (val < 1 || val > 9 || seen[val]) return false;
                seen[val] = true;
            }
            return true;
        }

        private bool IsValidBlock(int[,] board, int startRow, int startCol)
        {
            bool[] seen = new bool[10];
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    int val = board[startRow + r, startCol + c];
                    if (val < 1 || val > 9 || seen[val]) return false;
                    seen[val] = true;
                }
            }
            return true;
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
                                cells[row, col].Refresh();
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
                    for (int r = 0; r < 9; r++)
                    {
                        for (int c = 0; c < 9; c++)
                        {
                            if (!cells[r, c].ReadOnly)
                            {
                                cells[r, c].Text = solution.Board[r, c].ToString();
                                cells[r, c].BackColor = Color.FromArgb(212, 239, 223);
                            }
                        }
                    }

                    await Task.Delay(500);

                    MessageBox.Show("¡Sudoku resuelto por la IA!", "Completado",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No se encontró solución.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            this.ActiveControl = null;
        }
    }
}