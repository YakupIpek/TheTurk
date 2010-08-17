using ChessEngine.Main;

namespace ChessEngine.Pieces
{
    public class Queen : Piece
    {
        private const int queen = 4;
        public const char Letter = 'Q';
        const int pieceValue = 900;
        private static readonly int[,] pieceSquareTable;
        static Queen()
        {
            pieceSquareTable = new int[,] {
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
            get
            {
                return Coordinate.allDirectionDelta;
            }
        }
        public override int[,] PieceSquareTable
        {
            get { return pieceSquareTable; }
        }

        public override int ToInt
        {
            get { return queen; }
        }
    }
}
