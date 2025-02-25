using System.Collections.Generic;
using TheTurk.Engine;
using TheTurk.Moves;

namespace TheTurk.Pieces
{
    public class Knight : Piece
    {
        public const int Id = 1;
        public const char Letter = 'N';
        const int pieceValue = 325;
        public static readonly Coordinate[] Directions;
        private static readonly int[,] pieceSquareTable;
        public override int PieceValue => Color == Color.White ? pieceValue : -pieceValue;
        public override char NotationLetter => Letter;
        public override Coordinate[] PieceDirection => Directions;
        public override int[,] PieceSquareTable => pieceSquareTable;
        public override int Number => Id;
        public override bool Sliding => false;

        static Knight()
        {
            Directions = [
                    new Coordinate(2, 1),
                    new Coordinate(2, -1),
                    new Coordinate(-2, 1),
                    new Coordinate(-2, -1),
                    new Coordinate(1, -2),
                    new Coordinate(-1, -2),
                    new Coordinate(1, 2),
                    new Coordinate(-1, 2)
            ];

            pieceSquareTable = new int[,]
            {
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
    }
}
