using SEL;
using GalletaAI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalletaAI.Core
{
    public class AILogic
    {
        public int NodesSearched { get; private set; }
        private int _depthBound;

        public AILogic(int depthBound = 6)
        {
            _depthBound = depthBound;
        }

        // Basado en el método computerMoves de Reversi (Cap. 8) [cite: 3808]
        public Move? FindBestMove(GameState currentState)
        {
            NodesSearched = 0;

            // El nodo raíz representa el estado ACTUAL, y fue el HUMANO quien movió para llegar aquí.
            GalletaNode root = new GalletaNode(Player.Human, currentState, _depthBound);

            Graph<GalletaNode> graph = new Graph<GalletaNode>(root);

            // Ejecutamos la búsqueda DFS (Minimax)
            foreach (GalletaNode node in graph.depthFirst())
            {
                NodesSearched++;
            }

            // Después de que el DFS se completa, 'root.bestChild' tiene la mejor
            // jugada encontrada 
            if (root.bestChild != null)
            {
                return root.bestChild.moveTaken;
            }

            // No se encontró ningún movimiento (o el juego terminó)
            return null;
        }
    }
}
