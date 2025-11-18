// Archivo: GalletaAI/Core/AILogic.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalletaAI.Core
{
    public class AILogic
    {
        private int _depthBound;

        public AILogic(int depthBound = 4)
        {
            _depthBound = depthBound;
        }

        // Método principal que retorna el mejor movimiento para la IA
        public Move? GetBestMove(GameState state)
        {
            if (state.IsGameOver())
                return null;

            var validMoves = state.GetValidMoves();
            if (validMoves.Count == 0)
                return null;

            Move? bestMove = null;
            int bestScore = int.MinValue;

            foreach (var move in validMoves)
            {
                GameState clonedState = new GameState(state);
                clonedState.ApplyMove(move);

                // Si la IA completa un cuadrado, quiere maximizar
                // Si no completa, el humano juega después, así que minimizamos
                int score = Minimax(clonedState, _depthBound - 1, int.MinValue, int.MaxValue, false);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        // Algoritmo Minimax con poda Alpha-Beta
        // Algoritmo Minimax con poda Alpha-Beta
        private int Minimax(GameState state, int depth, int alpha, int beta, bool isMaximizing)
        {
            // Condición de parada: profundidad 0 o juego terminado
            if (depth == 0 || state.IsGameOver())
            {
                return EvaluateState(state);
            }

            var validMoves = state.GetValidMoves();

            if (isMaximizing) // Turno de la IA (maximizar)
            {
                int maxScore = int.MinValue;

                foreach (var move in validMoves)
                {
                    GameState clonedState = new GameState(state);
                    clonedState.ApplyMove(move);

                    // ✅ CAMBIO: ApplyMove SIEMPRE cambia de turno, así que siempre alternamos MAX/MIN
                    int score = Minimax(clonedState, depth - 1, alpha, beta, false);

                    maxScore = Math.Max(maxScore, score);
                    alpha = Math.Max(alpha, score);

                    if (beta <= alpha)
                        break; // Poda Beta
                }

                return maxScore;
            }
            else // Turno del humano (minimizar)
            {
                int minScore = int.MaxValue;

                foreach (var move in validMoves)
                {
                    GameState clonedState = new GameState(state);
                    clonedState.ApplyMove(move);

                    // ✅ CAMBIO: ApplyMove SIEMPRE cambia de turno, así que siempre alternamos MIN/MAX
                    int score = Minimax(clonedState, depth - 1, alpha, beta, true);

                    minScore = Math.Min(minScore, score);
                    beta = Math.Min(beta, score);

                    if (beta <= alpha)
                        break; // Poda Alpha
                }

                return minScore;
            }
        }

        // Función de evaluación: diferencia de cuadros completados
        private int EvaluateState(GameState state)
        {
            // Puntuación base: diferencia de cuadros
            int scoreDiff = state.AIScore - state.HumanScore;

            // Bonificación adicional: control del tablero
            int aiControl = CountPotentialSquares(state, Player.AI);
            int humanControl = CountPotentialSquares(state, Player.Human);

            // Peso: 100 puntos por cuadro completado, 1 punto por cuadro potencial
            return (scoreDiff * 100) + (aiControl - humanControl);
        }

        // Cuenta cuántos cuadros están a 1 movimiento de completarse
        private int CountPotentialSquares(GameState state, Player player)
        {
            int count = 0;

            for (int r = 0; r < GameState.BOARD_SIZE; r++)
            {
                for (int c = 0; c < GameState.BOARD_SIZE; c++)
                {
                    if (state.SquareOwners[r, c] == Player.None)
                    {
                        int sidesCompleted = 0;

                        if (state.HorizontalLines[r, c]) sidesCompleted++;
                        if (state.HorizontalLines[r + 1, c]) sidesCompleted++;
                        if (state.VerticalLines[r, c]) sidesCompleted++;
                        if (state.VerticalLines[r, c + 1]) sidesCompleted++;

                        // Si tiene 3 lados, está a punto de completarse
                        if (sidesCompleted == 3)
                            count++;
                    }
                }
            }

            return count;
        }
    }
}