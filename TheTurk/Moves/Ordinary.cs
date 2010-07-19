using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessEngine;
using ChessEngine.Pieces;
using ChessEngine.Moves;

namespace ChessEngine.Moves
{
    public class Ordinary : Move
    {
        protected Piece capturedPiece;
        protected Coordinate to;

        public Ordinary(Board board, Piece piece,Coordinate to):base(piece)
        {
            this.to = to;
            this.capturedPiece = board[to];
        }
    
        public override void MakeMove(Board board)
        {
            capturedPiece = board[to];
            piece.MoveTo(board, to);

        }

        public override void UnMakeMove(Board board)
        {
            piece.MoveTo(board,from);
            if (capturedPiece != null)
            {
                capturedPiece.PutMe(board);
            }
            

        }
        public override string Notation()
        {
            string captured = capturedPiece == null ? "" : "x";
            return (piece.notationLetter + captured+ to.ToString()).Trim();
        }
    }
}
