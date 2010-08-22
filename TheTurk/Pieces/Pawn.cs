using System.Collections.Generic;
using ChessEngine.Main;
using ChessEngine.Moves;

namespace ChessEngine.Pieces
{
    public class Pawn : Piece
    {
        public const int pawn = 0;
        public const char Letter = ' ';
        public const int Piecevalue = 100;
        public static readonly int[,] pieceSquareTable;
        static Pawn()
        {
            pieceSquareTable = new int[,]{    
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
            get { return Color == Color.White ? Piecevalue : -Piecevalue; }
        }

        public override Coordinate[] PieceDirection
        {
            get { return null; }
        }
        public override List<Move> GenerateMoves(Board board)
        {
            var moves = new List<Move>();
            Move move;
            if (Color == Color.White)
            {
                var upSquare = From.To(Coordinate.Directions.North);
                if (upSquare.IsEmpty(board) && From.Rank != 7)//Pawn can go up if it is empty square
                {
                    move = new Ordinary(board, this, upSquare);
                    moves.Add(move);
                    var twoUpSquare = upSquare.To(Coordinate.Directions.North);
                    if (From.Rank == 2 && twoUpSquare.IsEmpty(board))//Check can jump 2 square when it is on rank 2
                    {
                        move = new Ordinary(board, this, twoUpSquare);
                        moves.Add(move);
                    }
                }

                if (From.Rank != 7)
                {
                    Coordinate crossSquare = From.To(Coordinate.Directions.NorthEast);
                    //Check for capture
                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).Color == Color.Black)
                    {
                        move = new Ordinary(board, this, crossSquare);
                        moves.Add(move);
                    }
                    crossSquare = From.To(Coordinate.Directions.NorthWest);
                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).Color == Color.Black)
                    {
                        move = new Ordinary(board, this, crossSquare);
                        moves.Add(move);
                    }
                }
                if (From.Rank == 5)//Check possibility of enpassant move
                {
                    var crossSquare = From.To(Coordinate.Directions.NorthEast);
                    if (crossSquare.IsOnboard() && crossSquare.Equals(board.EnPassantSquare))
                    {
                        move = new EnPassant(board, this, crossSquare);
                        moves.Add(move);
                    }
                    else
                    {
                        crossSquare = From.To(Coordinate.Directions.NorthWest);
                        if (crossSquare.IsOnboard() && crossSquare.Equals(board.EnPassantSquare))
                        {
                            move = new EnPassant(board, this, crossSquare);
                            moves.Add(move);
                        }
                    }
                }
                if (From.Rank == 7)//Check pawn promotions
                {
                    upSquare = From.To(Coordinate.Directions.North);
                    if (upSquare.IsEmpty(board))
                    {
                        moves.AddRange(Promote.AllPossiblePromotions(board, this, upSquare));

                    }
                    var crossSquare = From.To(Coordinate.Directions.NorthEast);
                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).Color == Color.Black)
                    {
                        moves.AddRange(Promote.AllPossiblePromotions(board, this, crossSquare));
                    }
                    crossSquare = From.To(Coordinate.Directions.NorthWest);
                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).Color == Color.Black)
                    {
                        moves.AddRange(Promote.AllPossiblePromotions(board, this, crossSquare));
                    }
                }

            }
            else // For black pawn
            {
                var downSquare = From.To(Coordinate.Directions.South);
                if (downSquare.IsEmpty(board) && From.Rank != 2)
                {
                    move = new Ordinary(board, this, downSquare);
                    moves.Add(move);
                    var twoDownSquare = downSquare.To(Coordinate.Directions.South);

                    if (From.Rank == 7 && twoDownSquare.IsEmpty(board))
                    {
                        move = new Ordinary(board, this, twoDownSquare);
                        moves.Add(move);
                    }
                }

                if (From.Rank != 2)
                {
                    var crossSquare = From.To(Coordinate.Directions.SouthEast);
                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) &&
                        crossSquare.GetPiece(board).Color == Color.White)
                    {
                        move = new Ordinary(board, this, crossSquare);
                        moves.Add(move);
                    }
                    crossSquare = From.To(Coordinate.Directions.SouthWest);
                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) &&
                        crossSquare.GetPiece(board).Color == Color.White)
                    {
                        move = new Ordinary(board, this, crossSquare);
                        moves.Add(move);
                    }
                }
                if (From.Rank == 4)
                {
                    var crossSquare = From.To(Coordinate.Directions.SouthEast);
                    if (crossSquare.IsOnboard() && crossSquare.Equals(board.EnPassantSquare))
                    {
                        move = new EnPassant(board, this, crossSquare);
                        moves.Add(move);
                    }
                    else
                    {
                        crossSquare = From.To(Coordinate.Directions.SouthWest);
                        if (crossSquare.IsOnboard() && crossSquare.Equals(board.EnPassantSquare))
                        {
                            move = new EnPassant(board, this, crossSquare);
                            moves.Add(move);
                        }
                    }
                }
                if (From.Rank == 2)
                {
                    downSquare = From.To(Coordinate.Directions.South);
                    if (downSquare.IsEmpty(board))
                    {
                        moves.AddRange(Promote.AllPossiblePromotions(board, this, downSquare));

                    }
                    var crossSquare = From.To(Coordinate.Directions.SouthEast);
                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).Color == Color.White)
                    {
                        moves.AddRange(Promote.AllPossiblePromotions(board, this, crossSquare));
                    }
                    crossSquare = From.To(Coordinate.Directions.SouthWest);
                    if (crossSquare.IsOnboard() && !crossSquare.IsEmpty(board) && crossSquare.GetPiece(board).Color == Color.White)
                    {
                        moves.AddRange(Promote.AllPossiblePromotions(board, this, crossSquare));
                    }
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
            get { return pawn; }
        }
    }
}
