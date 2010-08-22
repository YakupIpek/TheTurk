using ChessEngine.Main;

namespace ChessEngine.Pieces
{
    public class Rook : Piece
    {
        public const int rook = 3;
        public const char Letter = 'R';
        const int pieceValue = 500;
        private static readonly int[,] pieceSquareTable;

        static Rook()
        {
            pieceSquareTable = new int[,]
                                         {
                                          { 5,   5,   5,   5,   5,   5,   5,   5},
                                          {20,  20,  20,  20,  20,  20,  20,  20},
                                          {-5,   0,   0,   0,   0,   0,   0,  -5},
                                          {-5,   0,   0,   0,   0,   0,   0,  -5},
                                          {-5,   0,   0,   0,   0,   0,   0,  -5},
                                          {-5,   0,   0,   0,   0,   0,   0,  -5},
                                          {-5,   0,   0,   0,   0,   0,   0,  -5},
                                          { 0,   0,   0,   2,   2,   0,   0,   0}
                                          };
        }
        public Rook(Coordinate from, Color color)
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
                return Coordinate.fourDirectionDelta;
            }
        }
        public override int[,] PieceSquareTable
        {
            get { return pieceSquareTable; }
        }

        public override int ToInt
        {
            get { return rook; }
        }
    }
}
