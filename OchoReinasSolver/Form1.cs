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
        private System.Windows.Forms.Timer? _animationTimer;
        private bool _isAnimating = false;
        private bool _isPaused = false; // Nueva variable para controlar pausa

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
            InitializeAnimationTimer();
            DrawInitialQueens();
            PreCalculateSolutions(); // ✅ Calcular soluciones al inicio
        }

        // ✅ NUEVO MÉTODO: Pre-calcular todas las soluciones al inicio
        private async void PreCalculateSolutions()
        {
            if (_allSolutions.Count > 0) return; // Ya calculadas

            lblSolutionCount.Text = "⏳ Calculando soluciones iniciales...";
            lblSolutionCount.ForeColor = Color.FromArgb(243, 156, 18);
            btnSolve.Enabled = false;

            await Task.Run(() =>
            {
                SolveQueensGUI solver = new SolveQueensGUI();
                solver.SolveDFS((solution) =>
                {
                    _allSolutions.Add((char[,])solution.Clone());
                });
            });

            lblSolutionCount.Text = "Usa ◀ ▶ para navegar manualmente o presiona 'RESOLVER' para ver animación";
            lblSolutionCount.ForeColor = Color.FromArgb(52, 73, 94);
            btnSolve.Enabled = true;
            btnNext.Enabled = true; // ✅ Habilitar SIGUIENTE desde el inicio
        }

        private void InitializeAnimationTimer()
        {
            _animationTimer = new System.Windows.Forms.Timer();
            _animationTimer.Interval = 500; // 500ms entre cada solución
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (_currentSolutionIndex < _allSolutions.Count - 1)
            {
                _currentSolutionIndex++;
                DrawBoard(_allSolutions[_currentSolutionIndex]);
                UpdateSolutionLabel();
            }
            else
            {
                // Terminó de mostrar todas las soluciones
                StopAnimation();
                ShowCompletionMessage();
            }
        }

        private void ShowCompletionMessage()
        {
            MessageBox.Show(
                $"Se han mostrado las {_allSolutions.Count} soluciones posibles del problema de las 8 reinas.\n\n" +
                "✓ Todas las soluciones son únicas (sin rotaciones ni simetrías).\n" +
                "✓ Ninguna reina se ataca entre sí.",
                "🎯 Soluciones Completadas",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            // Limpiar y resetear al estado inicial
            ResetToInitialState();
        }

        private void ResetToInitialState()
        {
            _allSolutions.Clear();
            _currentSolutionIndex = -1;
            _isPaused = false;
            ClearBoard();
            DrawInitialQueens();
            lblSolutionCount.Text = "Usa ◀ ▶ para navegar manualmente o presiona 'RESOLVER' para ver animación";
            lblSolutionCount.ForeColor = Color.FromArgb(52, 73, 94);
            btnPrevious.Enabled = false;
            btnNext.Enabled = false;
            btnSolve.Enabled = true;
            btnSolve.Text = "RESOLVER";
            btnSolve.BackColor = Color.FromArgb(46, 204, 113);
            btnSolve.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 174, 96);

            // ✅ Pre-calcular soluciones al inicio para navegación manual
            PreCalculateSolutions();
        }

        private void DrawInitialQueens()
        {
            // ✅ CORRECCIÓN: Ahora las reinas iniciales son ROJAS (mismo color que durante resolución)
            for (int row = 0; row < 8; row++)
            {
                Label queenLabel = new Label
                {
                    Text = "♛",
                    Font = new Font("Segoe UI", 36, FontStyle.Bold),
                    ForeColor = Color.FromArgb(231, 76, 60), // ✅ Ahora son ROJAS desde el inicio
                    Size = new Size(65, 65),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.Transparent
                };
                squares[row, 0].Controls.Add(queenLabel);
            }
        }

        private void InitializeCustomComponents()
        {
            this.Text = "8 Reinas Solver - 92 Soluciones Únicas";
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
                Text = "⏳ Cargando...",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                AutoSize = false,
                Size = new Size(640, 30),
                Location = new Point(30, 665),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblSolutionCount);

            lblInfo = new Label
            {
                Text = "Algoritmo: Backtracking (DFS) - Sin rotaciones ni simetrías",
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
                Enabled = false // ✅ Se habilitará después de pre-calcular
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
                            ForeColor = Color.FromArgb(231, 76, 60), // Rojo durante soluciones
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

        private void UpdateSolutionLabel()
        {
            lblSolutionCount.Text = $"🎬 Mostrando solución {_currentSolutionIndex + 1} de {_allSolutions.Count}...";
            lblSolutionCount.ForeColor = Color.FromArgb(155, 89, 182);
        }

        private void UpdateNavigationButtons()
        {
            // ✅ Los botones funcionan cuando NO está animando (incluso si está pausado)
            btnPrevious.Enabled = _currentSolutionIndex > 0 && !_isAnimating;
            btnNext.Enabled = _currentSolutionIndex < _allSolutions.Count - 1 && !_isAnimating;
        }

        private async void BtnSolve_Click(object? sender, EventArgs e)
        {
            // ✅ NUEVA FUNCIONALIDAD: PAUSAR/REANUDAR
            if (_isAnimating)
            {
                // Si está animando, PAUSAR
                PauseAnimation();
                return;
            }

            if (_isPaused)
            {
                // Si está pausado, REANUDAR
                ResumeAnimation();
                return;
            }

            // ✅ Si ya estaba navegando manualmente, reiniciar desde el inicio
            if (_allSolutions.Count > 0)
            {
                _currentSolutionIndex = 0;
                DrawBoard(_allSolutions[0]);

                lblSolutionCount.Text = $"✓ {_allSolutions.Count} soluciones cargadas. Iniciando animación...";
                lblSolutionCount.ForeColor = Color.FromArgb(46, 204, 113);

                await Task.Delay(1000); // Pausa de 1 segundo antes de iniciar

                // Iniciar animación automática desde la primera solución
                StartAnimation();
                return;
            }

            // Si no hay soluciones, calcular (esto no debería pasar ya)
            if (_solving) return;

            _solving = true;
            btnSolve.Enabled = false;
            btnPrevious.Enabled = false;
            btnNext.Enabled = false;
            _allSolutions.Clear();
            _currentSolutionIndex = -1;
            ClearBoard();

            lblSolutionCount.Text = "🔍 Calculando las 92 soluciones únicas...";
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
                lblSolutionCount.Text = $"✓ {_allSolutions.Count} soluciones encontradas. Iniciando animación...";
                lblSolutionCount.ForeColor = Color.FromArgb(46, 204, 113);

                await Task.Delay(1000); // Pausa de 1 segundo antes de iniciar

                // Iniciar animación automática
                _currentSolutionIndex = 0;
                DrawBoard(_allSolutions[0]);
                StartAnimation();
            }
            else
            {
                lblSolutionCount.Text = "❌ No se encontraron soluciones";
                lblSolutionCount.ForeColor = Color.FromArgb(231, 76, 60);
                btnSolve.Enabled = true;
            }

            _solving = false;
        }

        private void StartAnimation()
        {
            _isAnimating = true;
            _isPaused = false;
            btnSolve.Enabled = true;
            btnSolve.Text = "⏸ PAUSAR";
            btnSolve.BackColor = Color.FromArgb(230, 126, 34); // Color naranja para pausar
            btnSolve.FlatAppearance.MouseOverBackColor = Color.FromArgb(211, 84, 0);
            btnPrevious.Enabled = false;
            btnNext.Enabled = false;
            _animationTimer?.Start();
        }

        private void PauseAnimation()
        {
            _isAnimating = false;
            _isPaused = true;
            _animationTimer?.Stop();
            btnSolve.Text = "▶ REANUDAR";
            btnSolve.BackColor = Color.FromArgb(52, 152, 219); // Color azul para reanudar
            btnSolve.FlatAppearance.MouseOverBackColor = Color.FromArgb(41, 128, 185);
            lblSolutionCount.Text = $"⏸ PAUSADO - Solución {_currentSolutionIndex + 1} de {_allSolutions.Count}";
            lblSolutionCount.ForeColor = Color.FromArgb(230, 126, 34);
            UpdateNavigationButtons(); // ✅ Habilitar botones al pausar
        }

        private void ResumeAnimation()
        {
            _isAnimating = true;
            _isPaused = false;
            btnSolve.Text = "⏸ PAUSAR";
            btnSolve.BackColor = Color.FromArgb(230, 126, 34);
            btnSolve.FlatAppearance.MouseOverBackColor = Color.FromArgb(211, 84, 0);
            UpdateSolutionLabel();
            _animationTimer?.Start();
        }

        private void StopAnimation()
        {
            _isAnimating = false;
            _isPaused = false;
            _animationTimer?.Stop();
        }

        private void BtnPrevious_Click(object? sender, EventArgs e)
        {
            if (_currentSolutionIndex > 0 && !_isAnimating)
            {
                _currentSolutionIndex--;
                DrawBoard(_allSolutions[_currentSolutionIndex]);

                // ✅ Actualizar el mensaje según el estado
                if (_isPaused)
                {
                    lblSolutionCount.Text = $"⏸ PAUSADO - Solución {_currentSolutionIndex + 1} de {_allSolutions.Count}";
                    lblSolutionCount.ForeColor = Color.FromArgb(230, 126, 34);
                }
                else
                {
                    lblSolutionCount.Text = $"📍 Solución {_currentSolutionIndex + 1} de {_allSolutions.Count}";
                    lblSolutionCount.ForeColor = Color.FromArgb(52, 73, 94);
                }

                UpdateNavigationButtons();
            }
        }

        private void BtnNext_Click(object? sender, EventArgs e)
        {
            // ✅ Si es el primer click en SIGUIENTE (estado inicial), mostrar primera solución
            if (_currentSolutionIndex == -1 && _allSolutions.Count > 0)
            {
                _currentSolutionIndex = 0;
                ClearBoard();
                DrawBoard(_allSolutions[0]);
                lblSolutionCount.Text = $"📍 Solución 1 de {_allSolutions.Count}";
                lblSolutionCount.ForeColor = Color.FromArgb(52, 73, 94);
                UpdateNavigationButtons();
                return;
            }

            if (_currentSolutionIndex < _allSolutions.Count - 1 && !_isAnimating)
            {
                _currentSolutionIndex++;
                DrawBoard(_allSolutions[_currentSolutionIndex]);

                // ✅ Actualizar el mensaje según el estado
                if (_isPaused)
                {
                    lblSolutionCount.Text = $"⏸ PAUSADO - Solución {_currentSolutionIndex + 1} de {_allSolutions.Count}";
                    lblSolutionCount.ForeColor = Color.FromArgb(230, 126, 34);
                }
                else
                {
                    lblSolutionCount.Text = $"📍 Solución {_currentSolutionIndex + 1} de {_allSolutions.Count}";
                    lblSolutionCount.ForeColor = Color.FromArgb(52, 73, 94);
                }

                UpdateNavigationButtons();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopAnimation();
            _animationTimer?.Dispose();
            base.OnFormClosing(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}