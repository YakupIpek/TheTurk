using System.Collections.Generic;
using ChessEngine.Main;
using ChessEngine.Moves;

namespace ChessEngine.Pieces
{
    public class King : Piece
    {
        private const int king = 5;
        public const char Letter = 'K';
        const int pieceValue = 1000000;
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
        public override bool Sliding
        {
            get
            {
                return false;
            }
        }
        public override char NotationLetter
        {
            get { return Letter; }
        }

        public override int PieceValue
        {
            get { return Color == Color.White ? pieceValue : -pieceValue; }
        }
        public override Coordinate[] PieceDirection
        {
            get
            {
                return Coordinate.allDirectionDelta;
            }
        }
        public override List<Move> GenerateMoves(Board board)
        {
            var moves = base.GenerateMoves(board);

            moves.AddRange(CreateCastleMoves(board));
            return moves;
        }
        List<Move> CreateCastleMoves(Board board)
        {
            var moves = new List<Move>();
            var castleSide = this.Color == Pieces.Color.White ? board.WhiteCastle : board.BlackCastle;
            if (castleSide == Board.Castle.BothCastle || castleSide == Board.Castle.ShortCastle)
            {
                if (ShortCastle.Available(board, this.Color))
                {
                    moves.Add(new ShortCastle(this));

                }
            }
            if (castleSide == Board.Castle.BothCastle || castleSide == Board.Castle.LongCastle)
            {
                if (LongCastle.Available(board, this.Color))
                {
                    moves.Add(new LongCastle(this));
                }
            }


            return moves;
        }
        public override int[,] PieceSquareTable
        {
            get { return pieceSquareTable; }
        }

        public override int ToInt
        {
            get { return king; }
        }
    }
}
