using ChessEngine.Main;
using ChessEngine.Pieces;

namespace ChessEngine.Moves
{
    public class LongCastle : Move
    {
        public LongCastle(Piece piece)
            : base(piece)
        {
            To = piece.Color == Color.White ? Coordinate.c1 : Coordinate.c8;
        }

        public override void MakeMove(Board board)
        {
            if (Color.White == Piece.Color)
            {
                var rook = Coordinate.a1.GetPiece(board);
                Piece.MoveTo(board, Coordinate.c1);
                rook.MoveTo(board, Coordinate.d1);
            }
            else
            {
                var rook = Coordinate.a8.GetPiece(board);
                Piece.MoveTo(board, Coordinate.c8);
                rook.MoveTo(board, Coordinate.d8);
            }
        }

        public override void UnMakeMove(Board board)
        {
            if (Piece.Color == Color.White)
            {
                var rook = Coordinate.d1.GetPiece(board);
                Piece.MoveTo(board, Coordinate.e1);
                rook.MoveTo(board, Coordinate.a1);
            }
            else
            {
                var rook = Coordinate.d8.GetPiece(board);
                Piece.MoveTo(board, Coordinate.e8);
                rook.MoveTo(board, Coordinate.a8);
            }
        }
        public override int MovePriority()
        {
            return 2*Pawn.Piecevalue;
        }
        public override string IONotation()
        {
            Coordinate to = Piece.Color == Color.White ? Coordinate.c1 : Coordinate.c8;
            return base.IONotation()+to.ToString();
        }
        public override string Notation()
        {
            return "0-0-0";
        }
        public static bool Available(Board board, Color side)
        {
            if (side == Color.White)
            {
                return Coordinate.d1.IsEmpty(board) && Coordinate.c1.IsEmpty(board) &&
                    Coordinate.b1.IsEmpty(board) &&
                    !Coordinate.e1.IsAttackedSquare(board, Color.Black) &&
                    !Coordinate.d1.IsAttackedSquare(board, Color.Black) &&
                    !Coordinate.c1.IsAttackedSquare(board, Color.Black);
            }
            return Coordinate.d8.IsEmpty(board) &&
                Coordinate.c8.IsEmpty(board) &&
                Coordinate.b8.IsEmpty(board) &&
                !Coordinate.e8.IsAttackedSquare(board,Color.White)&&
                !Coordinate.d8.IsAttackedSquare(board, Color.White) &&
                !Coordinate.c8.IsAttackedSquare(board, Color.White);

        }

        public override string ToString()
        {
            return Notation();
        }
    }
}
