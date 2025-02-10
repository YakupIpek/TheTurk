using System;
using TheTurk.Pieces;
using System.Linq;

namespace TheTurk.Engine
{
    public class Evaluation
    {
        public static int Evaluate(Board board)
        {
            return (int)board.Side * board.GetPieces().Sum(p => p.Evaluation());
        }
    }
}
