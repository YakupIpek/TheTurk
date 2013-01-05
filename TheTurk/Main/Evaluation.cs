using System;
using ChessEngine.Pieces;
using System.Linq;

namespace ChessEngine.Main
{
    public class Evaluation
    {
        public static int Evaluate(Board board)
        {
            return (int)board.Side * board.GetPieces().Sum(p => p.Evaluation());
        }
    }
}
