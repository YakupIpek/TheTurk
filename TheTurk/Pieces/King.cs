using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessEngine;
using ChessEngine.Pieces;
using ChessEngine.Moves;

namespace ChessEngine.Pieces
{
    public class King : Piece
    {
        static int pieceValue = 1000000;
        public King(Coordinate from, Color color)
            : base(from, color)
        {

        }
        public override bool sliding
        {
            get
            {
                return false;
            }
        }
        public override char notationLetter
        {
            get { return 'K'; }
        }

        public override int PieceValue
        {
            get { return color == Color.White ? pieceValue : -pieceValue; }
        }
        public override Coordinate[] pieceDirection
        {
            get
            {
                return Coordinate.allDirection;
            }
        }
        public override List<Move> GenerateMoves(Board board)
        {
            return base.GenerateMoves(board);
        }

    }
}
