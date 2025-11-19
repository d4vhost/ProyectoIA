// Archivo: OchoReinasSolver/Form1.cs
using OchoReinasSolver.Core;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OchoReinasSolver
{
    public partial class Form1 : Form
    {
        private Panel boardPanel = null!;
        private Panel[,] squares = new Panel[8, 8];
        private Label lblTitle = null!;
        private Button btnSolve = null!;
        private Button btnNext = null!;
        private Button btnPrevious = null!;
        private Label lblSolutionCount = null!;
        private Label lblInfo = null!;

        private List<char[,]> _allSolutions = new List<char[,]>();
        private int _currentSolutionIndex = -1;
        private bool _solving = false;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "8 Reinas Solver";
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.ClientSize = new Size(700, 800);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            lblTitle = new Label
            {
                Text = "8 REINAS",
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = false,
                Size = new Size(640, 60),
                Location = new Point(30, 15),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblTitle);

            boardPanel = new Panel
            {
                Size = new Size(560, 560),
                Location = new Point(70, 90),
                BackColor = Color.FromArgb(44, 62, 80),
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(boardPanel);

            CreateChessBoard();

            lblSolutionCount = new Label
            {
                Text = "Presiona 'RESOLVER' para encontrar todas las soluciones",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = false,
                Size = new Size(640, 30),
                Location = new Point(30, 665),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblSolutionCount);

            // Label de información
            lblInfo = new Label
            {
                Text = "Algoritmo: Backtracking (DFS)",
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = Color.FromArgb(127, 140, 141),
                AutoSize = false,
                Size = new Size(640, 20),
                Location = new Point(30, 695),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblInfo);

            // Botón Resolver
            btnSolve = new Button
            {
                Text = "RESOLVER",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(200, 50),
                Location = new Point(250, 725),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(46, 204, 113),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSolve.FlatAppearance.BorderSize = 0;
            btnSolve.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 174, 96);
            btnSolve.Click += BtnSolve_Click;
            this.Controls.Add(btnSolve);

            // Botón Anterior
            btnPrevious = new Button
            {
                Text = "◀ ANTERIOR",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(150, 50),
                Location = new Point(70, 725),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(52, 152, 219),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnPrevious.FlatAppearance.BorderSize = 0;
            btnPrevious.FlatAppearance.MouseOverBackColor = Color.FromArgb(41, 128, 185);
            btnPrevious.Click += BtnPrevious_Click;
            this.Controls.Add(btnPrevious);

            // Botón Siguiente
            btnNext = new Button
            {
                Text = "SIGUIENTE ▶",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(150, 50),
                Location = new Point(480, 725),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(52, 152, 219),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.FlatAppearance.MouseOverBackColor = Color.FromArgb(41, 128, 185);
            btnNext.Click += BtnNext_Click;
            this.Controls.Add(btnNext);
        }

        private void CreateChessBoard()
        {
            int squareSize = 65;
            int margin = 10;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Panel square = new Panel
                    {
                        Size = new Size(squareSize, squareSize),
                        Location = new Point(col * squareSize + margin, row * squareSize + margin),
                        BackColor = (row + col) % 2 == 0 ? Color.FromArgb(240, 217, 181) : Color.FromArgb(181, 136, 99)
                    };

                    boardPanel.Controls.Add(square);
                    squares[row, col] = square;
                }
            }
        }

        private void DrawBoard(char[,] board)
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    squares[row, col].Controls.Clear();

                    if (board[row, col] == 'Q')
                    {
                        Label queenLabel = new Label
                        {
                            Text = "♛",
                            Font = new Font("Segoe UI", 36, FontStyle.Bold),
                            ForeColor = Color.FromArgb(231, 76, 60),
                            Size = new Size(65, 65),
                            TextAlign = ContentAlignment.MiddleCenter,
                            BackColor = Color.Transparent
                        };
                        squares[row, col].Controls.Add(queenLabel);
                    }
                }
            }
        }

        private void ClearBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    squares[row, col].Controls.Clear();
                }
            }
        }

        private async void BtnSolve_Click(object? sender, EventArgs e)
        {
            if (_solving) return;

            _solving = true;
            btnSolve.Enabled = false;
            btnPrevious.Enabled = false;
            btnNext.Enabled = false;
            _allSolutions.Clear();
            _currentSolutionIndex = -1;
            ClearBoard();

            lblSolutionCount.Text = "🔍 Buscando soluciones...";
            lblSolutionCount.ForeColor = Color.FromArgb(243, 156, 18);

            SolveQueensGUI solver = new SolveQueensGUI();

            await Task.Run(() =>
            {
                solver.SolveDFS((solution) =>
                {
                    _allSolutions.Add((char[,])solution.Clone());
                });
            });

            if (_allSolutions.Count > 0)
            {
                _currentSolutionIndex = 0;
                DrawBoard(_allSolutions[0]);
                lblSolutionCount.Text = $"✓ Solución 1 de {_allSolutions.Count} (Nodos explorados: {solver.NodesSearched})";
                lblSolutionCount.ForeColor = Color.FromArgb(46, 204, 113);

                btnPrevious.Enabled = false;
                btnNext.Enabled = _allSolutions.Count > 1;
            }
            else
            {
                lblSolutionCount.Text = "❌ No se encontraron soluciones";
                lblSolutionCount.ForeColor = Color.FromArgb(231, 76, 60);
            }

            btnSolve.Enabled = true;
            _solving = false;
        }

        private void BtnPrevious_Click(object? sender, EventArgs e)
        {
            if (_currentSolutionIndex > 0)
            {
                _currentSolutionIndex--;
                DrawBoard(_allSolutions[_currentSolutionIndex]);
                lblSolutionCount.Text = $"Solución {_currentSolutionIndex + 1} de {_allSolutions.Count}";
                lblSolutionCount.ForeColor = Color.FromArgb(52, 73, 94);

                btnPrevious.Enabled = _currentSolutionIndex > 0;
                btnNext.Enabled = true;
            }
        }

        private void BtnNext_Click(object? sender, EventArgs e)
        {
            if (_currentSolutionIndex < _allSolutions.Count - 1)
            {
                _currentSolutionIndex++;
                DrawBoard(_allSolutions[_currentSolutionIndex]);
                lblSolutionCount.Text = $"Solución {_currentSolutionIndex + 1} de {_allSolutions.Count}";
                lblSolutionCount.ForeColor = Color.FromArgb(52, 73, 94);

                btnPrevious.Enabled = true;
                btnNext.Enabled = _currentSolutionIndex < _allSolutions.Count - 1;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}