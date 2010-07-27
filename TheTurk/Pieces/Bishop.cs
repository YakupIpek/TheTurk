using ChessEngine.Main;

namespace ChessEngine.Pieces
{
    public class Bishop : Piece
    {
        public const char Letter = 'B';
        const int pieceValue=325;

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



    }
}
