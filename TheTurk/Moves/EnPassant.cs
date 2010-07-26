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
            if (piece.Color == Color.White)
            {
                CapturedPiece= To.To(Coordinate.Directions.South).GetPiece(board);
            }
            else
            {
                CapturedPiece = To.To(Coordinate.Directions.North).GetPiece(board);
            }
            CapturedPiece.RemoveMe(board);
            piece.MoveTo(board,To);
        }
        public override string Notation()
        {
            return from.ToString()[0]+ "x" + To.ToString() + " (e.p)";
        }
    }
}
