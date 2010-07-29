using System.Collections.Generic;
using ChessEngine.Main;
using ChessEngine.Moves;

namespace ChessEngine.Pieces
{
    public class Knight : Piece
    {
        const int pieceValue = 325;
        public const char Letter = 'N';
        public static readonly Coordinate[] Directions;
        static Knight()
        {
            Directions = new Coordinate[]{ new Coordinate(2,1),new Coordinate(2,-1),new Coordinate(-2,1),new Coordinate(-2,-1),
            new Coordinate(1,-2),new Coordinate(-1,-2),new Coordinate(1,2),new Coordinate(-1,2)};
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
        //public override System.Collections.Generic.List<Moves.Move> GenerateMoves(Board board)
        //{
        //    if (!board.IsInCheck())
        //    {
        //        RemoveMe(board);
        //        if (!board.IsInCheck())
        //        {
        //            PutMe(board);
        //            return new List<Move>();
        //        }
        //        PutMe(board);
        //    }

        //    return base.GenerateMoves(board);
        //}
    }
}
