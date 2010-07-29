using ChessEngine.Pieces;

namespace ChessEngine.Main
{
    class Evaluation
    {
        public static int Evaluate(Board board)
        {

            int score = 0;
            foreach (Piece piece in board)
            {

                score += piece.PieceValue;

            }

            if (board.Side == Color.White) score *= -1;
            return score;
        }
    }
}
