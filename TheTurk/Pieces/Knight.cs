using System.Collections.Generic;
using ChessEngine.Main;
using ChessEngine.Moves;

namespace ChessEngine.Pieces
{
    public class Knight : Piece
    {
        private const int knight = 1;
        const int pieceValue = 325;
        public const char Letter = 'N';
        public static readonly Coordinate[] Directions;
        private static readonly int[,] pieceSquareTable;
        static Knight()
        {
            Directions = new Coordinate[]
                             {
                                 new Coordinate(2, 1), new Coordinate(2, -1), new Coordinate(-2, 1),
                                 new Coordinate(-2, -1),
                                 new Coordinate(1, -2), new Coordinate(-1, -2), new Coordinate(1, 2),
                                 new Coordinate(-1, 2)
                             };
            pieceSquareTable = new int[,]{
                                           {-8,  -8,  -8,  -8,  -8,  -8,  -8,  -8},
                                           {-8,   0,   0,   0,   0,   0,   0,  -8},
                                           {-8,   0,   4,   4,   4,   4,   0,  -8},
                                           {-8,   0,   4,   8,   8,   4,   0,  -8},
                                           {-8,   0,   4,   8,   8,   4,   0,  -8},
                                           {-8,   0,   4,   4,   4,   4,   0,  -8},
                                           {-8,   0,   1,   2,   2,   1,   0,  -8},
                                           {-8, -12,  -8,  -8,  -8,  -8, -12,  -8},
                                         };
        }

        public Knight(Coordinate from, Color color)
            : base(from, color)
        {

        }
        public override Coordinate[] PieceDirection
        {
            get
            {
                return Directions;
            }
        }
        public override bool Sliding
        {
            get
            {
                return false;
            }
        }
        public override int PieceValue
        {
            get { return Color == Color.White ? pieceValue : -pieceValue; }
        }
        public override char NotationLetter
        {
            get { return Letter; }
        }
        public override int[,] PieceSquareTable
        {
            get { return pieceSquareTable; }
        }

        public override int ToInt
        {
            get { return knight; }
        }
    }
}
