using ChessEngine.Main;
using ChessEngine.Pieces;

namespace ChessEngine.Moves
{
    public class EnPassant : Ordinary
    {



        public EnPassant(Board board, Piece piece,Coordinate to):base(board,piece,to)
        {
            
        }
        public override void MakeMove(Board board)
        {
            if (Piece.Color == Color.White)
            {
                CapturedPiece= To.To(Coordinate.Directions.South).GetPiece(board);
            }
            else
            {
                CapturedPiece = To.To(Coordinate.Directions.North).GetPiece(board);
            }
            CapturedPiece.RemoveMe(board);
            Piece.MoveTo(board,To);
        }
        public override string Notation()
        {
            return From.ToString()[0]+ "x" + To.ToString() + " (e.p)";
        }
    }
}
