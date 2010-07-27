using System.Collections.Generic;
using ChessEngine.Main;
using ChessEngine.Moves;

namespace ChessEngine.Pieces
{
    public class Pawn : Piece
    {
        public const char letter = ' ';
        static readonly int piecevalue = 100;

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
            get { return letter; }
        }
        public override int PieceValue
        {
            get { return Color == Color.White ? piecevalue : -piecevalue; }
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
                if (upSquare.IsEmpty(board) && From.rank != 7)//Pawn can go up if it is empty square
                {
                    move = new Ordinary(board, this, upSquare);
                    moves.Add(move);
                    var twoUpSquare = upSquare.To(Coordinate.Directions.North);
                    if (From.rank == 2 && twoUpSquare.IsEmpty(board))//Check can jump 2 square when it is on rank 2
                    {
                        move = new Ordinary(board, this, twoUpSquare);
                        moves.Add(move);
                    }
                }

                if (From.rank!=7)
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
                if (From.rank == 5)//Check possibility of enpassant move
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
                if (From.rank == 7)//Check pawn promotions
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
                if (downSquare.IsEmpty(board) && From.rank != 2)
                {
                    move = new Ordinary(board, this, downSquare);
                    moves.Add(move);
                    var twoDownSquare = downSquare.To(Coordinate.Directions.South);

                    if (From.rank == 7 && twoDownSquare.IsEmpty(board))
                    {
                        move = new Ordinary(board, this, twoDownSquare);
                        moves.Add(move);
                    }
                }

                if (From.rank!=2)
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
                if (From.rank == 4)
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
                if (From.rank == 2)
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
    }
}
