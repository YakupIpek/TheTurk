using System;
using ChessEngine.Main;
using ChessEngine.Pieces;

namespace ChessEngine.Moves
{
    public class Ordinary : Move
    {
        public Piece CapturedPiece { get; protected set; }

        public Ordinary(Board board, Piece piece, Coordinate to)
            : base(piece)
        {
            To = to;
            CapturedPiece = To.GetPiece(board);
        }

        public override void MakeMove(Board board)
        {

            Piece.MoveTo(board, To);
        }
        public override void UnMakeMove(Board board)
        {
            Piece.MoveTo(board, From);
            if (CapturedPiece != null)
            {
                CapturedPiece.PutMe(board);
            }


        }
        public override int MovePriority()
        {
            if (CapturedPiece != null)
            {
                return Math.Abs(CapturedPiece.PieceValue - Piece.PieceValue) + Math.Abs(CapturedPiece.PieceValue / 10);
            }
            return 0;
        }
        public override string IONotation()
        {
            return base.IONotation() + To.ToString();
        }
        public override string Notation()
        {
            string captured = CapturedPiece == null ? "" : "x";
            if (Piece.GetType() == typeof(Pawn) && captured == "x") return (From.ToString()[0] + captured + To);
            return (Piece.NotationLetter + captured + To).Trim();
        }
        public override string ToString()
        {
            return Notation();
        }
        public override bool Equals(Move move)
        {
            return base.Equals(move) && To.Equals((move as Ordinary).To);
        }
    }
}
