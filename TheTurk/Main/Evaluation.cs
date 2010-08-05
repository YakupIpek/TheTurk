using System;
using ChessEngine.Pieces;

namespace ChessEngine.Main
{
    class Evaluation
    {
        public static int Evaluate(Board board)
        {

            var score = 0;
            foreach (Piece piece in board)
            {
                score += piece.Evaluation();
            }
            return (int)board.Side*score;
        }
    }
}
