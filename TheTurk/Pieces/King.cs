using System.Collections.Generic;
using TheTurk.Engine;
using TheTurk.Moves;

namespace TheTurk.Pieces
{
    public class King : Piece
    {
        public const int Id = 5;
        public const char Letter = 'K';
        const int pieceValue = 1000000;
        public override int Number => Id;

        public override bool Sliding => false;
        public override char NotationLetter => Letter;
        public override int PieceValue => Color == Color.White ? pieceValue : -pieceValue;
        public override Coordinate[] PieceDirection => Coordinate.allDirectionDelta;
        public override int[,] PieceSquareTable => pieceSquareTable;
        private static readonly int[,] pieceSquareTable;
        static King()
        {
            pieceSquareTable = new int[,] {
                                            {-40, -40, -40, -40, -40, -40, -40, -40},
                                            {-40, -40, -40, -40, -40, -40, -40, -40},
                                            {-40, -40, -40, -40, -40, -40, -40, -40},
                                            {-40, -40, -40, -40, -40, -40, -40, -40},
                                            {-40, -40, -40, -40, -40, -40, -40, -40},
                                            {-40, -40, -40, -40, -40, -40, -40, -40},
                                            {-15, -15, -20, -20, -20, -20, -15, -15},
                                            {  0,  20,  30, -30,   0, -20,  30,  20}
                                          };
        }

        public King(Coordinate from, Color color)
            : base(from, color)
        {

        }

        public override IEnumerable<Move> GenerateMoves(Board board)
        {
            return base.GenerateMoves(board).Concat(CreateCastleMoves(board));
        }

        IEnumerable<Move> CreateCastleMoves(Board board)
        {
            var castleSide = this.Color == Pieces.Color.White ? board.WhiteCastle : board.BlackCastle;
            
            if ((castleSide == Castle.BothCastle || castleSide == Castle.ShortCastle) && ShortCastle.Available(board, this.Color))
                yield return new ShortCastle(this);
            
            if ((castleSide == Castle.BothCastle || castleSide == Castle.LongCastle) && LongCastle.Available(board, this.Color))
                yield return new LongCastle(this);
        }
    }
}
