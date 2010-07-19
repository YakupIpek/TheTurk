using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessEngine;
using ChessEngine.Pieces;
using ChessEngine.Moves;

namespace ChessEngine.Moves
{
    public class ShortCastle : Move
    {
        public ShortCastle(Piece piece)
            : base(piece)
        {
            
        }
        public override void MakeMove(Board board)
        {
            var rook = board[Coordinate.h1];
            piece.MoveTo(board, Coordinate.g1);
            rook.MoveTo(board, Coordinate.f1);

        }

        public override void UnMakeMove(Board board)
        {
            var rook = board[Coordinate.f1];
            piece.MoveTo(board, Coordinate.e1);
            rook.MoveTo(board, Coordinate.h1);
        }
        public override string Notation()
        {
            return "0-0";
        }
    }
}
