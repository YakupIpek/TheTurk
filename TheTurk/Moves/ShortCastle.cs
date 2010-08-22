using ChessEngine.Main;
using ChessEngine.Pieces;

namespace ChessEngine.Moves
{
    public class ShortCastle : Move
    {
        public ShortCastle(Piece piece)
            : base(piece)
        {
            To = piece.Color == Color.White ? Coordinate.g1 : Coordinate.g8;
        }
        public override void MakeMove(Board board)
        {
            if (Color.White == Piece.Color)
            {
                var rook = Coordinate.h1.GetPiece(board);
                Piece.MoveTo(board, Coordinate.g1);
                rook.MoveTo(board, Coordinate.f1);
            }
            else
            {
                var rook = board[Coordinate.h8];
                Piece.MoveTo(board, Coordinate.g8);
                rook.MoveTo(board, Coordinate.f8);
            }

        }
        public override void UnMakeMove(Board board)
        {
            if (Color.White == Piece.Color)
            {
                var rook = Coordinate.f1.GetPiece(board);
                Piece.MoveTo(board, Coordinate.e1);
                rook.MoveTo(board, Coordinate.h1);
            }
            else
            {
                var rook = Coordinate.f8.GetPiece(board);
                Piece.MoveTo(board, Coordinate.e8);
                rook.MoveTo(board, Coordinate.h8);
            }
        }
        public override int MovePriority()
        {
            return 2 * Pawn.Piecevalue;
        }
        public override string IONotation()
        {
            Coordinate to = Piece.Color == Color.White ? Coordinate.g1 : Coordinate.g8;
            return base.IONotation() + to.ToString();
        }
        public override string Notation()
        {
            return "0-0";
        }
        public static bool Available(Board board, Color side)
        {
            if (side == Color.White)
            {
                return Coordinate.f1.IsEmpty(board) && Coordinate.g1.IsEmpty(board) &&
                       !Coordinate.e1.IsAttackedSquare(board, Color.Black) &&
                       !Coordinate.f1.IsAttackedSquare(board, Color.Black) &&
                       !Coordinate.g1.IsAttackedSquare(board, Color.Black);
            }

            return Coordinate.f8.IsEmpty(board) && Coordinate.g8.IsEmpty(board) &&
                       !Coordinate.e8.IsAttackedSquare(board, Color.White) &&
                       !Coordinate.f8.IsAttackedSquare(board, Color.White) &&
                       !Coordinate.g8.IsAttackedSquare(board, Color.White);
        }
        public override string ToString()
        {
            return Notation();
        }
    }
}
