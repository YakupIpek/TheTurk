using System.Collections.Generic;
using ChessEngine.Main;
using ChessEngine.Pieces;

namespace ChessEngine.Moves
{

    public class Promote : Ordinary
    {
        #region PromotionType enum

        public enum PromotionType
        {
            Queen, Rook, Bishop, Knight
        }

        #endregion

        readonly Piece PromotedPiece;

        public Promote(Board board, Piece piece, Coordinate to, PromotionType type)
            : base(board, piece, to)
        {
            switch (type)
            {
                case PromotionType.Queen: PromotedPiece = new Queen(to, piece.Color);
                    break;
                case PromotionType.Rook: PromotedPiece = new Rook(to, piece.Color);
                    break;
                case PromotionType.Bishop: PromotedPiece = new Bishop(to, piece.Color);
                    break;
                case PromotionType.Knight: PromotedPiece = new Knight(to, piece.Color);
                    break;
                default:
                    break;
            }


        }

        public static List<Move> AllPossiblePromotions(Board board, Piece piece, Coordinate square)
        {
            return new List<Move> 
            { 
                new Promote(board, piece, square, Promote.PromotionType.Queen),
                new Promote(board, piece, square, Promote.PromotionType.Rook),
                new Promote(board, piece, square, Promote.PromotionType.Bishop),
                new Promote(board, piece, square, Promote.PromotionType.Knight)
            };

        }
        public override void MakeMove(Board board)
        {
            Piece.RemoveMe(board);
            CapturedPiece = To.GetPiece(board);
            board[To] = PromotedPiece;
        }
        public override void UnMakeMove(Board board)
        {
            PromotedPiece.RemoveMe(board);
            if (CapturedPiece != null)
            {
                CapturedPiece.PutMe(board);
            }
            Piece.PutMe(board);
        }
        public override int MovePriority()
        {
            return PromotedPiece.PieceValue;
        }
        public override string Notation()
        {
            string notation = "=" + PromotedPiece.NotationLetter;
            return base.Notation() + notation;
        }
        public override string IONotation()
        {
            return base.IONotation() + char.ToLower(PromotedPiece.NotationLetter);
        }
        public override bool Equals(Move move)
        {
            return base.Equals(move)&& (move as Promote).PromotedPiece.Equals(PromotedPiece);
        }
    }
}
