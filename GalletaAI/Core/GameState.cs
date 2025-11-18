using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GalletaAI.Core
{
    public enum Player { None, Human, AI }

    public class Move
    {
        public bool IsHorizontal { get; }
        public int Row { get; }
        public int Col { get; }

        public Move(bool isHorizontal, int row, int col)
        {
            IsHorizontal = isHorizontal;
            Row = row;
            Col = col;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Move other)
            {
                return IsHorizontal == other.IsHorizontal && Row == other.Row && Col == other.Col;
            }
            return false;
        }
        public override int GetHashCode() => (IsHorizontal, Row, Col).GetHashCode();
    }

    public class GameState
    {
        public const int BOARD_SIZE = 13;
        public const int TOTAL_PLAYABLE_SQUARES = 57;

        public bool[,] HorizontalLines { get; private set; }
        public bool[,] VerticalLines { get; private set; }
        public Player[,] SquareOwners { get; private set; }

        public int HumanScore { get; private set; }
        public int AIScore { get; private set; }
        public Player CurrentPlayer { get; private set; }

        // ✅ Marcador de esquinas que NO cuentan para el puntaje
        private HashSet<(int, int)> _nonPlayableCorners = new HashSet<(int, int)>();

        public GameState()
        {
            HorizontalLines = new bool[BOARD_SIZE + 1, BOARD_SIZE];
            VerticalLines = new bool[BOARD_SIZE, BOARD_SIZE + 1];
            SquareOwners = new Player[BOARD_SIZE, BOARD_SIZE];

            HumanScore = 0;
            AIScore = 0;
            CurrentPlayer = Player.Human;
        }

        public GameState(GameState source)
        {
            HorizontalLines = (bool[,])source.HorizontalLines.Clone();
            VerticalLines = (bool[,])source.VerticalLines.Clone();
            SquareOwners = (Player[,])source.SquareOwners.Clone();
            HumanScore = source.HumanScore;
            AIScore = source.AIScore;
            CurrentPlayer = source.CurrentPlayer;
            _nonPlayableCorners = new HashSet<(int, int)>(source._nonPlayableCorners);
        }

        // ✅ Método para marcar esquinas como NO contables
        public void MarkCornerAsNonPlayable(int row, int col)
        {
            _nonPlayableCorners.Add((row, col));
        }

        public bool IsGameOver()
        {
            return HumanScore + AIScore >= TOTAL_PLAYABLE_SQUARES;
        }

        public List<Move> GetValidMoves()
        {
            var moves = new List<Move>();
            var playableLines = GalletaAI.Form1.PlayableLines;

            for (int r = 0; r <= BOARD_SIZE; r++)
            {
                for (int c = 0; c < BOARD_SIZE; c++)
                {
                    if (!HorizontalLines[r, c])
                    {
                        string key = $"H_{r}_{c}";
                        if (playableLines.Contains(key))
                        {
                            moves.Add(new Move(true, r, c));
                        }
                    }
                }
            }

            for (int r = 0; r < BOARD_SIZE; r++)
            {
                for (int c = 0; c <= BOARD_SIZE; c++)
                {
                    if (!VerticalLines[r, c])
                    {
                        string key = $"V_{r}_{c}";
                        if (playableLines.Contains(key))
                        {
                            moves.Add(new Move(false, r, c));
                        }
                    }
                }
            }

            return moves;
        }

        // ✅ Método especial para movimientos iniciales (no cambia turno, no suma puntaje)
        public void ApplyInitialMove(Move move)
        {
            if (move.IsHorizontal)
            {
                HorizontalLines[move.Row, move.Col] = true;
            }
            else
            {
                VerticalLines[move.Row, move.Col] = true;
            }

            // Completar cuadros sin sumar puntos
            CheckForCompletedSquares(move, false);
        }

        public Player ApplyMove(Move move)
        {
            if (move.IsHorizontal)
            {
                if (HorizontalLines[move.Row, move.Col]) return CurrentPlayer;
                HorizontalLines[move.Row, move.Col] = true;
            }
            else
            {
                if (VerticalLines[move.Row, move.Col]) return CurrentPlayer;
                VerticalLines[move.Row, move.Col] = true;
            }

            int squaresCompleted = CheckForCompletedSquares(move, true);

            if (squaresCompleted > 0)
            {
                if (CurrentPlayer == Player.Human)
                    HumanScore += squaresCompleted;
                else
                    AIScore += squaresCompleted;
            }

            CurrentPlayer = (CurrentPlayer == Player.Human) ? Player.AI : Player.Human;
            return CurrentPlayer;
        }

        // ✅ Modificado para NO contar esquinas negras
        private int CheckForCompletedSquares(Move move, bool countScore)
        {
            int completed = 0;

            if (move.IsHorizontal)
            {
                if (move.Row < BOARD_SIZE)
                    if (CheckSquare(move.Row, move.Col, countScore))
                        completed++;

                if (move.Row > 0)
                    if (CheckSquare(move.Row - 1, move.Col, countScore))
                        completed++;
            }
            else
            {
                if (move.Col < BOARD_SIZE)
                    if (CheckSquare(move.Row, move.Col, countScore))
                        completed++;

                if (move.Col > 0)
                    if (CheckSquare(move.Row, move.Col - 1, countScore))
                        completed++;
            }
            return completed;
        }

        // ✅ Modificado para NO contar esquinas negras en el puntaje
        private bool CheckSquare(int r, int c, bool countScore)
        {
            if (SquareOwners[r, c] != Player.None) return false;

            if (HorizontalLines[r, c] &&
                HorizontalLines[r + 1, c] &&
                VerticalLines[r, c] &&
                VerticalLines[r, c + 1])
            {
                SquareOwners[r, c] = CurrentPlayer;

                // ✅ Si es una esquina negra, NO contarla
                if (_nonPlayableCorners.Contains((r, c)))
                {
                    return false; // No suma al puntaje
                }

                return countScore; // Solo cuenta si countScore es true
            }
            return false;
        }
    }
}