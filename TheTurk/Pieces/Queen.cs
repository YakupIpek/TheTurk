namespace ChessEngine.Pieces
{
    public class Queen : Piece
    {
        public const char letter = 'Q';
        static readonly int pieceValue=900;

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
            get { return letter; }
        }
        public override Coordinate[] PieceDirection
        {
            get
            {
                return Coordinate.allDirectionDelta;
            }
        }
        
    }
}
