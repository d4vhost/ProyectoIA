// Archivo: GalletaAI/Core/GalletaNode.cs
using GalletaAI.Core;
using SEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GalletaAI.Core
{
    // Esta clase es la implementación de IGNode para el árbol de Minimax
    // Está basada directamente en el ReversiNode del Capítulo 8
    public class GalletaNode : IGNode<GalletaNode>
    {
        public GalletaNode? bestChild = null; // La mejor jugada encontrada desde este nodo
        public Player sideThatMoved; // El jugador que HIZO el movimiento para LLEGAR a este estado
        public Move? moveTaken = null; // El movimiento específico que se tomó
        public int evaluation = 0; // La puntuación heurística de este nodo
        public GameState position = null!; // El estado del tablero en este nodo

        private List<Move>? moves; // Lista de movimientos pendientes por explorar
        private int depth = 0;
        private GalletaNode? theParent = null;
        private static int depthBound = 6; // Profundidad de búsqueda.
                                           // 6 es rápido. 8-10 es más inteligente pero lento.

        // Constructor para el nodo RAÍZ
        public GalletaNode(Player side, GameState pos, int dBound)
        {
            sideThatMoved = side; // El jugador que movió *antes* de este estado (el oponente)
            position = pos;
            depthBound = dBound;
        }

        // Constructor para nodos HIJOS
        public GalletaNode(GalletaNode par)
        {
            theParent = par;
            if (theParent != null)
            {
                // Por defecto, el jugador que mueve es el oponente del padre
                sideThatMoved = (theParent.sideThatMoved == Player.Human) ? Player.AI : Player.Human;
                depth = theParent.depth + 1;
                position = new GameState(theParent.position); // Clonamos el estado
            }
        }

        public GalletaNode? parent
        {
            get { return theParent; }
            set { theParent = value; }
        }

        // Basado en firstChild de ReversiNode (Cap. 8) 
        public GalletaNode? firstChild()
        {
            if (depth == depthBound)
            {
                // Hoja de búsqueda: calcula la heurística (la puntuación simple)
                // Esta es la heurística del libro (puntuación material) 
                evaluation = (position.AIScore - position.HumanScore);
                return null;
            }

            var child = new GalletaNode(this);
            child.moves = child.position.GetValidMoves(); // Obtener todos los movimientos posibles

            if (child.moves.Count > 0)
            {
                child.moveTaken = child.moves[0]; // Tomar el primer movimiento
                child.moves.RemoveAt(0);

                // Aplicar el movimiento. 
                // La lógica de "turno extra" está en ApplyMove
                // sideThatMoved se ajustará basado en si se completó un cuadro o no
                Player nextPlayer = child.position.ApplyMove(child.moveTaken);

                // Si el jugador es el mismo (turno extra), este nodo es del mismo tipo (MAX/MIN)
                // Si el jugador cambió, el tipo de nodo se invierte.
                // El 'sideThatMoved' del hijo debe ser el jugador que REALIZÓ el 'moveTaken'
                child.sideThatMoved = (nextPlayer == child.parent?.sideThatMoved) ? nextPlayer : child.sideThatMoved;

                return child;
            }
            else // No hay movimientos, juego terminado en este nodo
            {
                evaluation = (position.AIScore - position.HumanScore);
                return null;
            }
        }

        // Basado en nextSibling de ReversiNode (Cap. 8) 
        public GalletaNode? nextSibling()
        {
            if (theParent == null) return null; // El nodo raíz no tiene hermanos

            // Esta es la lógica Minimax. Se llama cuando todos los hijos
            // de 'this' han sido explorados por DFS.
            // La lógica es idéntica a la del Cap. 8 
            minimax();

            // Poda Alpha/Beta (idéntica al Cap. 8) 
            if (AlphaBetaPrune())
                return null; // Podar el resto de hermanos

            if (moves == null || moves.Count == 0)
            {
                return null; // No hay más movimientos (hermanos)
            }

            var sib = new GalletaNode(theParent);
            sib.moves = moves; // Pasa la lista de movimientos restantes

            sib.moveTaken = sib.moves[0];
            sib.moves.RemoveAt(0);

            Player nextPlayer = sib.position.ApplyMove(sib.moveTaken);
            sib.sideThatMoved = (nextPlayer == sib.parent?.sideThatMoved) ? nextPlayer : sib.sideThatMoved;

            return sib;
        }

        // Lógica Minimax idéntica al Cap. 8
        private bool minimax()
        {
            if (theParent == null) return false;

            bool propagate = false;
            if (theParent.bestChild == null)
                propagate = true;
            else if (theParent.sideThatMoved == Player.AI) // Padre es un nodo MAX (movió AI)
            {
                // El padre es MAX, por lo que los hijos son MIN (mueve Humano)
                // El padre (MAX) quiere la *máxima* evaluación de sus hijos
                // *Corrección*: El libro dice que el padre es la posición DESPUÉS de que el jugador movió.
                // Si 'theParent.sideThatMoved == Player.AI', el padre es un estado al que llegó la IA.
                // Los hijos son movimientos del Humano (nodos MIN).
                // El padre MIN (Humano) elegirá la 'evaluation' MÁS BAJA.
                if (theParent.evaluation > evaluation)
                    propagate = true;
            }
            else // Padre es un nodo MIN (movió Humano)
            {
                // El padre es MIN, los hijos son MAX (mueve AI).
                // El padre (MIN) quiere la *mínima* evaluación de sus hijos.
                // *Corrección*: Si 'theParent.sideThatMoved == Player.Human', el padre es un estado al que llegó el Humano.
                // Los hijos son movimientos de la IA (nodos MAX).
                // El padre MAX (IA) elegirá la 'evaluation' MÁS ALTA.
                if (theParent.evaluation < evaluation)
                    propagate = true;
            }

            if (propagate)
            {
                theParent.evaluation = evaluation;
                theParent.bestChild = this;
                return true;
            }
            return false;
        }

        // Lógica de Poda Alpha/Beta, basada en Cap. 8 
        private bool AlphaBetaPrune()
        {
            if (theParent?.parent == null) return false;

            GalletaNode? n = theParent.parent; // Abuelo

            // Recorre la cadena de ancestros
            while (n != null)
            {
                if (n.bestChild != null) // Si el ancestro ha sido evaluado
                {
                    if (theParent.sideThatMoved == Player.AI) // Padre es MAX, Abuelo es MIN
                    {
                        // Poda Beta (el nodo MAX 'this' es > que un ancestro MIN)
                        // 'n.sideThatMoved' sería Humano (MIN)
                        if (n.sideThatMoved == Player.Human && evaluation >= n.evaluation)
                            return true; // Podar
                    }
                    else // Padre es MIN, Abuelo es MAX
                    {
                        // Poda Alpha (el nodo MIN 'this' es < que un ancestro MAX)
                        // 'n.sideThatMoved' sería AI (MAX)
                        if (n.sideThatMoved == Player.AI && evaluation <= n.evaluation)
                            return true; // Podar
                    }
                }
                n = n.parent;
            }
            return false;
        }
    }
}