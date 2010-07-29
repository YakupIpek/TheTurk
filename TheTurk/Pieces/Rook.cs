using ChessEngine.Main;

namespace ChessEngine.Pieces
{
    public class Rook : Piece
    {
        public const char Letter = 'R';
        const int pieceValue=500;

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
    }
}
