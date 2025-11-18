// Archivo: Form1.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sudoku.Core;

namespace Sudoku
{
    public partial class Form1 : Form
    {
        // SOLUCIÓN A ERRORES DE ÁMBITO: Declaración explícita de los controles UI
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnSolve;

        private SudokuSolver _solver = new SudokuSolver();
        private TextBox[,] _cells = new TextBox[9, 9];

        public Form1()
        {
            InitializeComponent();

            // Inicialización manual de los controles (asegura que no sean null)
            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.btnSolve = new Button();
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.btnSolve);

            InitializeSudokuGrid();
            LoadInitialBoard();

            // Conexión del evento del botón
            this.btnSolve.Click += new System.EventHandler(this.btnSolve_Click);
        }

        // --- Inicialización y Carga de la Interfaz ---
        private void InitializeSudokuGrid()
        {
            // Configuración del TableLayoutPanel
            tableLayoutPanel1.Dock = DockStyle.None;
            tableLayoutPanel1.Location = new Point(10, 10);
            tableLayoutPanel1.Size = new Size(500, 500);
            tableLayoutPanel1.RowCount = 9;
            tableLayoutPanel1.ColumnCount = 9;

            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.SuspendLayout();

            // Ajustar estilos de filas y columnas para celdas cuadradas
            for (int i = 0; i < 9; i++)
            {
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 11.11f));
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 11.11f));
            }

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    TextBox cell = new TextBox
                    {
                        Tag = new Point(r, c),
                        Text = "",
                        TextAlign = HorizontalAlignment.Center,
                        Font = new Font("Arial", 14, FontStyle.Bold),
                        Margin = new Padding(1),
                        Dock = DockStyle.Fill
                    };

                    // Separadores visuales para bloques 3x3
                    if (r % 3 == 0 && r != 0) cell.BorderStyle = BorderStyle.Fixed3D;
                    if (c % 3 == 0 && c != 0) cell.BorderStyle = BorderStyle.Fixed3D;

                    _cells[r, c] = cell;
                    tableLayoutPanel1.Controls.Add(cell, c, r);
                    cell.Validating += new System.ComponentModel.CancelEventHandler(this.cell_Validating);
                }
            }

            tableLayoutPanel1.ResumeLayout(false);

            // Configuración del botón
            btnSolve.Text = "Resolver Sudoku (IA)";
            btnSolve.Dock = DockStyle.None;
            btnSolve.Location = new Point(10, tableLayoutPanel1.Bottom + 20);
            btnSolve.Size = new Size(180, 40);
        }

        private void LoadInitialBoard()
        {
            int[,] board = _solver.InitialBoard;
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (board[r, c] != 0)
                    {
                        _cells[r, c].Text = board[r, c].ToString();
                        _cells[r, c].ReadOnly = true;
                        _cells[r, c].BackColor = Color.LightGray;
                    }
                    else
                    {
                        _cells[r, c].Text = "";
                        _cells[r, c].ReadOnly = false;
                        _cells[r, c].BackColor = Color.White;
                    }
                }
            }
        }

        // --- Requisito b: Resolver automáticamente (Advertencia de nulidad corregida) ---
        private async void btnSolve_Click(object? sender, EventArgs e)
        {
            btnSolve.Enabled = false;

            // Ejecuta el DFS en un hilo de fondo
            SudokuNode? solutionNode = await Task.Run(() => _solver.Solve());

            if (solutionNode != null)
            {
                List<SudokuSolver.Move> solutionPath = _solver.GetSolutionPath(solutionNode);
                await DisplaySolutionStepByStep(solutionPath);
            }
            else
            {
                MessageBox.Show("No se encontró solución para este tablero.", "Error");
            }

            btnSolve.Enabled = true;
        }

        private async Task DisplaySolutionStepByStep(List<SudokuSolver.Move> path)
        {
            // Pausa de 50ms para ver el proceso (requisito: pausar para mirar)
            const int StepDelay = 50;

            LoadInitialBoard();

            foreach (var move in path)
            {
                _cells[move.R, move.C].Text = move.Value.ToString();
                _cells[move.R, move.C].BackColor = Color.LightGreen;

                // Application.DoEvents() y Task.Delay: esenciales para la animación
                Application.DoEvents();
                await Task.Delay(StepDelay);
            }

            // Marcar celdas resueltas al finalizar
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (!_cells[r, c].ReadOnly)
                        _cells[r, c].BackColor = Color.Yellow;
                }
            }
        }

        // --- Requisito a: Permitir jugar al usuario (Advertencia de nulidad corregida) ---
        private void cell_Validating(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Lógica para la interacción del usuario
            TextBox? cell = sender as TextBox;
            if (cell == null || cell.ReadOnly) return;

            // ... (Aquí iría la validación de entrada del usuario)
        }

        private void Form1_Load(object sender, EventArgs e) { /* vacío */ }
    }
}