// Archivo: GalletaAI/Core/AILogic.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalletaAI.Core
{
    /// <summary>
    /// Lógica de IA para el juego Timbiriche.
    /// Implementa el algoritmo Minimax con Poda Alpha-Beta.
    /// Evalúa estados del tablero recursivamente para maximizar la puntuación de la IA y minimizar la del oponente.
    /// </summary>
    public class AILogic
    {
        private int _depthBound;

        public AILogic(int depthBound = 4)
        {
            _depthBound = depthBound;
        }

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

                int score = Minimax(clonedState, _depthBound - 1, int.MinValue, int.MaxValue, false);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private int Minimax(GameState state, int depth, int alpha, int beta, bool isMaximizing)
        {
            if (depth == 0 || state.IsGameOver())
            {
                return EvaluateState(state);
            }

            var validMoves = state.GetValidMoves();

            if (isMaximizing) 
            {
                int maxScore = int.MinValue;

                foreach (var move in validMoves)
                {
                    GameState clonedState = new GameState(state);
                    clonedState.ApplyMove(move);

                    int score = Minimax(clonedState, depth - 1, alpha, beta, false);

                    maxScore = Math.Max(maxScore, score);
                    alpha = Math.Max(alpha, score);

                    if (beta <= alpha)
                        break;
                }

                return maxScore;
            }
            else 
            {
                int minScore = int.MaxValue;

                foreach (var move in validMoves)
                {
                    GameState clonedState = new GameState(state);
                    clonedState.ApplyMove(move);

                    int score = Minimax(clonedState, depth - 1, alpha, beta, true);

                    minScore = Math.Min(minScore, score);
                    beta = Math.Min(beta, score);

                    if (beta <= alpha)
                        break; 
                }

                return minScore;
            }
        }

        private int EvaluateState(GameState state)
        {
            int scoreDiff = state.AIScore - state.HumanScore;

            int aiControl = CountPotentialSquares(state, Player.AI);
            int humanControl = CountPotentialSquares(state, Player.Human);

            return (scoreDiff * 100) + (aiControl - humanControl);
        }

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

                        if (sidesCompleted == 3)
                            count++;
                    }
                }
            }

            return count;
        }
    }
}