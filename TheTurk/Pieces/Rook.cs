using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessEngine;
using ChessEngine.Pieces;
using ChessEngine.Moves;

namespace ChessEngine.Pieces
{
    public class Rook : Piece
    {
        static readonly int pieceValue=500;
        public const char letter = 'R';
        public Rook(Coordinate from, Color color)
            : base(from, color) 
        {

        }
        public override int PieceValue
        {
            get { return color == Color.White ? pieceValue : -pieceValue; }
        }
        public override char notationLetter
        {
            get { return letter; }
        }
        public override Coordinate[] pieceDirection
        {
            get
            {
                return Coordinate.fourDirectionDelta;
            }
        }
    }
}
