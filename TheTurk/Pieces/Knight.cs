using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChessEngine;
using ChessEngine.Pieces;
using ChessEngine.Moves;

namespace ChessEngine.Pieces
{
    public class Knight : Piece
    {
        static readonly int pieceValue = 325;
        public const char letter = 'N';
        static readonly Coordinate[] directions;
        static Knight()
        {
            directions = new Coordinate[]{ new Coordinate(2,1),new Coordinate(2,-1),new Coordinate(-2,1),new Coordinate(-2,-1),
            new Coordinate(1,-2),new Coordinate(-1,-2),new Coordinate(1,2),new Coordinate(-1,2)};
        }
        public Knight(Coordinate from, Color color)
            : base(from, color)
        {

        }
        public override Coordinate[] pieceDirection
        {
            get
            {
                return directions;
            }
        }
        public override bool sliding
        {
            get
            {
                return false;
            }
        }
        public override int PieceValue
        {
            get { return color == Color.White ? pieceValue : -pieceValue; }
        }
        public override char notationLetter
        {
            get { return letter; }
        }

    }
}
