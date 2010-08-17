using ChessEngine.Main;

namespace ChessEngine.Pieces
{
    public class Bishop : Piece
    {
        private const int bishop = 2;
        public const char Letter = 'B';
        const int pieceValue = 325;
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
        public override int PieceValue
        {
            get { return Color == Color.White ? pieceValue : -pieceValue; }
        }
        public override char NotationLetter
        {
            get { return Letter; }
        }

        public override Coordinate[] PieceDirection
        {
            get { return Coordinate.crossFourDirectionDelta; }
        }
        public override int[,] PieceSquareTable
        {
            get { return pieceSquareTable; }
        }

        public override int ToInt
        {
            get { return bishop; }
        }
    }
}
