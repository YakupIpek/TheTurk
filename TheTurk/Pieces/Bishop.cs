using TheTurk.Engine;

namespace TheTurk.Pieces
{
    public class Bishop : Piece
    {
        public const int Id = 2;
        public const char Letter = 'B';
        const int pieceValue = 330;
        public override int PieceValue => Color == Color.White ? pieceValue : -pieceValue;
        public override char NotationLetter => Letter;
        public override Coordinate[] PieceDirection => Coordinate.crossFourDirectionDelta;
        public override int[,] PieceSquareTable => pieceSquareTable;
        public override int Number => Id;

        private static readonly int[,] pieceSquareTable;

        static Bishop()
        {
            pieceSquareTable = new int[,]{
                                                 {-4,  -4,  -4,  -4,  -4,  -4,  -4,  -4},
                                                 {-4,   0,   0,   0,   0,   0,   0,  -4},
                                                 {-4,   0,   2,   4,   4,   2,   0,  -4},
                                                 {-4,   0,   4,   6,   6,   4,   0,  -4},
                                                 {-4,   0,   4,   6,   6,   4,   0,  -4},
                                                 {-4,   1,   2,   4,   4,   2,   1,  -4},
                                                 {-4,   2,   1,   1,   1,   1,   2,  -4},
                                                 {-4,  -4, -12,  -4,  -4, -12,  -4,  -4}
                                                };
        }


        public Bishop(Coordinate from, Color color)
            : base(from, color)
        {
        }
    }
}
