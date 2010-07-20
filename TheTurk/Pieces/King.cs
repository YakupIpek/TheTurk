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
        static readonly int pieceValue = 1000000;
        public const char letter = 'K';
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
            get { return letter; }
        }

        public override int PieceValue
        {
            get { return color == Color.White ? pieceValue : -pieceValue; }
        }
        public override Coordinate[] pieceDirection
        {
            get
            {
                return Coordinate.allDirectionDelta;
            }
        }
        public override List<Move> GenerateMoves(Board board)
        {
            return base.GenerateMoves(board);
        }

    }
}
