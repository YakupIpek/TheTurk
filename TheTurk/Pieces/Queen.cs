using TheTurk.Engine;

namespace TheTurk.Pieces
{
    public class Queen : Piece
    {
        public const int Id = 4;
        public const char Letter = 'Q';
        const int pieceValue = 900;
        private static readonly int[,] pieceSquareTable;
        public override int PieceValue => Color == Color.White ? pieceValue : -pieceValue;
        public override char NotationLetter => Letter;
        public override Coordinate[] PieceDirection => Coordinate.allDirectionDelta;
        public override int[,] PieceSquareTable => pieceSquareTable;
        public override int Number => Id;

        static Queen()
        {
            pieceSquareTable = new int[,]
            {
                    { 0,   0,   0,   0,   0,   0,   0,   0},
                    { 0,   0,   1,   1,   1,   1,   0,   0},
                    { 0,   0,   1,   2,   2,   1,   0,   0},
                    { 0,   0,   2,   3,   3,   2,   0,   0},
                    { 0,   0,   2,   3,   3,   2,   0,   0},
                    { 0,   0,   1,   2,   2,   1,   0,   0},
                    { 0,   0,   1,   1,   1,   1,   0,   0},
                    {-5,  -5,  -5,  -5,  -5,  -5,  -5,  -5}
            };
        }

        public Queen(Coordinate from, Color color)
            : base(from, color)
        {
        }

    }
}
