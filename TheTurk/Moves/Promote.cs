using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessEngine;
using ChessEngine.Pieces;
using ChessEngine.Moves;

namespace ChessEngine.Moves
{

    public class Promote : Ordinary
    {
        Piece PromotedPiece;

        public Promote(Board board, Piece piece, Coordinate to, PromotionType type)
            : base(board, piece, to)
        {
            capturedPiece = to.GetPiece(board);

            var color = piece.color == Piece.Color.White ? Piece.Color.Black : Piece.Color.White;
            switch (type)
            {
                case PromotionType.Queen: piece = new Queen(to, color);
                    break;
                case PromotionType.Rook: PromotedPiece = new Rook(to, color);
                    break;
                case PromotionType.Bishop: PromotedPiece = new Bishop(to, color);
                    break;
                case PromotionType.Knight: PromotedPiece = new Knight(to, color);
                    break;
                default:
                    break;
            }


        }

        public enum PromotionType
        {
            Queen, Rook, Bishop, Knight
        }
        public static List<Move> AllPossiblePromotions(Board board, Piece piece, Coordinate square)
        {
            List<Move> moves = new List<Move>();
            Move move = new Promote(board, piece, square, Promote.PromotionType.Queen);
            moves.Add(move);
            move = new Promote(board, piece, square, Promote.PromotionType.Rook);
            moves.Add(move);
            move = new Promote(board, piece, square, Promote.PromotionType.Bishop);
            moves.Add(move);
            move = new Promote(board, piece, square, Promote.PromotionType.Knight);
            moves.Add(move);
            return moves;
        }
        public override void MakeMove(Board board)
        {
            piece.RemoveMe(board);
            capturedPiece = board[to];
            board[to] = PromotedPiece;
        }
        public override void UnMakeMove(Board board)
        {
            PromotedPiece.RemoveMe(board);
            if (capturedPiece != null)
            {
                capturedPiece.PutMe(board);
            }
            piece.PutMe(board);
        }
        public override string Notation()
        {
            string notation = "=" + PromotedPiece.notationLetter;
            return base.Notation() +notation;
        }
    }
}
