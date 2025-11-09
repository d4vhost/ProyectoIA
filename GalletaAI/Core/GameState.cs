using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GalletaAI.Core
{
    // Enum para saber quién es el dueño de una "galleta" (cuadro)
    public enum Player { None, Human, AI }

    // Representa un solo movimiento (una línea dibujada)
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

        // Para comparar movimientos
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

    // Clase principal que almacena el estado del tablero y la lógica de reglas
    public class GameState
    {
        public const int BOARD_SIZE = 4; // 4x4 cuadros (5x5 puntos)

        // Almacenamos las líneas y los dueños de los cuadros
        public bool[,] HorizontalLines { get; private set; }
        public bool[,] VerticalLines { get; private set; }
        public Player[,] SquareOwners { get; private set; }

        public int HumanScore { get; private set; }
        public int AIScore { get; private set; }
        public Player CurrentPlayer { get; private set; }

        public GameState()
        {
            // 5 filas de líneas horizontales (0 a 4), 4 columnas (0 a 3)
            HorizontalLines = new bool[BOARD_SIZE + 1, BOARD_SIZE];
            // 4 filas de líneas verticales (0 a 3), 5 columnas (0 a 4)
            VerticalLines = new bool[BOARD_SIZE, BOARD_SIZE + 1];
            // 4x4 cuadros
            SquareOwners = new Player[BOARD_SIZE, BOARD_SIZE];

            HumanScore = 0;
            AIScore = 0;
            CurrentPlayer = Player.Human; // El humano empieza
        }

        // Constructor para clonar el estado (esencial para el árbol de IA)
        public GameState(GameState source)
        {
            HorizontalLines = (bool[,])source.HorizontalLines.Clone();
            VerticalLines = (bool[,])source.VerticalLines.Clone();
            SquareOwners = (Player[,])source.SquareOwners.Clone();
            HumanScore = source.HumanScore;
            AIScore = source.AIScore;
            CurrentPlayer = source.CurrentPlayer;
        }

        public bool IsGameOver()
        {
            return HumanScore + AIScore == BOARD_SIZE * BOARD_SIZE;
        }

        // Obtiene una lista de todos los movimientos (líneas) posibles
        public List<Move> GetValidMoves()
        {
            var moves = new List<Move>();

            // Líneas horizontales
            for (int r = 0; r <= BOARD_SIZE; r++)
                for (int c = 0; c < BOARD_SIZE; c++)
                    if (!HorizontalLines[r, c])
                        moves.Add(new Move(true, r, c));

            // Líneas verticales
            for (int r = 0; r < BOARD_SIZE; r++)
                for (int c = 0; c <= BOARD_SIZE; c++)
                    if (!VerticalLines[r, c])
                        moves.Add(new Move(false, r, c));

            return moves;
        }

        // Aplica un movimiento y retorna el jugador del *siguiente* turno
        // Esta es la regla más importante: si completas un cuadro, sigues jugando.
        public Player ApplyMove(Move move)
        {
            if (move.IsHorizontal)
            {
                if (HorizontalLines[move.Row, move.Col]) return CurrentPlayer; // Movimiento ilegal
                HorizontalLines[move.Row, move.Col] = true;
            }
            else
            {
                if (VerticalLines[move.Row, move.Col]) return CurrentPlayer; // Movimiento ilegal
                VerticalLines[move.Row, move.Col] = true;
            }

            int squaresCompleted = CheckForCompletedSquares(move);

            if (squaresCompleted > 0)
            {
                if (CurrentPlayer == Player.Human)
                    HumanScore += squaresCompleted;
                else
                    AIScore += squaresCompleted;

                // El jugador completó un cuadro, así que le toca de nuevo
                return CurrentPlayer;
            }
            else
            {
                // No se completó cuadro, cambia el turno
                CurrentPlayer = (CurrentPlayer == Player.Human) ? Player.AI : Player.Human;
                return CurrentPlayer;
            }
        }

        // Revisa si el último movimiento completó uno o dos cuadros
        private int CheckForCompletedSquares(Move move)
        {
            int completed = 0;

            if (move.IsHorizontal)
            {
                // Revisar cuadro de abajo (si existe)
                if (move.Row < BOARD_SIZE)
                    if (CheckSquare(move.Row, move.Col))
                        completed++;

                // Revisar cuadro de arriba (si existe)
                if (move.Row > 0)
                    if (CheckSquare(move.Row - 1, move.Col))
                        completed++;
            }
            else // Movimiento Vertical
            {
                // Revisar cuadro de la derecha (si existe)
                if (move.Col < BOARD_SIZE)
                    if (CheckSquare(move.Row, move.Col))
                        completed++;

                // Revisar cuadro de la izquierda (si existe)
                if (move.Col > 0)
                    if (CheckSquare(move.Row, move.Col - 1))
                        completed++;
            }
            return completed;
        }

        // Revisa si un cuadro específico (r, c) está completo
        private bool CheckSquare(int r, int c)
        {
            if (SquareOwners[r, c] != Player.None) return false; // Ya tiene dueño

            if (HorizontalLines[r, c] &&   // Arriba
                HorizontalLines[r + 1, c] && // Abajo
                VerticalLines[r, c] &&     // Izquierda
                VerticalLines[r, c + 1])   // Derecha
            {
                SquareOwners[r, c] = CurrentPlayer; // Asignar dueño
                return true;
            }
            return false;
        }
    }
}