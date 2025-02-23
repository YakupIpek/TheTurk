using TheTurk.Engine;
using TheTurk.Pieces;

namespace TheTurk.Moves
{
    public class EnPassant : Ordinary
    {

        public EnPassant(Board board, Piece piece, Coordinate to) : base(board, piece, to)
        {
            var direction = Piece.Color == Color.White ? Coordinate.Directions.South: Coordinate.Directions.North;
            
            CapturedPiece = To.To(direction).GetPiece(board);
            
        }

        public override void MakeMove(Board board)
        {
            CapturedPiece.RemoveMe(board);
            Piece.MoveTo(board, To);
        }

        public override int MovePriority()
        {
            return base.MovePriority() + Pawn.Piecevalue * 2;
        }
        public override string Notation()
        {
            return From.ToString()[0]+ "x" + To.ToString() + " (e.p)";
        }
    }
}
