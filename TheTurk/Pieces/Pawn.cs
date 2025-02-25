using System.Collections.Generic;
using TheTurk.Engine;
using TheTurk.Moves;

namespace TheTurk.Pieces
{
    public class Pawn : Piece
    {
        public const int Id = 0;
        public const char Letter = ' ';
        public const int Piecevalue = 100;
        public static readonly int[,] pieceSquareTable;

        static Pawn()
        {
            pieceSquareTable = new int[,]
            {
                    {  0,   0,   0,   0,   0,   0,   0,   0},
                    { -6,  -4,   1,   1,   1,   1,  -4,  -6},
                    { -6,  -4,   1,   2,   2,   1,  -4,  -6},
                    { -6,  -4,   2,   8,   8,   2,  -4,  -6},
                    { -6,  -4,   5,  10,  10,   5,  -4,  -6},
                    { -4,  -4,   1,   5,   5,   1,  -4,  -4},
                    { -6,  -4,   1, -24, -24,   1,  -4,  -6},
                    {  0,   0,   0,   0,   0,   0,   0,   0}
            };
        }

        public Pawn(Coordinate from, Color color)
            : base(from, color)
        {
        }

        public override bool Sliding => false;
        public override char NotationLetter => Letter;
        public override int PieceValue => Color == Color.White ? Piecevalue : -Piecevalue;
        public override Coordinate[] PieceDirection => null;
        public override int[,] PieceSquareTable => pieceSquareTable;
        public override int Number => Id;

        public override IEnumerable<Move> GenerateMoves(Board board)
        {
            if (Color == Color.White)
            {
                var upSquare = From.To(Coordinate.Directions.North);
                if (upSquare.IsEmpty(board) && From.Rank != 7) // Pawn can go up if it is empty square
                {
                    yield return new Ordinary(board, this, upSquare);
                    var twoUpSquare = upSquare.To(Coordinate.Directions.North);
                    if (From.Rank == 2 && twoUpSquare.IsEmpty(board)) // Check can jump 2 square when it is on rank 2
                    {
                        yield return new Ordinary(board, this, twoUpSquare);
                    }
                }

                if (From.Rank != 7)
                {
                    var crossSquare = From.To(Coordinate.Directions.NorthEast);
                    // Check for capture
                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).Color == Color.Black)
                        yield return new Ordinary(board, this, crossSquare);

                    crossSquare = From.To(Coordinate.Directions.NorthWest);

                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).Color == Color.Black)
                        yield return new Ordinary(board, this, crossSquare);
                }

                if (From.Rank == 5) // Check possibility of enpassant move
                {
                    var crossSquare = From.To(Coordinate.Directions.NorthEast);

                    if (crossSquare.Equals(board.EnPassantSquare))
                        yield return new EnPassant(board, this, crossSquare);
                    else
                    {
                        crossSquare = From.To(Coordinate.Directions.NorthWest);

                        if (crossSquare.Equals(board.EnPassantSquare))
                            yield return new EnPassant(board, this, crossSquare);
                    }
                }

                if (From.Rank == 7) // Check pawn promotions
                {
                    if (upSquare.IsEmpty(board))
                    {
                        foreach (var move in Promote.AllPossiblePromotions(board, this, upSquare))
                        {
                            yield return move;
                        }
                    }

                    var crossSquare = From.To(Coordinate.Directions.NorthEast);

                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).Color == Color.Black)
                    {
                        foreach (var move in Promote.AllPossiblePromotions(board, this, crossSquare))
                        {
                            yield return move;
                        }
                    }

                    crossSquare = From.To(Coordinate.Directions.NorthWest);

                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).Color == Color.Black)
                    {
                        foreach (var move in Promote.AllPossiblePromotions(board, this, crossSquare))
                        {
                            yield return move;
                        }
                    }
                }
            }
            else // For black pawn
            {
                var downSquare = From.To(Coordinate.Directions.South);
                if (downSquare.IsEmpty(board) && From.Rank != 2)
                {
                    yield return new Ordinary(board, this, downSquare);

                    var twoDownSquare = downSquare.To(Coordinate.Directions.South);

                    if (From.Rank == 7 && twoDownSquare.IsEmpty(board))
                        yield return new Ordinary(board, this, twoDownSquare);
                }

                if (From.Rank != 2)
                {
                    var crossSquare = From.To(Coordinate.Directions.SouthEast);

                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).Color == Color.White)
                        yield return new Ordinary(board, this, crossSquare);

                    crossSquare = From.To(Coordinate.Directions.SouthWest);

                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).Color == Color.White)
                        yield return new Ordinary(board, this, crossSquare);
                }

                if (From.Rank == 4)
                {
                    var crossSquare = From.To(Coordinate.Directions.SouthEast);

                    if (crossSquare.Equals(board.EnPassantSquare))
                        yield return new EnPassant(board, this, crossSquare);
                    else
                    {
                        crossSquare = From.To(Coordinate.Directions.SouthWest);
                        if (crossSquare.Equals(board.EnPassantSquare))
                            yield return new EnPassant(board, this, crossSquare);
                    }
                }

                if (From.Rank == 2)
                {
                    downSquare = From.To(Coordinate.Directions.South);

                    if (downSquare.IsEmpty(board))
                    {
                        foreach (var move in Promote.AllPossiblePromotions(board, this, downSquare))
                        {
                            yield return move;
                        }
                    }

                    var crossSquare = From.To(Coordinate.Directions.SouthEast);

                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).Color == Color.White)
                    {
                        foreach (var move in Promote.AllPossiblePromotions(board, this, crossSquare))
                        {
                            yield return move;
                        }
                    }

                    crossSquare = From.To(Coordinate.Directions.SouthWest);

                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).Color == Color.White)
                    {
                        foreach (var move in Promote.AllPossiblePromotions(board, this, crossSquare))
                        {
                            yield return move;
                        }
                    }
                }
            }
        }
    }
}
