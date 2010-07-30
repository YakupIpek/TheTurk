using ChessEngine.Main;
using ChessEngine.Pieces;

namespace ChessEngine.Moves
{
    public class Ordinary : Move
    {
        public Ordinary(Board board, Piece piece, Coordinate to)
            : base(piece)
        {
            this.To = to;
            //this.CapturedPiece = board[to];
        }

        public Piece CapturedPiece { get; protected set; }
        public Coordinate To { get; protected set; }

        public override void MakeMove(Board board)
        {
            CapturedPiece = To.GetPiece(board);
            piece.MoveTo(board, To);
        }

        public override void UnMakeMove(Board board)
        {
            piece.MoveTo(board, from);
            if (CapturedPiece != null)
            {
                CapturedPiece.PutMe(board);
            }


        }
        public override string IONotation()
        {
            return base.IONotation() + To.ToString();
        }
        public override string Notation()
        {
            string captured = CapturedPiece == null ? "" : "x";
            if (piece.GetType() == typeof(Pawn) && captured == "x") return (from.ToString()[0] + captured + To);
            return (piece.NotationLetter + captured + To).Trim();
        }
        public override string ToString()
        {
            return Notation();
        }
    }
}
