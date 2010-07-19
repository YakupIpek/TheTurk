using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessEngine;
using ChessEngine.Pieces;
using ChessEngine.Moves;

namespace ChessEngine.Moves
{
    public class LongCastle : Move
    {
        public LongCastle(Piece piece):base(piece)
        {
            
        }
    
        public override void MakeMove(Board board)
        {
            var rook = board[Coordinate.a1];
            piece.MoveTo(board,Coordinate.c1);
            rook.MoveTo(board,Coordinate.d1);
        }

        public override void UnMakeMove(Board board)
        {
            var rook = board[Coordinate.d1];
            piece.MoveTo(board,Coordinate.e1);
            rook.MoveTo(board, Coordinate.a1);
        }

        public override string Notation()
        {
            return "0-0-0";
        }



    }
}
